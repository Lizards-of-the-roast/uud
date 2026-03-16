using System.Collections.Generic;
using System.Linq;
using Core.Meta.MainNavigation.Challenge;
using MTGA.Social;
using UnityEngine;
using Wizards.Mtga.PrivateGame;
using Wotc.Mtga.Extensions;

public class SocialMessagesView : MonoBehaviour
{
	[SerializeField]
	private Transform _messageContainer;

	[SerializeField]
	private MessageTile _prefabMessageIncoming;

	[SerializeField]
	private MessageTile _prefabMessageOutgoing;

	private Dictionary<SocialMessage, MessageTile> _tilesByMessage;

	private SocialUI _socialUI;

	private ISocialManager _socialManager;

	private PVPChallengeController _challengeController;

	private bool _initialized;

	private readonly Dictionary<SocialMessage, MessageTile> _activeMessages = new Dictionary<SocialMessage, MessageTile>();

	private readonly Stack<MessageTile> _inactiveMessagesIncoming = new Stack<MessageTile>();

	private readonly Stack<MessageTile> _inactiveMessagesOutgoing = new Stack<MessageTile>();

	private ChatManager _chatManager => _socialManager.ChatManager;

	public bool Initialized => _initialized;

	public void Init(SocialUI socialUI, ISocialManager socialManager, PVPChallengeController challengeController, List<SocialMessage> pendingMessages = null)
	{
		if (_socialUI != null)
		{
			OnDestroy();
		}
		_socialUI = socialUI;
		_socialManager = socialManager;
		_challengeController = challengeController;
		_socialManager.FriendPresenceChanged += FriendPresenceChanged;
		_chatManager.ConversationSelected += ConversationSelected;
		_chatManager.MessageAdded += MessageAdded;
		_chatManager.NotificationAlert += NotificationAlert;
		_chatManager.NotificationUpdate += NotificationUpdate;
		_chatManager.NotificationCancel += NotificationCancel;
		_initialized = true;
		ConversationSelected(null, _chatManager.CurrentConversation);
		if (pendingMessages == null)
		{
			return;
		}
		foreach (SocialMessage pendingMessage in pendingMessages)
		{
			NotificationAlert(pendingMessage);
		}
	}

	private void OnDestroy()
	{
		_socialManager.FriendPresenceChanged -= FriendPresenceChanged;
		_chatManager.ConversationSelected -= ConversationSelected;
		_chatManager.MessageAdded -= MessageAdded;
		_chatManager.NotificationAlert -= NotificationAlert;
		_chatManager.NotificationUpdate -= NotificationUpdate;
		_chatManager.NotificationCancel -= NotificationCancel;
	}

	private void ConversationSelected(Conversation prevConversation, Conversation currentConversation)
	{
		MessageTile[] array = _activeMessages.Values.ToArray();
		foreach (MessageTile messageTile in array)
		{
			RemoveMessageTile(messageTile);
		}
		_activeMessages.Clear();
		if (currentConversation != null)
		{
			for (int j = Mathf.Max(currentConversation.MessageHistory.Count - 40, 0); j < currentConversation.MessageHistory.Count; j++)
			{
				SocialMessage socialMessage = currentConversation.MessageHistory[j];
				AddMessageTile(socialMessage);
				socialMessage.Seen = MessageSeen.InChat;
			}
			_socialUI.UpdateCornerIconAnimatorState();
		}
	}

	private void FriendPresenceChanged(SocialEntity friend)
	{
		if (!friend.Equals(_chatManager.CurrentConversation?.Friend))
		{
			return;
		}
		foreach (MessageTile value in _activeMessages.Values)
		{
			value.Init(value.Message);
		}
	}

	private void MessageAdded(SocialMessage message, Conversation conversation)
	{
		if (_socialUI.ChatVisible && _chatManager.CurrentConversation == conversation && !(_messageContainer == null))
		{
			AddMessageTile(message);
			message.Seen = MessageSeen.InChat;
		}
	}

	private void NotificationAlert(SocialMessage notification)
	{
		if (_socialUI.ChatVisible || _messageContainer == null || _socialManager.LocalPlayer.Status == PresenceStatus.Busy || (notification.Type == MessageType.Challenge && (_challengeController.ChallengePermissionState == ChallengeDataProvider.ChallengePermissionState.Restricted_ChallengesKillswitched || _challengeController.ChallengePermissionState == ChallengeDataProvider.ChallengePermissionState.Restricted_InMatch)))
		{
			return;
		}
		MessageTile messageTile = AddMessageTile(notification);
		switch (notification.Type)
		{
		case MessageType.Invite:
			messageTile.NotificationClicked += delegate(SocialMessage m)
			{
				RemoveMessage(m);
				_socialUI.ShowSocialEntitiesList();
			};
			messageTile.Disabled += RemoveMessageTile;
			break;
		case MessageType.Challenge:
			messageTile.NotificationClicked += delegate
			{
				_socialUI.ShowSocialEntitiesList();
			};
			messageTile.Disabled += RemoveMessageTile;
			break;
		case MessageType.Chat:
			messageTile.NotificationClicked += delegate(SocialMessage m)
			{
				_socialUI.ShowChatWindow(m.Friend);
			};
			messageTile.Disabled += RemoveMessageTile;
			break;
		case MessageType.LobbyInvite:
			messageTile.NotificationClicked += delegate(SocialMessage m)
			{
				RemoveMessage(m);
				_socialUI.TablesCornerUI.ShowTablesList();
			};
			messageTile.Disabled += RemoveMessageTile;
			break;
		case MessageType.LobbyChat:
			messageTile.NotificationClicked += delegate(SocialMessage m)
			{
				_socialUI.TablesCornerUI.OpenSpecificTableView(m.Lobby);
			};
			messageTile.Disabled += RemoveMessageTile;
			break;
		case MessageType.TournamentIsReady:
			messageTile.NotificationClicked += RemoveMessage;
			messageTile.Disabled += RemoveMessageTile;
			break;
		case MessageType.TournamentRoundIsReady:
			messageTile.NotificationClicked += RemoveMessage;
			messageTile.Disabled += RemoveMessageTile;
			break;
		}
	}

	private void AcceptClick(SocialMessage notification)
	{
		switch (notification.Type)
		{
		case MessageType.Invite:
			_socialManager.AcceptFriendInviteIncoming(notification.Friend);
			break;
		case MessageType.Challenge:
			_socialUI.AcceptChallenge(notification.Friend, notification.ChallengeId);
			break;
		case MessageType.TournamentIsReady:
			_socialUI.ReadyForTournamentStart();
			break;
		case MessageType.TournamentRoundIsReady:
			_socialUI.ReadyForTournamentRound();
			break;
		}
		if (!notification.Friend.Equals(_chatManager.CurrentConversation?.Friend))
		{
			RemoveMessage(notification);
		}
	}

	private void NotificationUpdate(SocialMessage notification)
	{
		if (_activeMessages.ContainsKey(notification))
		{
			_activeMessages[notification].UpdateNotification();
		}
	}

	private void NotificationCancel(SocialMessage notification)
	{
		if (_activeMessages.ContainsKey(notification))
		{
			_activeMessages[notification].UpdateNotification();
		}
	}

	private MessageTile AddMessageTile(SocialMessage message)
	{
		MessageTile messageTile = null;
		Stack<MessageTile> stack = ((message.Direction == Direction.Incoming) ? _inactiveMessagesIncoming : _inactiveMessagesOutgoing);
		if (_activeMessages.ContainsKey(message))
		{
			messageTile = _activeMessages[message];
			messageTile.gameObject.UpdateActive(active: false);
		}
		else if (stack.Count > 0)
		{
			messageTile = stack.Pop();
		}
		else
		{
			messageTile = Object.Instantiate((message.Direction == Direction.Incoming) ? _prefabMessageIncoming : _prefabMessageOutgoing, _messageContainer);
			messageTile.AcceptClicked += AcceptClick;
		}
		messageTile.gameObject.UpdateActive(active: true);
		messageTile.transform.SetSiblingIndex(int.MaxValue);
		messageTile.Init(message, !_socialUI.ChatVisible);
		_activeMessages[message] = messageTile;
		return messageTile;
	}

	private void RemoveMessage(SocialMessage message)
	{
		if (_activeMessages.TryGetValue(message, out var value))
		{
			RemoveMessageTile(value);
		}
	}

	private void RemoveMessageTile(MessageTile messageTile)
	{
		if (messageTile.Message != null && _activeMessages.ContainsKey(messageTile.Message))
		{
			_activeMessages.Remove(messageTile.Message);
		}
		SocialMessage message = messageTile.Message;
		((message != null && message.Direction == Direction.Outgoing) ? _inactiveMessagesOutgoing : _inactiveMessagesIncoming).Push(messageTile);
		messageTile.Cleanup();
		messageTile.gameObject.UpdateActive(active: false);
	}
}
