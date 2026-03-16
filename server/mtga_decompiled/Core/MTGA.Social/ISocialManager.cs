using System;
using System.Collections.Generic;
using HasbroGo.Social.Models;
using Wizards.Arena.Promises;

namespace MTGA.Social;

public interface ISocialManager
{
	bool IsSocialEnabled { get; }

	bool Connected { get; }

	bool DeclineIncomingFriendRequests { get; }

	SocialEntity LocalPlayer { get; }

	List<SocialEntity> Friends { get; }

	List<Invite> InvitesOutgoing { get; }

	List<Invite> InvitesIncoming { get; }

	List<Block> Blocks { get; }

	ChatManager ChatManager { get; }

	event Action<bool> ConnectionChanged;

	event Action<SocialEntity> FriendAdded;

	event Action<SocialEntity> FriendRemoved;

	event Action<SocialEntity> FriendPresenceChanged;

	event Action FriendsChanged;

	event Action<Invite> InviteOutgoingAdded;

	event Action<Invite> InviteOutgoingRemoved;

	event Action InvitesOutgoingChanged;

	event Action<Invite> InviteIncomingAdded;

	event Action<Invite> InviteIncomingRemoved;

	event Action InvitesIncomingChanged;

	event Action BlocksChanged;

	event Action<SocialError> SocialException;

	event Action<Exception> UnknownException;

	event Action<bool> SocialEnabledChanged;

	event Action<bool> ChatEnabledChanged;

	event Action<DirectMessage> OnDirectMessage;

	event Action<GameMessage> OnGameMessage;

	event Action<PresenceStatus, PresenceStatus> LocalPresenceStatusChanged;

	void Destroy();

	void Connect();

	void Connect(PresenceStatus connectStatus);

	void Disconnect(bool shutdownStyle);

	void Reconnect();

	void SendDirectMessage(string friendFullName, string message);

	void SendGameMessage(string playerId, string message);

	void SetUserPresenceStatus(PresenceStatus presenceStatus);

	void RefreshFriendsData();

	void AcceptFriendInviteIncoming(SocialEntity potentialFriend);

	void AcceptFriendInviteIncoming(Invite invite);

	void DeclineFriendInviteIncoming(Invite invite);

	void ToggleAutoDeclineFriendInviteIncoming(bool shouldDecline);

	void SubmitFriendInviteOutgoing(string potentialFriend);

	void RevokeFriendInviteOutgoing(Invite invite);

	void RemoveFriend(SocialEntity friend);

	bool CheckIfAlreadyFriends(string playerId);

	bool CheckIfAlreadyFriendInvited(string playerId);

	void BlockByDisplayName(string fullDisplayName);

	void AddBlock(SocialEntity userToBeBlocked);

	void RemoveBlock(Block block);

	void ShowEnteringQueueWithOutgoingChallengeMessage(Action onOkAction);

	Promise<string> GetPlayerIdFromFullPlayerName(string fullPlayerName);

	void ForwardNotificationAlert(SocialMessage socialMessage);

	void BIMessage_ChatWindowOpened();

	void BIMessage_DirectMessageRecieved(Conversation conversation);

	void BIMessage_DirectMessageSent(Conversation conversation);

	void BIMessage_FriendsListOpened();

	void BIMessage_NotificationBubbleShown(SocialMessage notification);
}
