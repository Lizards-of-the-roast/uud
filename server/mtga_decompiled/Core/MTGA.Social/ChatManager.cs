using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;
using UnityEngine;
using Wizards.Mtga;
using Wizards.Mtga.PrivateGame;

namespace MTGA.Social;

public class ChatManager
{
	private ISocialManager _socialManager;

	public readonly List<SocialMessage> Notifications = new List<SocialMessage>();

	public readonly List<Conversation> Conversations = new List<Conversation>();

	private int _cycleIndex = -1;

	private readonly List<Conversation> _selectionHistory = new List<Conversation>();

	private readonly HashAlgorithm _hashAlgorithm = new SHA1CryptoServiceProvider();

	private ChallengeDataProvider _challengeDataProvider;

	public Conversation CurrentConversation { get; private set; }

	public event Action<SocialMessage> NotificationAlert;

	public event Action<SocialMessage> NotificationUpdate;

	public event Action<SocialMessage> NotificationCancel;

	public event Action<Conversation, Conversation> ConversationSelected;

	public event Action<SocialMessage, Conversation> MessageAdded;

	public ChatManager(ISocialManager socialManager)
	{
		_socialManager = socialManager;
		_challengeDataProvider = Pantry.Get<ChallengeDataProvider>();
		_challengeDataProvider.RegisterForChallengeChanges(HandleChallengeUpdates);
	}

	~ChatManager()
	{
		if (_challengeDataProvider != null)
		{
			_challengeDataProvider.UnRegisterForChallengeChanges(HandleChallengeUpdates);
		}
	}

	public void Init()
	{
		_socialManager.OnDirectMessage += ChatMessageReceived;
		_socialManager.FriendRemoved += FriendRemoved;
		_socialManager.InviteIncomingAdded += InviteIncomingAdded;
		_socialManager.InviteIncomingRemoved += InviteIncomingRemoved;
	}

	private void HandleChallengeUpdates(PVPChallengeData challengeData)
	{
		challengeData.Invites.TryGetValue(challengeData.LocalPlayerId, out var value);
		if (value == null)
		{
			value = challengeData.Invites?.FirstOrDefault((KeyValuePair<string, ChallengeInvite> pair) => pair.Value.Sender.FullDisplayName == challengeData.LocalPlayerDisplayName).Value;
		}
		if (value != null)
		{
			if (challengeData.Status == ChallengeStatus.Removed || value.Status == InviteStatus.Cancelled)
			{
				ChallengeNotificationCanceled(challengeData, value.Sender.FullDisplayName);
			}
			else if (value.Status == InviteStatus.None || value.Status == InviteStatus.Sent)
			{
				ChallengeNotificationSentOrReceived(challengeData, value);
			}
			else
			{
				ChallengeNotificationUpdate(challengeData);
			}
		}
	}

	public void Destroy()
	{
		_socialManager.OnDirectMessage -= ChatMessageReceived;
		_socialManager.FriendRemoved -= FriendRemoved;
		if (_challengeDataProvider != null)
		{
			_challengeDataProvider.UnRegisterForChallengeChanges(HandleChallengeUpdates);
		}
		_socialManager.InviteIncomingAdded -= InviteIncomingAdded;
		_socialManager.InviteIncomingRemoved -= InviteIncomingRemoved;
		Conversations.Clear();
		_selectionHistory.Clear();
		Notifications.Clear();
	}

	public Conversation SelectConversation(SocialEntity friend, bool cycled = false)
	{
		Conversation currentConversation = CurrentConversation;
		CurrentConversation = GetConversation(friend, autoCreate: true);
		if (CurrentConversation == currentConversation)
		{
			return CurrentConversation;
		}
		if (CurrentConversation != null)
		{
			CurrentConversation.HasUnseenChallenge = false;
			CurrentConversation.HasUnseenMessages = false;
			CurrentConversation.Friend.HasUnseenMessages = false;
			if (CurrentConversation != null)
			{
				_socialManager.BIMessage_ChatWindowOpened();
				_selectionHistory.Remove(CurrentConversation);
				_selectionHistory.Add(CurrentConversation);
			}
			if (!cycled)
			{
				_cycleIndex = -1;
			}
		}
		else
		{
			_cycleIndex = -1;
		}
		this.ConversationSelected?.Invoke(currentConversation, CurrentConversation);
		if (currentConversation != null && currentConversation.MessageHistory.Count == 0 && string.IsNullOrEmpty(currentConversation.MessageDraft))
		{
			Conversations.Remove(currentConversation);
		}
		return CurrentConversation;
	}

	public Conversation SelectNextConversation(bool reverse = false, bool unseenOnly = false)
	{
		Conversation conversation = null;
		if (reverse)
		{
			_selectionHistory.Remove(CurrentConversation);
			if (_selectionHistory.Count == 0)
			{
				return SelectConversation(null);
			}
			conversation = _selectionHistory[_selectionHistory.Count - 1];
			return SelectConversation(conversation.Friend);
		}
		if (_cycleIndex == -1)
		{
			Conversations.Sort(ConversationSort);
		}
		_cycleIndex++;
		while (_cycleIndex < Conversations.Count)
		{
			Conversation conversation2 = Conversations[_cycleIndex];
			if (conversation2 != CurrentConversation && (!unseenOnly || conversation2.HasUnseenMessages || conversation2.HasUnseenChallenge))
			{
				conversation = conversation2;
				break;
			}
			_cycleIndex++;
		}
		return SelectConversation(conversation?.Friend, cycled: true);
	}

	public List<Conversation> GetSortedConversations()
	{
		_cycleIndex = -1;
		Conversations.Sort(ConversationSort);
		return Conversations;
	}

	private int ConversationSort(Conversation c1, Conversation c2)
	{
		if (c1.Friend.IsOnline || c1.HasUnseenMessages)
		{
			if (!c2.Friend.IsOnline && !c2.HasUnseenMessages)
			{
				return -1;
			}
			return c2.ActivityTimestamp.CompareTo(c1.ActivityTimestamp);
		}
		if (!c2.Friend.IsOnline && !c2.HasUnseenMessages)
		{
			return c2.ActivityTimestamp.CompareTo(c1.ActivityTimestamp);
		}
		return 1;
	}

	public int ConversationsUnseenCount()
	{
		int num = 0;
		foreach (Conversation conversation in Conversations)
		{
			if (conversation.HasUnseenMessages)
			{
				num++;
			}
		}
		return num;
	}

	public bool IsChallengedInConversation()
	{
		foreach (Conversation conversation in Conversations)
		{
			if (conversation.HasUnseenChallenge)
			{
				return true;
			}
		}
		return false;
	}

	public Conversation GetConversation(SocialEntity arenaFriend, bool autoCreate = false)
	{
		if (arenaFriend == null)
		{
			return null;
		}
		Conversation conversation = Conversations.Find((Conversation c) => c.Friend.Equals(arenaFriend));
		if (conversation == null && autoCreate)
		{
			arenaFriend = _socialManager.Friends.Find((SocialEntity f) => f.Equals(arenaFriend));
			if (arenaFriend != null)
			{
				conversation = new Conversation(arenaFriend);
				string stringToHash = ((string.Compare(arenaFriend.SocialId, _socialManager.LocalPlayer.SocialId, StringComparison.Ordinal) < 0) ? (arenaFriend.SocialId + _socialManager.LocalPlayer.SocialId) : (_socialManager.LocalPlayer.SocialId + arenaFriend.SocialId));
				conversation.RelationshipId = HashString(stringToHash);
				Conversations.Insert(0, conversation);
			}
		}
		return conversation;
	}

	public void ManualNotificationAlert(SocialMessage socialMessage)
	{
		this.NotificationAlert?.Invoke(socialMessage);
	}

	internal string HashString(string stringToHash)
	{
		return BitConverter.ToString(_hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(stringToHash))).Replace("-", string.Empty);
	}

	public Conversation SendChatMessage(string message, SocialEntity friend = null)
	{
		Conversation conversation = ((friend != null) ? GetConversation(friend, autoCreate: true) : CurrentConversation);
		if (conversation != null)
		{
			conversation.SaveMessageDraft(null);
			SendPlatformMessage(conversation.Friend, message);
		}
		_socialManager.BIMessage_DirectMessageSent(conversation);
		return conversation;
	}

	private void AddSocialMessage(SocialMessage message)
	{
		Conversation conversation = GetConversation(message.Friend, autoCreate: true);
		if (conversation != null)
		{
			conversation.AddSocialMessage(message);
			if (message.Direction == Direction.Incoming && conversation != CurrentConversation)
			{
				_cycleIndex = -1;
				conversation.HasUnseenChallenge = message.Type == MessageType.Challenge && message.Seen != MessageSeen.InChat && !message.Canceled;
				bool flag = message.Type == MessageType.Chat && message.Seen != MessageSeen.InChat;
				conversation.HasUnseenMessages |= flag;
				conversation.Friend.HasUnseenMessages |= flag;
			}
			_socialManager.BIMessage_DirectMessageRecieved(conversation);
			this.MessageAdded?.Invoke(message, conversation);
		}
	}

	private void RemoveSocialMessage(SocialMessage message)
	{
		GetConversation(message.Friend)?.RemoveSocialMessage(message);
	}

	private void ChallengeNotificationSentOrReceived(PVPChallengeData challengeData, ChallengeInvite invite)
	{
		string fullDisplayName = invite.Sender.FullDisplayName;
		SocialMessage socialMessage = Notifications.Find((SocialMessage n) => !n.Canceled && n.Type == MessageType.Challenge && n.ChallengeId == challengeData.ChallengeId);
		if (socialMessage != null)
		{
			return;
		}
		SocialEntity friend = new SocialEntity(fullDisplayName);
		foreach (SocialEntity friend2 in _socialManager.Friends)
		{
			if (friend2.FullName == fullDisplayName)
			{
				friend = friend2;
				socialMessage = new SocialMessage(challengeData, friend, invite);
				AddSocialMessage(socialMessage);
			}
		}
		if (invite.Recipient.PlayerId == challengeData.LocalPlayerId)
		{
			if (socialMessage == null)
			{
				socialMessage = new SocialMessage(challengeData, friend, invite);
			}
			Notifications.Add(socialMessage);
			this.NotificationAlert?.Invoke(socialMessage);
			_socialManager.BIMessage_NotificationBubbleShown(socialMessage);
		}
	}

	private void ChallengeNotificationUpdate(PVPChallengeData challengeData)
	{
		Debug.Log($"[Social.Chat] Updating challenge from {challengeData.ChallengeId}: {challengeData.Status}");
		SocialMessage socialMessage = Notifications.Find((SocialMessage n) => !n.Canceled && n.Type == MessageType.Challenge && n.ChallengeId == challengeData.ChallengeId);
		if (socialMessage != null)
		{
			socialMessage.Canceled = !challengeData.Invites.ContainsKey(challengeData.LocalPlayerId) || challengeData.Invites.Where((KeyValuePair<string, ChallengeInvite> a) => a.Value.Recipient.PlayerId == challengeData.LocalPlayerId && a.Value.Status != InviteStatus.Sent).Any();
			Conversation conversation = GetConversation(socialMessage.Friend);
			if (conversation != null)
			{
				conversation.HasUnseenChallenge = socialMessage.Seen != MessageSeen.InChat && !socialMessage.Canceled;
			}
			this.NotificationUpdate?.Invoke(socialMessage);
		}
	}

	private void ChallengeNotificationCanceled(PVPChallengeData challengeData, string senderName)
	{
		Debug.Log($"[Social.Chat] Canceled challenge {challengeData.ChallengeId}: {challengeData.Status}");
		SocialMessage socialMessage = Notifications.Find((SocialMessage n) => !n.Canceled && n.Type == MessageType.Challenge && n.ChallengeId == challengeData.ChallengeId);
		if (socialMessage != null)
		{
			Conversation conversation = Conversations.Find((Conversation conversation2) => conversation2.Friend.FullName == senderName);
			socialMessage.Canceled = true;
			if (conversation != null)
			{
				conversation.HasUnseenChallenge = false;
			}
			this.NotificationCancel?.Invoke(socialMessage);
			Notifications.Remove(socialMessage);
		}
	}

	private void InviteIncomingAdded(Invite invite)
	{
		SocialMessage socialMessage = new SocialMessage(invite);
		Notifications.Add(socialMessage);
		_socialManager.BIMessage_NotificationBubbleShown(socialMessage);
		this.NotificationAlert?.Invoke(socialMessage);
	}

	private void InviteIncomingRemoved(Invite invite)
	{
		SocialMessage socialMessage = Notifications.Find((SocialMessage n) => !n.Canceled && n.Type == MessageType.Invite && n.Friend.Equals(invite.PotentialFriend));
		if (socialMessage != null)
		{
			socialMessage.Canceled = true;
			this.NotificationCancel?.Invoke(socialMessage);
		}
	}

	private void SendPlatformMessage(SocialEntity friend, string message)
	{
		Debug.Log("[Social.Chat] Sending private message to " + friend.DisplayName + ": " + message);
		SocialMessage message2 = new SocialMessage(friend, new UnlocalizedMTGAString(message), Direction.Outgoing);
		AddSocialMessage(message2);
		_socialManager.SendDirectMessage(friend.FullName, message);
	}

	private void ChatMessageReceived(DirectMessage directMessage)
	{
		if (_socialManager.LocalPlayer.PlayerId == directMessage.SenderPlayerId)
		{
			Debug.Log("[Social.Chat] Message received by recipient.");
			return;
		}
		if (string.IsNullOrEmpty(directMessage.SenderPlayerId) && string.IsNullOrEmpty(directMessage.SenderDisplayName))
		{
			Debug.Log("[Social.Chat] Message received from unknown sender.");
			return;
		}
		if (directMessage.Message.StartsWith("\u009b\u0080\u0099\u0091\u0092"))
		{
			Debug.Log("[Social.Chat] Received message prefixed with challenge command sequence, ignoring.");
			return;
		}
		Debug.Log("[Social.Chat] Receiving message from " + directMessage.SenderDisplayName + ": " + directMessage.Message);
		SocialMessage socialMessage = null;
		foreach (SocialEntity friend in _socialManager.Friends)
		{
			if (string.IsNullOrEmpty(directMessage.SenderPlayerId))
			{
				if (friend.FullName == directMessage.SenderDisplayName)
				{
					socialMessage = new SocialMessage(friend, new UnlocalizedMTGAString(directMessage.Message), Direction.Incoming);
					break;
				}
			}
			else if (friend.PlayerId == directMessage.SenderPlayerId)
			{
				socialMessage = new SocialMessage(friend, new UnlocalizedMTGAString(directMessage.Message), Direction.Incoming);
				break;
			}
		}
		if (socialMessage == null)
		{
			Debug.LogError("[Social.Chat] Received message from a non-friend.");
			return;
		}
		AddSocialMessage(socialMessage);
		Notifications.Add(socialMessage);
		_socialManager.BIMessage_NotificationBubbleShown(socialMessage);
		this.NotificationAlert?.Invoke(socialMessage);
	}

	private void FriendRemoved(SocialEntity friend)
	{
		Conversation conversation = GetConversation(friend);
		if (conversation != null)
		{
			Conversations.Remove(conversation);
		}
	}

	private void HandleException(Exception exception)
	{
	}
}
