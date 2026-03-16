using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HasbroGo.Accounts.Events;
using HasbroGo.Accounts.Models;
using HasbroGo.Errors;
using HasbroGo.Helpers;
using HasbroGo.Logging;
using HasbroGo.Social.Events;
using HasbroGo.Social.Models;
using HasbroGo.Social.Models.Requests;
using HasbroGo.Social.Models.Requests.Invites;
using UnityEngine;

namespace HasbroGo;

public class SocialManager : MonoBehaviour
{
	public enum FriendAction
	{
		Block,
		Unblock,
		Unfriend,
		SendFriendInvite,
		AcceptIncomingFriendInvite,
		DeclineIncomingFriendInvite,
		DeclineIncomingFriendInviteAndBlockUser,
		RevokeOutgoingFriendInvite
	}

	public enum PresenceAction
	{
		UpdatePresence
	}

	private CancellationTokenSource userPresenceAutoRefreshCancellationToken = new CancellationTokenSource();

	private readonly string logCategory = "SocialManager";

	private const int userPresenceAutoRefreshInterval = 600000;

	private Dictionary<string, List<DirectMessageReceivedEventArgs>> socialDirectChatMessages = new Dictionary<string, List<DirectMessageReceivedEventArgs>>();

	public static SocialManager Instance { get; private set; }

	private HasbroGoSDK sdk => HasbroGoSDKManager.Instance.HasbroGoSdk;

	public event EventHandler FriendBlockedEvent;

	public event EventHandler FriendUnblockedEvent;

	public event EventHandler FriendUnfriendedEvent;

	public event EventHandler FriendInviteSentEvent;

	public event EventHandler FriendIncomingInviteAcceptedEvent;

	public event EventHandler FriendIncomingInviteDeclinedEvent;

	public event EventHandler FriendIncomingInviteDeclinedAndUserBlockedEvent;

	public event EventHandler FriendOutgoingInviteRevokedEvent;

	public event EventHandler FriendActionErrorEvent;

	public event EventHandler PresenceUpdatedEvent;

	public event EventHandler PresenceActionErrorEvent;

	public event EventHandler SendChatSuccessEvent;

	public event EventHandler SendChatErrorEvent;

	public event EventHandler ChatMessageReceivedEvent;

	public event EventHandler SendChallengeSuccessEvent;

	public event EventHandler SendChallengeErrorEvent;

	public event EventHandler ChallengeReceivedEvent;

	public event EventHandler GetProfileErrorEvent;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		Instance = this;
		Task.Run(() => AutoRefreshUserPresence(userPresenceAutoRefreshCancellationToken.Token), userPresenceAutoRefreshCancellationToken.Token);
		sdk.SocialService.OnDirectMessageReceived += HandlePrivateMessageReceived;
		sdk.SocialService.OnGameMessageReceived += HandleChallengeMessageReceived;
		sdk.AccountsService.Logout += HandleLogOutEvent;
		UnityEngine.Object.DontDestroyOnLoad(this);
	}

	private void OnDestroy()
	{
		userPresenceAutoRefreshCancellationToken.Cancel();
		sdk.SocialService.OnDirectMessageReceived -= HandlePrivateMessageReceived;
		sdk.SocialService.OnGameMessageReceived -= HandleChallengeMessageReceived;
		sdk.AccountsService.Logout -= HandleLogOutEvent;
	}

	private Task HandleLogOutEvent(object sender, LogoutEvent logoutEvent, CancellationToken token)
	{
		socialDirectChatMessages.Clear();
		return Task.CompletedTask;
	}

	public async void BlockFriend(string accountId)
	{
		BlockUserRequest blockRequest = new BlockUserRequest(accountId);
		Result<BlockedUser, Error> result = await sdk.SocialService.BlockUser(blockRequest);
		if (result.IsOk)
		{
			this.FriendBlockedEvent?.Invoke(this, EventArgs.Empty);
		}
		else
		{
			DispatchFriendActionErrorEvent(new ErrorEventArgs(result.Error, FriendAction.Block, accountId));
		}
	}

	public async void UnblockFriend(BlockedUser blockedUser)
	{
		Result<Success, Error> result = await sdk.SocialService.UnblockUser(blockedUser);
		if (result.IsOk)
		{
			this.FriendUnblockedEvent?.Invoke(this, EventArgs.Empty);
		}
		else
		{
			DispatchFriendActionErrorEvent(new ErrorEventArgs(result.Error, FriendAction.Unblock, blockedUser.AccountId));
		}
	}

	public async void Unfriend(Friend friend)
	{
		Result<Success, Error> result = await sdk.SocialService.RemoveFriend(friend);
		if (result.IsOk)
		{
			this.FriendUnfriendedEvent?.Invoke(this, EventArgs.Empty);
		}
		else
		{
			DispatchFriendActionErrorEvent(new ErrorEventArgs(result.Error, FriendAction.Unfriend, friend));
		}
	}

	public async void SendFriendInvite(string friendRequestInfo)
	{
		if (string.IsNullOrEmpty(friendRequestInfo))
		{
			DispatchFriendActionErrorEvent(new ErrorEventArgs(null, FriendAction.SendFriendInvite, true));
			return;
		}
		Email email = new Email(friendRequestInfo);
		DisplayName displayName = new DisplayName(friendRequestInfo);
		if (displayName.IsValid)
		{
			DispatchSendFriendRequestResult(await sdk.SocialService.SendFriendInviteWithDisplayName(displayName));
			return;
		}
		if (email.IsValid)
		{
			DispatchSendFriendRequestResult(await sdk.SocialService.SendFriendInviteWithEmail(email));
			return;
		}
		Result<FriendRequestReference, Error> result = await sdk.SocialService.SendFriendInviteWithAccountId(friendRequestInfo);
		if (result.IsOk)
		{
			DispatchSendFriendRequestResult(result);
			return;
		}
		result = await sdk.SocialService.SendFriendInviteWithPersonaId(friendRequestInfo);
		if (result.IsOk)
		{
			DispatchSendFriendRequestResult(result);
		}
		else
		{
			DispatchFriendActionErrorEvent(new ErrorEventArgs(null, FriendAction.SendFriendInvite, true));
		}
	}

	public async void AcceptIncomingFriendInvite(string inviteId)
	{
		Result<FriendWithPresence, Error> result = await sdk.SocialService.AcceptFriendInvite(new FriendRequestReference(inviteId));
		if (result.IsOk)
		{
			this.FriendIncomingInviteAcceptedEvent?.Invoke(this, EventArgs.Empty);
		}
		else
		{
			DispatchFriendActionErrorEvent(new ErrorEventArgs(result.Error, FriendAction.AcceptIncomingFriendInvite, inviteId));
		}
	}

	public async void DeclineIncomingFriendInvite(string inviteId)
	{
		Result<Success, Error> result = await sdk.SocialService.DeclineFriendInvite(new FriendRequestReference(inviteId));
		if (result.IsOk)
		{
			this.FriendIncomingInviteDeclinedEvent?.Invoke(this, EventArgs.Empty);
		}
		else
		{
			DispatchFriendActionErrorEvent(new ErrorEventArgs(result.Error, FriendAction.DeclineIncomingFriendInvite, inviteId));
		}
	}

	public async void DeclineIncomingInviteAndBlockUser(string senderAccountId, string inviteId)
	{
		Result<Success, Error> declineInviteResult = await sdk.SocialService.DeclineFriendInvite(new FriendRequestReference(inviteId));
		BlockUserRequest blockRequest = new BlockUserRequest(senderAccountId);
		Result<BlockedUser, Error> result = await sdk.SocialService.BlockUser(blockRequest);
		if (declineInviteResult.IsOk && result.IsOk)
		{
			this.FriendIncomingInviteDeclinedAndUserBlockedEvent?.Invoke(this, EventArgs.Empty);
		}
		else
		{
			DispatchFriendActionErrorEvent(new ErrorEventArgs(declineInviteResult.Error, FriendAction.DeclineIncomingFriendInviteAndBlockUser, "Sender Accound Id: " + senderAccountId + ", Invite Id: " + inviteId));
		}
	}

	public async void RevokeOutgoingFriendInvite(string inviteId)
	{
		Result<Success, Error> result = await sdk.SocialService.RevokeFriendInvite(new FriendRequestReference(inviteId));
		if (result.IsOk)
		{
			this.FriendOutgoingInviteRevokedEvent?.Invoke(this, EventArgs.Empty);
		}
		else
		{
			DispatchFriendActionErrorEvent(new ErrorEventArgs(result.Error, FriendAction.RevokeOutgoingFriendInvite, inviteId));
		}
	}

	public async void UpdatePresence(PlatformStatus platformStatus)
	{
		Result<Presence, Error> result = await sdk.SocialService.UpdatePresence(platformStatus);
		if (result.IsOk)
		{
			this.PresenceUpdatedEvent?.Invoke(this, new UpdatePresenceEventArgs(result.Value));
		}
		else
		{
			DispatchPresenceActionErrorEvent(new ErrorEventArgs(result.Error, PresenceAction.UpdatePresence, platformStatus.ToString()));
		}
	}

	public async Task RefreshUserPresence(bool isLoginRefresh)
	{
		if (!sdk.AccountsService.IsLoggedIn())
		{
			LogHelper.Logger.Log(LogLevel.Log, "User is not logged in. Cannot refresh Presence.");
			return;
		}
		LogHelper.Logger.Log(LogLevel.Log, "Refreshing user presence.");
		Result<Presence, Error> result = await sdk.SocialService.GetPresence();
		if (!result.IsOk)
		{
			LogHelper.Logger.Log(logCategory, LogLevel.Error, "Failed to get user's Presence with error: " + result.Error.Message);
			return;
		}
		Presence value = result.Value;
		PlatformStatus platformStatus = ((value != null && (!isLoginRefresh || value.PlatformStatus != PlatformStatus.Offline)) ? value.PlatformStatus : PlatformStatus.Playing);
		Result<Presence, Error> result2 = await sdk.SocialService.UpdatePresence(platformStatus);
		if (!result2.IsOk)
		{
			LogHelper.Logger.Log(logCategory, LogLevel.Error, "Failed to update user's Presence with error: " + result2.Error.Message);
		}
		else
		{
			this.PresenceUpdatedEvent?.Invoke(this, new UpdatePresenceEventArgs(result2.Value));
		}
	}

	public async void SendDirectChatMessage(Friend friend, string message)
	{
		DirectMessage msg = new DirectMessage(sdk.AuthManager.GetAccountId(), message);
		DirectMessageReceivedEventArgs privateMessageReceivedEventArgs = new DirectMessageReceivedEventArgs(msg);
		Result<Success, Error> result = await sdk.SocialService.SendDirectChatMessage(friend, message);
		if (socialDirectChatMessages.ContainsKey(friend.AccountId))
		{
			socialDirectChatMessages[friend.AccountId].Add(privateMessageReceivedEventArgs);
		}
		else
		{
			socialDirectChatMessages[friend.AccountId] = new List<DirectMessageReceivedEventArgs> { privateMessageReceivedEventArgs };
		}
		if (result.IsOk)
		{
			this.SendChatSuccessEvent?.Invoke(this, EventArgs.Empty);
		}
		else
		{
			DispatchSendChatFailureEvent(new ErrorEventArgs(result.Error, friend.AccountId, message));
		}
	}

	private void HandlePrivateMessageReceived(object sender, DirectMessageReceivedEventArgs privateMessageReceivedEventArgs)
	{
		if (socialDirectChatMessages.ContainsKey(privateMessageReceivedEventArgs.Message.SenderAccountId))
		{
			socialDirectChatMessages[privateMessageReceivedEventArgs.Message.SenderAccountId].Add(privateMessageReceivedEventArgs);
		}
		else
		{
			socialDirectChatMessages[privateMessageReceivedEventArgs.Message.SenderAccountId] = new List<DirectMessageReceivedEventArgs> { privateMessageReceivedEventArgs };
		}
		this.ChatMessageReceivedEvent?.Invoke(this, privateMessageReceivedEventArgs);
	}

	public async void SendChallengeGameMessage(string displayName, string schema, DateTime expirationTime, string message)
	{
		Result<PublicProfileViaDisplayName, Error> result = await sdk.SocialService.GetProfileViaDisplayName(displayName);
		if (result.IsOk)
		{
			Result<Success, List<Error>> result2 = await sdk.SocialService.SendGameMessage(new List<string> { result.Value.PersonaId }, new GameMessage(sdk.AuthManager.GetPersonaId(), schema, message, expirationTime));
			if (result2.IsOk)
			{
				this.SendChallengeSuccessEvent?.Invoke(this, new EventArgs());
			}
			else
			{
				this.SendChallengeErrorEvent?.Invoke(this, new ErrorEventArgs(result2.Error[0], displayName, message));
			}
		}
		else
		{
			this.GetProfileErrorEvent?.Invoke(this, new ErrorEventArgs(result.Error, displayName, false));
		}
	}

	private void HandleChallengeMessageReceived(object sender, GameMessageReceivedEventArgs gameMessageReceivedEventArgs)
	{
		if (gameMessageReceivedEventArgs.GameMessage.SchemaId == "ChallengeConfirmData" || gameMessageReceivedEventArgs.GameMessage.SchemaId == "ChallengeSendData")
		{
			this.ChallengeReceivedEvent?.Invoke(this, gameMessageReceivedEventArgs);
		}
	}

	private void DispatchFriendActionErrorEvent(ErrorEventArgs eventArgs)
	{
		this.FriendActionErrorEvent?.Invoke(this, eventArgs);
	}

	private void DispatchSendFriendRequestResult(Result<FriendRequestReference, Error> result)
	{
		if (result.IsOk)
		{
			this.FriendInviteSentEvent?.Invoke(this, EventArgs.Empty);
		}
		else
		{
			DispatchFriendActionErrorEvent(new ErrorEventArgs(result.Error, FriendAction.SendFriendInvite, false));
		}
	}

	private void DispatchPresenceActionErrorEvent(ErrorEventArgs eventArgs)
	{
		this.PresenceActionErrorEvent?.Invoke(this, eventArgs);
	}

	private async Task AutoRefreshUserPresence(CancellationToken token)
	{
		while (!token.IsCancellationRequested)
		{
			await Task.Delay(600000, token);
			await RefreshUserPresence(isLoginRefresh: false);
		}
	}

	private void DispatchSendChatFailureEvent(ErrorEventArgs eventArgs)
	{
		this.SendChatErrorEvent?.Invoke(this, eventArgs);
	}

	public List<DirectMessageReceivedEventArgs> GetAccountIdChatMessages(string accountId)
	{
		if (!socialDirectChatMessages.ContainsKey(accountId))
		{
			return new List<DirectMessageReceivedEventArgs>();
		}
		return socialDirectChatMessages[accountId];
	}
}
