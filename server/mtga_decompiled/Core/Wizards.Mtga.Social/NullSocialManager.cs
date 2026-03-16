using System;
using System.Collections.Generic;
using HasbroGo.Social.Models;
using MTGA.Social;
using Wizards.Arena.Promises;

namespace Wizards.Mtga.Social;

public class NullSocialManager : ISocialManager, IDisposable
{
	public bool IsSocialEnabled => false;

	public bool Connected => false;

	public bool DeclineIncomingFriendRequests => false;

	public SocialEntity LocalPlayer { get; }

	public List<SocialEntity> Friends { get; }

	public List<Invite> InvitesOutgoing { get; }

	public List<Invite> InvitesIncoming { get; }

	public List<Block> Blocks { get; }

	public ChatManager ChatManager { get; }

	public event Action<bool> ConnectionChanged;

	public event Action<SocialEntity> FriendAdded;

	public event Action<SocialEntity> FriendRemoved;

	public event Action<SocialEntity> FriendPresenceChanged;

	public event Action FriendsChanged;

	public event Action<Invite> InviteOutgoingAdded;

	public event Action<Invite> InviteOutgoingRemoved;

	public event Action InvitesOutgoingChanged;

	public event Action<Invite> InviteIncomingAdded;

	public event Action<Invite> InviteIncomingRemoved;

	public event Action InvitesIncomingChanged;

	public event Action BlocksChanged;

	public event Action<SocialError> SocialException;

	public event Action<Exception> UnknownException;

	public event Action<bool> SocialEnabledChanged;

	public event Action<bool> ChatEnabledChanged;

	public event Action<MTGA.Social.DirectMessage> OnDirectMessage;

	public event Action<GameMessage> OnGameMessage;

	public event Action<PresenceStatus, PresenceStatus> LocalPresenceStatusChanged;

	public static NullSocialManager Create()
	{
		return new NullSocialManager();
	}

	public void Destroy()
	{
	}

	public void Connect()
	{
	}

	public void Connect(PresenceStatus connectStatus)
	{
	}

	public void Disconnect(bool shutdownStyle)
	{
	}

	public void Reconnect()
	{
	}

	public void SendDirectMessage(string friendFullName, string message)
	{
	}

	public void SendGameMessage(string playerId, string message)
	{
	}

	public void SetUserPresenceStatus(PresenceStatus presenceStatus)
	{
	}

	public void RefreshFriendsData()
	{
	}

	public void AcceptFriendInviteIncoming(SocialEntity potentialFriend)
	{
	}

	public void AcceptFriendInviteIncoming(Invite invite)
	{
	}

	public void DeclineFriendInviteIncoming(Invite invite)
	{
	}

	public void ToggleAutoDeclineFriendInviteIncoming(bool shouldDecline)
	{
	}

	public void SubmitFriendInviteOutgoing(string potentialFriend)
	{
	}

	public void RevokeFriendInviteOutgoing(Invite invite)
	{
	}

	public void RemoveFriend(SocialEntity friend)
	{
	}

	public bool CheckIfAlreadyFriends(string playerId)
	{
		return false;
	}

	public bool CheckIfAlreadyFriendInvited(string playerId)
	{
		return false;
	}

	public void BlockByDisplayName(string fullDisplayName)
	{
	}

	public void AddBlock(SocialEntity userToBeBlocked)
	{
	}

	public void RemoveBlock(Block block)
	{
	}

	public void ShowEnteringQueueWithOutgoingChallengeMessage(Action onOkAction)
	{
	}

	public Promise<string> GetPlayerIdFromFullPlayerName(string fullPlayerName)
	{
		return new SimplePromise<string>();
	}

	public void ForwardNotificationAlert(SocialMessage socialMessage)
	{
	}

	public void BIMessage_ChatWindowOpened()
	{
	}

	public void BIMessage_DirectMessageRecieved(Conversation conversation)
	{
	}

	public void BIMessage_DirectMessageSent(Conversation conversation)
	{
	}

	public void BIMessage_FriendsListOpened()
	{
	}

	public void BIMessage_NotificationBubbleShown(SocialMessage notification)
	{
	}

	public void Dispose()
	{
	}
}
