using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Code.ClientFeatureToggle;
using Core.Code.Promises;
using Core.Meta.MainNavigation.Challenge;
using HasbroGo;
using HasbroGo.Accounts.Events;
using HasbroGo.Accounts.Models;
using HasbroGo.Authentication;
using HasbroGo.Authentication.Errors;
using HasbroGo.Authentication.Results;
using HasbroGo.Config;
using HasbroGo.Errors;
using HasbroGo.Helpers;
using HasbroGo.Social;
using HasbroGo.Social.Events;
using HasbroGo.Social.Models;
using HasbroGo.Social.Models.Requests;
using HasbroGo.Social.Models.Requests.Invites;
using HasbroGo.TestSupport;
using RSG;
using UnityEngine;
using UnityEngine.SceneManagement;
using WAS;
using Wizards.Arena.Client.Logging;
using Wizards.Arena.Promises;
using Wizards.Arena.TcpConnection;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wizards.Mtga.Logging;
using Wizards.Unification.Models.PlayBlade;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;

namespace MTGA.Social;

public class SocialManagerHasbroGo : ISocialManager, IAccessTokenProvider, ILoginChangedNotifier, IDisposable
{
	private bool _isSocialEnabled = true;

	private IPromise _connectionSequencePromise;

	private PresenceStatus _previousStatus;

	private const int MAX_RECONNECT_ATTEMPTS = 4;

	private Promise _sdkInitPromise;

	private int _sdkInitRetryCount;

	private FrontDoorConnectionAWS _fdc;

	private readonly IAccountClient _accountClient;

	private readonly ConnectionManager _connectionManager;

	private ISocialService _socialSdk;

	private PlayerPrefsDataProvider _playerPrefsDataProvider;

	private ClientFeatureToggleDataProvider _clientFeatureToggleDataProvider;

	private int _reconnectAttempts;

	private bool _socialSettingsChanged;

	private Coroutine _maintainPresenceCoroutine;

	private PVPChallengeController _challengeController;

	private UnityLogger _logger;

	private IBILogger _biLogger;

	public bool IsSocialEnabled
	{
		get
		{
			return _isSocialEnabled;
		}
		internal set
		{
			if (_isSocialEnabled != value)
			{
				_isSocialEnabled = value;
				InvokeActionOnMainThread(this.SocialEnabledChanged, value);
			}
		}
	}

	public bool Connected { get; private set; }

	public bool DeclineIncomingFriendRequests { get; private set; }

	public SocialEntity LocalPlayer { get; private set; }

	public List<SocialEntity> Friends { get; } = new List<SocialEntity>();

	public List<Invite> InvitesOutgoing { get; } = new List<Invite>();

	public List<Invite> InvitesIncoming { get; } = new List<Invite>();

	public List<Block> Blocks { get; } = new List<Block>();

	public ChatManager ChatManager { get; private set; }

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

	public event Action<DirectMessage> OnDirectMessage;

	public event Action<GameMessage> OnGameMessage;

	public event Action<PresenceStatus, PresenceStatus> LocalPresenceStatusChanged;

	public event AsyncEventHandler? UserLoggedIn;

	public event AsyncEventHandler? UserLoggedOut;

	public static SocialManagerHasbroGo Create()
	{
		return new SocialManagerHasbroGo();
	}

	private SocialManagerHasbroGo()
	{
		_accountClient = Pantry.Get<IAccountClient>();
		_fdc = Pantry.Get<IFrontDoorConnectionServiceWrapper>().FDCAWS;
		_connectionManager = Pantry.Get<ConnectionManager>();
		_biLogger = Pantry.Get<IBILogger>();
		_playerPrefsDataProvider = Pantry.Get<PlayerPrefsDataProvider>();
		_clientFeatureToggleDataProvider = Pantry.Get<ClientFeatureToggleDataProvider>();
		LocalPlayer = new SocialEntity(_accountClient.AccountInformation?.DisplayName);
		ChatManager = new ChatManager(this);
		UpdateSocialEnabled();
		SocialEnabledChanged += HandleSocialEnabledChanged;
		_clientFeatureToggleDataProvider.RegisterForToggleUpdates(UpdateSocialEnabled);
		_logger = new UnityLogger("SocialManager", LoggerLevel.Debug);
		LoggerManager.Register(_logger);
		_logger.LogWarningForRelease("Social manager for HasbroGo is being used");
		InitSocialService();
	}

	private void UpdateSocialEnabled()
	{
		IsSocialEnabled = !_fdc.Killswitch.IsSocialDisabled;
	}

	public void InitSocialService()
	{
		ChatManager.Init();
		_accountClient.LoginStateChanged += HandleLoginStateChange;
		SocialLogger logger = new SocialLogger(new UnityLogger("SocialSDK", LoggerLevel.Debug));
		LogHelper.SetLogger(logger);
		IBaseConfiguration config = new Configuration(Configuration.ProdBaseUrl, Configuration.ProdBaseWizMessageBusUrl, WASHTTPClient.ClientID, WASHTTPClient.ClientSecret, logger);
		IJwtTokenReader tokenReader = new JwtTokenReader();
		IAuthManager authManager = new AuthManager(this, tokenReader);
		_socialSdk = new SocialService(config, this, authManager, new HttpResultFactoryUnchecked(), this);
		AddHandlers();
		InitLocalPlayerPresence();
	}

	public void Destroy()
	{
		_accountClient.LoginStateChanged -= HandleLoginStateChange;
		Disconnect(shutdownStyle: true);
		RemoveHandlers();
		_socialSdk.ShutdownService();
		if (Connected)
		{
			Connected = false;
			InvokeActionOnMainThread(this.ConnectionChanged, true);
		}
		ChatManager.Destroy();
	}

	private void HandleLoginStateChange(LoginState state)
	{
		switch (state)
		{
		case LoginState.AttemptingToLogin:
			return;
		case LoginState.FullyRegisteredLogin:
			Connect();
			return;
		}
		if (Connected)
		{
			this.UserLoggedOut?.Invoke(this, new LogoutEvent(), default(CancellationToken));
		}
		Disconnect(shutdownStyle: false);
	}

	public void Connect()
	{
		UpdateSocialEnabled();
		RefreshAutoDeclineFriendInvitePreference();
		if (!Connected && LocalPlayer.Status != PresenceStatus.Offline && IsSocialEnabled)
		{
			DoConnectSequence();
		}
	}

	public void Connect(PresenceStatus connectStatus)
	{
	}

	public void Disconnect(bool shutdownStyle)
	{
		if (Connected)
		{
			if (!shutdownStyle)
			{
				CancelAndRejectAllChallenges();
			}
			(_connectionSequencePromise ?? Promise.Resolved()).Then((Func<IPromise>)DoDisconnectSequence).Then(delegate
			{
				InvokeActionOnMainThread(this.ConnectionChanged, shutdownStyle);
			});
		}
	}

	public void Reconnect()
	{
		if (_reconnectAttempts < 4)
		{
			float secondsDelayed = Mathf.Pow(_reconnectAttempts, 2f) / 2f;
			_reconnectAttempts++;
			PAPA.StartGlobalCoroutine(Coroutine_DelayConnect(secondsDelayed));
		}
		else
		{
			Connected = false;
			InvokeActionOnMainThread(this.ConnectionChanged, true);
		}
	}

	private void AddHandlers()
	{
		RemoveHandlers();
		_socialSdk.OnGameMessageReceived += HandleGameMessage;
		_socialSdk.OnDirectMessageReceived += HandlePrivateMessage;
		_socialSdk.OnFriendPresenceUpdateReceived += HandleFriendPresenceUpdate;
		_socialSdk.OnOutgoingFriendInviteSent += HandleOutgoingInviteAdded;
		_socialSdk.OnFriendInviteRemoved += HandleFriendInviteRemoved;
		_socialSdk.OnIncomingFriendInviteReceived += HandleIncomingInviteAdded;
		_socialSdk.OnIncomingFriendInviteRevoked += HandleIncomingInviteRemoved;
		_socialSdk.OnFriendAdded += HandleFriendAdded;
		_socialSdk.OnFriendRemoved += HandleRemoveFriend;
		PlayerPrefsDataProvider playerPrefsDataProvider = _playerPrefsDataProvider;
		playerPrefsDataProvider.PreferenceDataChanged = (Action)Delegate.Combine(playerPrefsDataProvider.PreferenceDataChanged, new Action(RefreshAutoDeclineFriendInvitePreference));
	}

	private void RemoveHandlers()
	{
		_socialSdk.OnGameMessageReceived -= HandleGameMessage;
		_socialSdk.OnDirectMessageReceived -= HandlePrivateMessage;
		_socialSdk.OnFriendPresenceUpdateReceived -= HandleFriendPresenceUpdate;
		_socialSdk.OnOutgoingFriendInviteSent -= HandleOutgoingInviteAdded;
		_socialSdk.OnFriendInviteRemoved -= HandleFriendInviteRemoved;
		_socialSdk.OnIncomingFriendInviteReceived -= HandleIncomingInviteAdded;
		_socialSdk.OnIncomingFriendInviteRevoked -= HandleIncomingInviteRemoved;
		_socialSdk.OnFriendAdded -= HandleFriendAdded;
		_socialSdk.OnFriendRemoved -= HandleRemoveFriend;
		PlayerPrefsDataProvider playerPrefsDataProvider = _playerPrefsDataProvider;
		playerPrefsDataProvider.PreferenceDataChanged = (Action)Delegate.Remove(playerPrefsDataProvider.PreferenceDataChanged, new Action(RefreshAutoDeclineFriendInvitePreference));
	}

	public void SetUserPresenceStatus(PresenceStatus presenceStatus)
	{
		BIMessage_ChangeSocialMode(LocalPlayer.Status, presenceStatus, SocialModeValidity.Assumed);
		_previousStatus = LocalPlayer.Status;
		LocalPlayer.SetPresence(presenceStatus);
		if (Connected)
		{
			SyncPresenceWithBackend();
			if (_previousStatus != PresenceStatus.Busy && presenceStatus == PresenceStatus.Busy)
			{
				CancelAndRejectAllChallenges();
			}
			else if (presenceStatus == PresenceStatus.Offline)
			{
				Disconnect(shutdownStyle: false);
			}
		}
		else if (presenceStatus != PresenceStatus.Offline)
		{
			if (_challengeController == null)
			{
				_challengeController = Pantry.Get<PVPChallengeController>();
			}
			_challengeController.RejectAllChallengeInvites();
			Connect();
		}
	}

	private void CancelAndRejectAllChallenges()
	{
		if (_challengeController == null)
		{
			_challengeController = Pantry.Get<PVPChallengeController>();
		}
		_challengeController.RejectAllChallengeInvites();
		_challengeController.CancelAllChallenges();
	}

	public void RefreshFriendsData()
	{
		_logger.Log(LoggerLevel.Error, new Exception($"({DateTimeOffset.UtcNow:T}) Refreshing friends data"), "");
		GetFriendsFromPlatform().Then((Func<IPromise>)GetIncomingFriendInvites).Then((Func<IPromise>)GetOutgoingFriendInvites).Then((Func<IPromise>)GetBlocks);
	}

	public void SendDirectMessage(string friendDisplayName, string message)
	{
		foreach (SocialEntity friend2 in Friends)
		{
			if (!(friend2.FullName == friendDisplayName))
			{
				continue;
			}
			Friend friend = new Friend(friend2.SocialId, DateTime.UtcNow, null, new DisplayName(friendDisplayName));
			_socialSdk.SendDirectChatMessage(friend, message).Then<Result<Success, HasbroGo.Errors.Error>>(delegate(Result<Success, HasbroGo.Errors.Error> result)
			{
				if (!result.IsOk)
				{
					_logger.LogError(result.Error.Message);
				}
			});
		}
	}

	public void SendGameMessage(string playerId, string message)
	{
		string obj = _accountClient?.AccountInformation?.PersonaID;
		GameMessage customGameMessage = new GameMessage(obj ?? "unknown", "none", message);
		if (!string.IsNullOrEmpty(obj) || Connected)
		{
			_socialSdk.SendGameMessage(new List<string> { playerId }, customGameMessage);
		}
	}

	public void AcceptFriendInviteIncoming(SocialEntity potentialFriend)
	{
		Invite invite = InvitesIncoming.Find((Invite i) => i.PotentialFriend.Equals(potentialFriend));
		AcceptFriendInviteIncoming(invite);
	}

	public void AcceptFriendInviteIncoming(Invite invite)
	{
		if (!InvitesIncoming.Contains(invite))
		{
			_logger.LogError("Invite not found");
			return;
		}
		InvitesIncoming.Remove(invite);
		InvokeActionOnMainThread(this.InviteIncomingRemoved, invite);
		InvokeActionOnMainThread(this.InvitesIncomingChanged);
		BIMessage_AddFriend(invite.PotentialFriend);
		_socialSdk.AcceptFriendInvite(new FriendRequestReference(invite.InviteId)).Then<Result<FriendWithPresence, HasbroGo.Errors.Error>>(delegate(Result<FriendWithPresence, HasbroGo.Errors.Error> result)
		{
			if (!result.IsOk)
			{
				InvitesIncoming.Add(invite);
				InvokeActionOnMainThread(this.InviteIncomingAdded, invite);
				InvokeActionOnMainThread(this.InvitesIncomingChanged);
				_logger.LogError(result.Error.Message);
			}
		});
	}

	public void DeclineFriendInviteIncoming(Invite invite)
	{
		if (!InvitesIncoming.Contains(invite))
		{
			_logger.LogError("Invite not found");
			return;
		}
		InvitesIncoming.Remove(invite);
		InvokeActionOnMainThread(this.InviteIncomingRemoved, invite);
		InvokeActionOnMainThread(this.InvitesIncomingChanged);
		_socialSdk.DeclineFriendInvite(new FriendRequestReference(invite.InviteId)).Then<Result<Success, HasbroGo.Errors.Error>>(delegate(Result<Success, HasbroGo.Errors.Error> result)
		{
			if (!result.IsOk)
			{
				InvitesIncoming.Add(invite);
				InvokeActionOnMainThread(this.InviteIncomingAdded, invite);
				InvokeActionOnMainThread(this.InvitesIncomingChanged);
				_logger.LogError(result.Error.Message);
			}
		});
	}

	public void ToggleAutoDeclineFriendInviteIncoming(bool shouldDecline)
	{
		BIMessage_ToggleBlockIncomingFriendRequests(shouldDecline);
		_playerPrefsDataProvider.SetPreferenceBool("AutoDeclineFriendInvites", shouldDecline).ThenOnMainThreadIfSuccess((Action<DTO_PlayerPreferences>)delegate
		{
			DeclineIncomingFriendRequests = shouldDecline;
			if (InvitesIncoming == null || InvitesIncoming.Count == 0 || !DeclineIncomingFriendRequests)
			{
				return;
			}
			foreach (Invite item in new List<Invite>(InvitesIncoming))
			{
				DeclineFriendInviteIncoming(item);
			}
		});
	}

	public void RefreshAutoDeclineFriendInvitePreference()
	{
		if (!_playerPrefsDataProvider.Initialized)
		{
			DeclineIncomingFriendRequests = false;
			return;
		}
		_playerPrefsDataProvider.GetPreferenceBool("AutoDeclineFriendInvites").Convert((bool autoDeclineInvites) => DeclineIncomingFriendRequests = autoDeclineInvites);
	}

	public void SubmitFriendInviteOutgoing(string potentialFriend)
	{
		if (potentialFriend.Contains("@"))
		{
			if (InvitesOutgoing.Exists((Invite i) => i.PotentialFriend.DisplayName == potentialFriend) || !new Email(potentialFriend).IsValid)
			{
				return;
			}
			_socialSdk.SendFriendInviteWithEmail(new Email(potentialFriend)).Then<Result<FriendRequestReference, HasbroGo.Errors.Error>>(delegate(Result<FriendRequestReference, HasbroGo.Errors.Error> result)
			{
				if (!result.IsOk)
				{
					_logger.LogError(result.Error.Message);
				}
			});
		}
		else
		{
			if (InvitesOutgoing.Exists((Invite i) => i.PotentialFriend.FullName == potentialFriend))
			{
				return;
			}
			Block block = Blocks.Find((Block i) => i.BlockedPlayer.DisplayName == potentialFriend);
			if (block != null)
			{
				RemoveBlock(block);
			}
			DisplayName displayName = new DisplayName(potentialFriend);
			if (!displayName.IsValid)
			{
				return;
			}
			_socialSdk.SendFriendInviteWithDisplayName(displayName).Then<Result<FriendRequestReference, HasbroGo.Errors.Error>>(delegate(Result<FriendRequestReference, HasbroGo.Errors.Error> result)
			{
				if (!result.IsOk)
				{
					_logger.LogError(result.Error.Message);
				}
			});
		}
	}

	public void RevokeFriendInviteOutgoing(Invite invite)
	{
		if (!InvitesOutgoing.Contains(invite))
		{
			_logger.LogError("Invite not found");
			return;
		}
		RemoveOutgoingFriendInvite(invite);
		_socialSdk.RevokeFriendInvite(new FriendRequestReference(invite.InviteId)).Then<Result<Success, HasbroGo.Errors.Error>>(delegate(Result<Success, HasbroGo.Errors.Error> result)
		{
			if (!result.IsOk)
			{
				InvitesOutgoing.Add(invite);
				InvokeActionOnMainThread(this.InviteOutgoingAdded, invite);
				InvokeActionOnMainThread(this.InvitesOutgoingChanged);
				_logger.LogError(result.Error.Message);
			}
		});
	}

	public void RemoveFriend(SocialEntity friend)
	{
		if (!Friends.Contains(friend))
		{
			_logger.LogError("Friend not found");
			return;
		}
		Friends.Remove(friend);
		InvokeActionOnMainThread(this.FriendRemoved, friend);
		InvokeActionOnMainThread(this.FriendsChanged);
		BIMessage_RemoveFriend(friend);
		_socialSdk.RemoveFriend(new Friend(friend.SocialId, DateTime.UtcNow, null, new DisplayName(friend.DisplayName))).Then<Result<Success, HasbroGo.Errors.Error>>(delegate(Result<Success, HasbroGo.Errors.Error> result)
		{
			if (!result.IsOk)
			{
				Friends.Add(friend);
				InvokeActionOnMainThread(this.FriendAdded, friend);
				InvokeActionOnMainThread(this.FriendsChanged);
				_logger.LogError(result.Error.Message);
			}
		});
	}

	public bool CheckIfAlreadyFriends(string playerId)
	{
		if (!string.IsNullOrEmpty(playerId))
		{
			return Friends.Count((SocialEntity socialEntity) => socialEntity.PlayerId == playerId) > 0;
		}
		return false;
	}

	public bool CheckIfAlreadyFriendInvited(string playerName)
	{
		if (!string.IsNullOrEmpty(playerName))
		{
			if (!InvitesIncoming.Exists((Invite invite) => invite.PotentialFriend.FullName == playerName))
			{
				return InvitesOutgoing.Exists((Invite invite) => invite.PotentialFriend.FullName == playerName);
			}
			return true;
		}
		return false;
	}

	public void BlockByDisplayName(string displayName)
	{
		_socialSdk.GetProfileViaDisplayName(displayName).Then<Result<PublicProfileViaDisplayName, HasbroGo.Errors.Error>>(delegate(Result<PublicProfileViaDisplayName, HasbroGo.Errors.Error> result)
		{
			if (result.IsOk)
			{
				SocialEntity userToBeBlocked = new SocialEntity(displayName, result.Value.AccountId);
				AddBlock(userToBeBlocked);
			}
			else
			{
				_logger.LogError(result.Error.Message);
			}
		});
	}

	public void AddBlock(SocialEntity userToBeBlocked)
	{
		SocialEntity entity = Friends.Find((SocialEntity f) => f.Equals(userToBeBlocked));
		if (entity != null)
		{
			Friends.Remove(entity);
			InvokeActionOnMainThread(this.FriendRemoved, entity);
			InvokeActionOnMainThread(this.FriendsChanged);
		}
		Invite incomingInvite = InvitesIncoming.Find((Invite i) => i.PotentialFriend.SocialId == userToBeBlocked.SocialId);
		if (incomingInvite != null)
		{
			InvitesIncoming.Remove(incomingInvite);
			InvokeActionOnMainThread(this.InviteIncomingRemoved, incomingInvite);
			InvokeActionOnMainThread(this.InvitesIncomingChanged);
		}
		Block block = new Block(userToBeBlocked);
		BIMessage_BlockPlayer(userToBeBlocked);
		Blocks.Add(block);
		InvokeActionOnMainThread(this.BlocksChanged);
		BlockUserRequest blockRequest = new BlockUserRequest(userToBeBlocked.SocialId);
		_socialSdk.BlockUser(blockRequest).Then<Result<BlockedUser, HasbroGo.Errors.Error>>(delegate(Result<BlockedUser, HasbroGo.Errors.Error> result)
		{
			if (result.IsOk)
			{
				block.SetPlatformBlock(result.Value);
				InvokeActionOnMainThread(this.BlocksChanged);
			}
			else
			{
				Blocks.Remove(block);
				InvokeActionOnMainThread(this.BlocksChanged);
				if (entity != null)
				{
					Friends.Add(entity);
					InvokeActionOnMainThread(this.FriendAdded, entity);
					InvokeActionOnMainThread(this.FriendsChanged);
				}
				else if (incomingInvite != null)
				{
					InvitesIncoming.Add(incomingInvite);
					InvokeActionOnMainThread(this.InviteIncomingAdded, incomingInvite);
					InvokeActionOnMainThread(this.InvitesIncomingChanged);
				}
				_logger.LogError(result.Error.Message);
			}
		});
	}

	public void RemoveBlock(Block block)
	{
		if (!Blocks.Contains(block))
		{
			_logger.LogError("Block not found");
			return;
		}
		BlockedUser blockedUser = new BlockedUser(block.BlockId, block.BlockedPlayer.SocialId);
		_socialSdk.UnblockUser(blockedUser).Then<Result<Success, HasbroGo.Errors.Error>>(delegate(Result<Success, HasbroGo.Errors.Error> result)
		{
			if (result.IsOk)
			{
				block.BlockedPlayer.SetAsBlocked(isBlocked: false);
				Blocks.Remove(block);
				InvokeActionOnMainThread(this.BlocksChanged);
			}
			else
			{
				_logger.LogError(result.Error.Message);
			}
		});
	}

	public void ShowEnteringQueueWithOutgoingChallengeMessage(Action onOkAction)
	{
		string localizedText = Languages.ActiveLocProvider.GetLocalizedText("MainNav/FriendChallenge/ClosePlayBladeDuringDeckSelect_Header");
		string localizedText2 = Languages.ActiveLocProvider.GetLocalizedText("MainNav/FriendChallenge/CancelChallenge_EnteringQueue_Desc");
		List<SystemMessageManager.SystemMessageButtonData> list = new List<SystemMessageManager.SystemMessageButtonData>();
		SystemMessageManager.SystemMessageButtonData item = new SystemMessageManager.SystemMessageButtonData
		{
			Text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/GoBack")
		};
		list.Add(item);
		SystemMessageManager.SystemMessageButtonData item2 = new SystemMessageManager.SystemMessageButtonData
		{
			Text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Popups/Modal/ModalOptions_OK"),
			Callback = delegate
			{
				if (_challengeController == null)
				{
					_challengeController = Pantry.Get<PVPChallengeController>();
				}
				_challengeController.RejectAllChallengeInvites();
				_challengeController.CancelAllChallenges();
				onOkAction?.Invoke();
			}
		};
		list.Add(item2);
		SystemMessageManager.Instance.ShowMessage(localizedText, localizedText2, list);
	}

	public Wizards.Arena.Promises.Promise<string> GetPlayerIdFromFullPlayerName(string fullPlayerName)
	{
		SimplePromise<string> ret = new SimplePromise<string>();
		_socialSdk.GetProfileViaDisplayName(fullPlayerName).Then<Result<PublicProfileViaDisplayName, HasbroGo.Errors.Error>>(delegate(Result<PublicProfileViaDisplayName, HasbroGo.Errors.Error> result)
		{
			if (result.IsOk)
			{
				ret.SetResult(result.Value.PersonaId);
			}
			else
			{
				ret.SetError(new Wizards.Arena.Promises.Error(0, result.Error.Message));
			}
		});
		return ret;
	}

	public void ForwardNotificationAlert(SocialMessage socialMessage)
	{
		ChatManager.ManualNotificationAlert(socialMessage);
	}

	private IEnumerator Coroutine_DelayConnect(float secondsDelayed)
	{
		yield return new WaitForSeconds(secondsDelayed);
		if (Connected)
		{
			DoDisconnectSequence().Then((Action)Connect);
		}
		else
		{
			Connect();
		}
	}

	private void OnFrontDoorClosed(TcpConnectionCloseType closeType, string closeReason)
	{
		if (closeType == TcpConnectionCloseType.ClientSideIdle || closeType == TcpConnectionCloseType.ClosedByServer || closeType == TcpConnectionCloseType.SocketFailure)
		{
			ConnectionManager connectionManager = _connectionManager;
			connectionManager.OnFdReconnected = (Action)Delegate.Combine(connectionManager.OnFdReconnected, new Action(OnFrontDoorReconnected));
			SendPresenceUpdateForLocalPlayer(PresenceStatus.Offline);
			Disconnect(shutdownStyle: false);
		}
	}

	private void OnFrontDoorReconnected()
	{
		ConnectionManager connectionManager = _connectionManager;
		connectionManager.OnFdReconnected = (Action)Delegate.Remove(connectionManager.OnFdReconnected, new Action(OnFrontDoorReconnected));
		Connect();
	}

	public Result<GetTokenResult, TokenError> GetAccessToken()
	{
		Result<GetTokenResult, TokenError> result = default(Result<GetTokenResult, TokenError>);
		if (_accountClient.CurrentLoginState == LoginState.FullyRegisteredLogin && _accountClient.AccountInformation != null)
		{
			if (_accountClient.AccountInformation.Credentials.IsExpired())
			{
				TokenError tokenError = new TokenError("Access token is expired");
				_logger.LogError(tokenError.ErrorMessage);
				return tokenError;
			}
			return new GetTokenResult(_accountClient.AccountInformation.Credentials.Jwt);
		}
		return new TokenError("Trying to get access token but we're not logged in to an account.");
	}

	public async Task<Result<GetTokenResult, TokenError>> GetAccessTokenAsync()
	{
		Result<GetTokenResult, TokenError> result = default(Result<GetTokenResult, TokenError>);
		if (_accountClient.CurrentLoginState == LoginState.FullyRegisteredLogin && _accountClient.AccountInformation != null)
		{
			if (_accountClient.AccountInformation.Credentials.IsExpired())
			{
				await _accountClient.RefreshAccessToken().Then(delegate(Wizards.Arena.Promises.Promise<LoginResponse> loginResponse)
				{
					if (loginResponse.Successful)
					{
						result = new GetTokenResult(loginResponse.Result.access_token);
					}
					else
					{
						if (loginResponse.Error.Exception != null)
						{
							_logger.LogError(loginResponse.Error.Exception.Message);
						}
						string errorMessage = WASUtils.ToAccountError(loginResponse.Error).ErrorMessage;
						if (errorMessage != null)
						{
							_logger.LogError(errorMessage);
						}
						result = new TokenError(errorMessage);
					}
				}).AsTask;
			}
			else
			{
				result = new GetTokenResult(_accountClient.AccountInformation.Credentials.Jwt);
			}
		}
		else
		{
			string message = "Trying to get unexpired access token but we're not logged in to an account.";
			_logger.LogError(message);
			result = new TokenError(message);
		}
		return result;
	}

	private IPromise GetUnexpiredAccessToken()
	{
		Promise promise = new Promise();
		GetAccessTokenAsync().Then(delegate(Result<GetTokenResult, TokenError> result)
		{
			if (result.IsOk)
			{
				MainThreadDispatcher.Instance.Add(delegate
				{
					Connected = true;
					promise.Resolve();
				});
			}
			else
			{
				promise.Reject(new Exception(result.Error.ErrorMessage));
			}
		});
		return promise;
	}

	private IPromise WaitForSocialServiceInit()
	{
		if (_sdkInitRetryCount == 0)
		{
			_sdkInitPromise = new Promise();
		}
		if (_accountClient.CurrentLoginState == LoginState.FullyRegisteredLogin)
		{
			this.UserLoggedOut?.Invoke(this, new LogoutEvent(), default(CancellationToken)).Then(delegate
			{
				this.UserLoggedIn?.Invoke(this, new LoginSuccessfulEvent(_accountClient.AccountInformation.DisplayName, _accountClient.AccountInformation.AccountID, "Login"), default(CancellationToken)).Then(delegate
				{
					_sdkInitPromise.Resolve();
					_sdkInitRetryCount = 0;
				}).TryCatch(delegate(Exception exception)
				{
					_sdkInitPromise.Reject(exception);
					_sdkInitRetryCount = 0;
				});
			}).TryCatch(delegate(Exception exception)
			{
				_sdkInitPromise.Reject(exception);
				_sdkInitRetryCount = 0;
			});
		}
		else if (_sdkInitRetryCount < 5)
		{
			_sdkInitRetryCount++;
			DelayServiceInitAttempt(500);
		}
		else
		{
			_sdkInitPromise.Reject(new Exception($"({DateTimeOffset.UtcNow:T}) Account login did not happen, failing SocialService init"));
			_sdkInitRetryCount = 0;
		}
		return _sdkInitPromise;
	}

	private async Task DelayServiceInitAttempt(int delay)
	{
		await Task.Delay(delay);
		WaitForSocialServiceInit();
	}

	private IPromise DoConnectSequence()
	{
		if (LocalPlayer.Status == PresenceStatus.Offline)
		{
			string message = "Player is Offline and we're trying to connect. Update LocalPlayer to Online to connect.";
			_logger.LogError(message);
			return Promise.Rejected(new Exception(message));
		}
		if (_connectionSequencePromise != null)
		{
			return _connectionSequencePromise;
		}
		return _connectionSequencePromise = WaitForSocialServiceInit().Then((Func<IPromise>)GetUnexpiredAccessToken).Then((Func<IPromise>)SyncPresenceWithBackend).Then((Func<IPromise>)GetLocalPlayerPresence)
			.Then((Func<IPromise>)GetFriendsFromPlatform)
			.Then((Func<IPromise>)GetIncomingFriendInvites)
			.Then((Func<IPromise>)GetOutgoingFriendInvites)
			.Then((Func<IPromise>)GetBlocks)
			.Then(delegate
			{
				SetSocialModeToPublic();
				_reconnectAttempts = 0;
				_fdc.OnForceClosed += OnFrontDoorClosed;
				InvokeActionOnMainThread(this.ConnectionChanged, false);
				BIMessage_OnlineFriendsOnConnect(Friends.Count((SocialEntity f) => f.IsOnline), Friends.Count());
			})
			.Catch(delegate(Exception exception)
			{
				_logger.LogException(exception);
				Connected = false;
			})
			.Finally(delegate
			{
				_connectionSequencePromise = null;
			});
	}

	private void InvokeActionOnMainThread<T>(Action<T> action, object param = null)
	{
		if (action == null)
		{
			return;
		}
		if (param is T)
		{
			T typedParam = (T)param;
			MainThreadDispatcher.Instance.Add(delegate
			{
				action(typedParam);
			});
		}
		else
		{
			_logger.LogError("Invoke parameter type mismatch");
		}
	}

	private void InvokeActionOnMainThread(Action action)
	{
		if (action != null)
		{
			MainThreadDispatcher.Instance.Add(action.Invoke);
		}
	}

	private IPromise DoDisconnectSequence()
	{
		if (_socialSdk == null)
		{
			return new Promise(delegate(Action resolve, Action<Exception> reject)
			{
				reject(new Exception("SDK is null, failed to disconnect."));
			});
		}
		Connected = false;
		if (Blocks.Count > 0)
		{
			Blocks.Clear();
			InvokeActionOnMainThread(this.BlocksChanged);
		}
		if (InvitesIncoming.Count > 0)
		{
			InvitesIncoming.Clear();
			InvokeActionOnMainThread(this.InvitesIncomingChanged);
		}
		if (InvitesOutgoing.Count > 0)
		{
			InvitesOutgoing.Clear();
			InvokeActionOnMainThread(this.InvitesOutgoingChanged);
		}
		if (Friends.Count > 0)
		{
			Friends.Clear();
			InvokeActionOnMainThread(this.FriendsChanged);
		}
		if (_maintainPresenceCoroutine != null)
		{
			PAPA.StopGlobalCoroutine(_maintainPresenceCoroutine);
			_maintainPresenceCoroutine = null;
		}
		InvokeActionOnMainThread(this.ConnectionChanged, false);
		Promise promise = new Promise();
		BIMessage_ChangeSocialMode(LocalPlayer.Status, PresenceStatus.Offline, SocialModeValidity.Verified);
		promise.Resolve();
		_fdc.OnForceClosed -= OnFrontDoorClosed;
		return promise;
	}

	private void InitLocalPlayerPresence()
	{
		PresenceStatus presenceStatus = (PresenceStatus)MDNPlayerPrefs.LocalPlayerPresence;
		LocalPlayer.SetPresence(presenceStatus);
		MainThreadDispatcher.Instance.Add(delegate
		{
			this.LocalPresenceStatusChanged?.Invoke(PresenceStatus.Offline, presenceStatus);
		});
	}

	private PlatformStatus GetPlatformStatusFromPresenceStatus(PresenceStatus status)
	{
		switch (status)
		{
		case PresenceStatus.Offline:
			return PlatformStatus.Offline;
		case PresenceStatus.Available:
			return PlatformStatus.Playing;
		case PresenceStatus.Away:
			return PlatformStatus.Idle;
		case PresenceStatus.Busy:
			return PlatformStatus.Busy;
		default:
			_logger.LogError("Unknown Presence status " + status);
			return PlatformStatus.Offline;
		}
	}

	private IPromise SyncPresenceWithBackend()
	{
		return UpdatePresenceForLocalPlayer(_previousStatus, LocalPlayer.Status);
	}

	private IPromise GetLocalPlayerPresence()
	{
		Promise ret = new Promise();
		_socialSdk.GetPresence().Then<Result<Presence, HasbroGo.Errors.Error>>(delegate(Result<Presence, HasbroGo.Errors.Error> result)
		{
			if (result.IsOk)
			{
				LocalPlayer.UpdateIds(result.Value, _accountClient.AccountInformation);
				ret.Resolve();
			}
			else
			{
				ret.Reject(new Exception(result.Error.Message));
			}
		});
		return ret;
	}

	private void SetSocialModeToPublic()
	{
		_socialSdk.UpdateSocialMode(SocialMode.Public).Then<Result<Success, HasbroGo.Errors.Error>>(delegate(Result<Success, HasbroGo.Errors.Error> result)
		{
			if (!result.IsOk)
			{
				_logger.LogError("Error setting social mode to public." + result.Error.Message);
			}
		});
	}

	private IPromise UpdatePresenceForLocalPlayer(PresenceStatus previousStatus, PresenceStatus newStatus)
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			MDNPlayerPrefs.LocalPlayerPresence = (int)newStatus;
			this.LocalPresenceStatusChanged?.Invoke(previousStatus, newStatus);
		});
		LocalPlayer.SetPresence(newStatus);
		return SendPresenceUpdateForLocalPlayer(newStatus);
	}

	private IPromise SendPresenceUpdateForLocalPlayer(PresenceStatus newStatus)
	{
		Promise ret = new Promise();
		if (newStatus == PresenceStatus.Offline)
		{
			RemoveHandlers();
		}
		else
		{
			AddHandlers();
		}
		if (Connected)
		{
			PlatformStatus platformStatusFromPresenceStatus = GetPlatformStatusFromPresenceStatus(newStatus);
			_socialSdk.UpdatePresence(platformStatusFromPresenceStatus).Then<Result<Presence, HasbroGo.Errors.Error>>(delegate(Result<Presence, HasbroGo.Errors.Error> result)
			{
				if (result.IsOk)
				{
					BIMessage_ChangeSocialMode(_previousStatus, newStatus, SocialModeValidity.Verified);
					ret.Resolve();
				}
				else
				{
					_logger.LogError(result.Error.Message);
					ret.Reject(new Exception(result.Error.Message));
				}
			});
		}
		else
		{
			ret.Resolve();
		}
		return ret;
	}

	private IPromise GetFriendsFromPlatform()
	{
		Promise ret = new Promise();
		_socialSdk.GetFriendsWithPresence().Then<Result<IEnumerable<FriendWithPresence>, HasbroGo.Errors.Error>>(delegate(Result<IEnumerable<FriendWithPresence>, HasbroGo.Errors.Error> result)
		{
			if (result.IsOk)
			{
				Friends.Clear();
				foreach (FriendWithPresence item in result.Value)
				{
					Friends.Add(new SocialEntity(item));
				}
				InvokeActionOnMainThread(this.FriendsChanged);
				ret.Resolve();
			}
			else
			{
				_logger.LogError(result.Error.Message);
				ret.Reject(new Exception(result.Error.Message));
			}
		});
		return ret;
	}

	private IPromise GetBlocks()
	{
		Promise promise = new Promise();
		GetBlockListRequest request = new GetBlockListRequest(50);
		_socialSdk.GetBlockedUsers(request).Then<Result<IEnumerable<BlockedUser>, HasbroGo.Errors.Error>>(delegate(Result<IEnumerable<BlockedUser>, HasbroGo.Errors.Error> result)
		{
			if (result.IsOk)
			{
				Blocks.Clear();
				foreach (BlockedUser item in result.Value)
				{
					Blocks.Add(new Block(item));
				}
				InvokeActionOnMainThread(this.BlocksChanged);
				promise.Resolve();
			}
			else
			{
				_logger.LogError(result.Error.Message);
				promise.Reject(new Exception(result.Error.Message));
			}
		});
		return promise;
	}

	private IPromise GetIncomingFriendInvites()
	{
		Promise ret = new Promise();
		_socialSdk.GetIncomingFriendInvites().Then<Result<IEnumerable<IncomingFriendInvite>, HasbroGo.Errors.Error>>(delegate(Result<IEnumerable<IncomingFriendInvite>, HasbroGo.Errors.Error> result)
		{
			if (result.IsOk)
			{
				InvitesIncoming.Clear();
				foreach (IncomingFriendInvite item in result.Value)
				{
					Invite invite = new Invite(item);
					InvitesIncoming.Add(invite);
					if (DeclineIncomingFriendRequests)
					{
						DeclineFriendInviteIncoming(invite);
					}
				}
				if (InvitesIncoming.Count > 0)
				{
					InvokeActionOnMainThread(this.InviteIncomingAdded, new Invite(result.Value.FirstOrDefault()));
				}
				InvokeActionOnMainThread(this.InvitesIncomingChanged);
				ret.Resolve();
			}
			else
			{
				_logger.LogError(result.Error.Message);
				ret.Reject(new Exception(result.Error.Message));
			}
		});
		return ret;
	}

	private IPromise GetOutgoingFriendInvites()
	{
		Promise ret = new Promise();
		_socialSdk.GetOutgoingFriendInvites().Then<Result<IEnumerable<OutgoingFriendInvite>, HasbroGo.Errors.Error>>(delegate(Result<IEnumerable<OutgoingFriendInvite>, HasbroGo.Errors.Error> result)
		{
			if (result.IsOk)
			{
				InvitesOutgoing.Clear();
				foreach (OutgoingFriendInvite item in result.Value)
				{
					InvitesOutgoing.Add(new Invite(item));
				}
				InvokeActionOnMainThread(this.InvitesOutgoingChanged);
				ret.Resolve();
			}
			else
			{
				_logger.LogError(result.Error.Message);
				ret.Reject(new Exception(result.Error.Message));
			}
		});
		return ret;
	}

	private void HandleIncomingInviteAdded(object sender, IncomingFriendInviteReceivedEventArgs obj)
	{
		Invite invite = InvitesIncoming.Find((Invite i) => i.InviteId == obj.IncomingFriendInvite.InviteId);
		if (invite == null)
		{
			invite = new Invite(obj.IncomingFriendInvite);
			InvitesIncoming.Add(invite);
			if (DeclineIncomingFriendRequests)
			{
				DeclineFriendInviteIncoming(invite);
			}
			else
			{
				InvokeActionOnMainThread(this.InviteIncomingAdded, invite);
			}
			InvokeActionOnMainThread(this.InvitesIncomingChanged);
		}
	}

	private void HandleIncomingInviteRemoved(object sender, FriendInviteRevokedEventArgs obj)
	{
		Invite invite = InvitesIncoming.Find((Invite i) => i.InviteId == obj.FriendRequestReference.InviteId);
		if (invite != null)
		{
			InvitesIncoming.Remove(invite);
			InvokeActionOnMainThread(this.InviteIncomingRemoved, invite);
			InvokeActionOnMainThread(this.InvitesIncomingChanged);
		}
	}

	private void HandleOutgoingInviteAdded(object sender, FriendInviteSentEventArgs obj)
	{
		OutgoingFriendInvite sdkInvite = obj.OutgoingFriendInvite;
		Invite invite = InvitesOutgoing.Find((Invite i) => i.InviteId == sdkInvite.InviteId);
		if (invite == null)
		{
			invite = new Invite(sdkInvite);
			InvitesOutgoing.Add(invite);
			InvokeActionOnMainThread(this.InviteOutgoingAdded, invite);
			InvokeActionOnMainThread(this.InvitesOutgoingChanged);
		}
	}

	private void HandleFriendInviteRemoved(object sender, FriendInviteRemovedEventArgs obj)
	{
		Invite invite = InvitesOutgoing.Find((Invite i) => i.InviteId == obj.FriendRequestReference.InviteId);
		if (invite != null)
		{
			RemoveOutgoingFriendInvite(invite);
		}
	}

	private void HandleFriendAdded(object sender, FriendAddedEventArgs obj)
	{
		SocialEntity friend = Friends.Find((SocialEntity f) => f.SocialId == obj.Friend.AccountId);
		if (friend != null)
		{
			return;
		}
		friend = new SocialEntity(obj.Friend);
		Friends.Add(friend);
		List<Invite> invitesOutgoing = InvitesOutgoing;
		if (invitesOutgoing != null && invitesOutgoing.Count > 0)
		{
			Invite invite = InvitesOutgoing.Find((Invite i) => i.PotentialFriend.FullName == friend.FullName);
			if (invite != null)
			{
				RemoveOutgoingFriendInvite(invite);
			}
			else
			{
				RefreshFriendsData();
			}
		}
		_socialSdk.GetFriendPresence(obj.Friend).Then<Result<FriendWithPresence, HasbroGo.Errors.Error>>(delegate(Result<FriendWithPresence, HasbroGo.Errors.Error> result)
		{
			if (result.IsOk)
			{
				friend.SetPresence(result.Value.Presence);
			}
			else
			{
				_logger.LogError($"({DateTimeOffset.UtcNow:T}) Failed to get Presence for new friend: {obj.Friend.DisplayName}");
			}
			InvokeActionOnMainThread(this.FriendAdded, friend);
			InvokeActionOnMainThread(this.FriendsChanged);
		});
	}

	private void HandleRemoveFriend(object sender, FriendRemovedEventArgs obj)
	{
		SocialEntity socialEntity = Friends.Find((SocialEntity f) => f.SocialId == obj.RemovedFriendAccountId);
		if (socialEntity != null)
		{
			socialEntity.SetPresence(PresenceStatus.Offline);
			InvokeActionOnMainThread(this.FriendPresenceChanged, socialEntity);
			Friends.Remove(socialEntity);
			InvokeActionOnMainThread(this.FriendRemoved, socialEntity);
			InvokeActionOnMainThread(this.FriendsChanged);
		}
	}

	private void HandleSocialEnabledChanged(bool isSocialEnabled)
	{
		if (isSocialEnabled)
		{
			InitSocialService();
			Connect();
			return;
		}
		if (Connected)
		{
			string localizedText = Languages.ActiveLocProvider.GetLocalizedText("Social/Errors/KillSwitchEnabled_Header");
			string localizedText2 = Languages.ActiveLocProvider.GetLocalizedText("Social/Errors/KillSwitchEnabled_Desc");
			SystemMessageManager.ShowSystemMessage(localizedText, localizedText2);
		}
		Destroy();
	}

	private void HandleFriendPresenceUpdate(object sender, PresenceUpdateReceivedEventArgs obj)
	{
		FriendWithPresence friendWithPresence = obj.FriendWithPresence;
		int friendIndex = Friends.FindIndex((SocialEntity f) => f.SocialId == friendWithPresence.Friend.AccountId);
		if (friendIndex == -1)
		{
			return;
		}
		Friends[friendIndex].SetPresence(friendWithPresence.Presence);
		InvokeActionOnMainThread(this.FriendPresenceChanged, Friends[friendIndex]);
		InvokeActionOnMainThread(this.FriendsChanged);
		if (Friends[friendIndex].Status == PresenceStatus.Offline)
		{
			if (_challengeController == null)
			{
				_challengeController = Pantry.Get<PVPChallengeController>();
			}
			InvokeActionOnMainThread(delegate
			{
				_challengeController.CancelChallengeInvite(Friends[friendIndex].PlayerId);
			});
		}
	}

	private void HandlePrivateMessage(object sender, DirectMessageReceivedEventArgs privateMessage)
	{
		foreach (SocialEntity friend in Friends)
		{
			if (friend.SocialId == privateMessage.Message.SenderAccountId)
			{
				InvokeActionOnMainThread(this.OnDirectMessage, new DirectMessage
				{
					Message = privateMessage.Message.Payload,
					SenderPlayerId = friend.PlayerId,
					SenderDisplayName = friend.FullName
				});
				return;
			}
		}
		_logger.LogError("Failed to find friend that sent a direct message.");
	}

	private void HandleGameMessage(object sender, GameMessageReceivedEventArgs gameMessageArgs)
	{
		GameMessage gameMessage = gameMessageArgs.GameMessage;
		InvokeActionOnMainThread(this.OnGameMessage, gameMessage);
	}

	private void RemoveOutgoingFriendInvite(Invite invite)
	{
		if (invite != null)
		{
			InvitesOutgoing.Remove(invite);
			InvokeActionOnMainThread(this.InviteOutgoingRemoved, invite);
			InvokeActionOnMainThread(this.InvitesOutgoingChanged);
		}
	}

	private void BIMessage_ChangeSocialMode(PresenceStatus oldStatus, PresenceStatus newStatus, SocialModeValidity validity)
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			_biLogger.Send(ClientBusinessEventType.SocialChangeSocialMode, new SocialChangeSocialMode
			{
				EventTime = DateTime.UtcNow,
				PlayerSocialId = LocalPlayer.SocialId,
				NewStatus = newStatus.ToString(),
				OldStatus = oldStatus.ToString(),
				SceneId = SceneManager.GetActiveScene().name,
				Validity = validity.ToString()
			});
		});
	}

	public void BIMessage_ChatWindowOpened()
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			_biLogger.Send(ClientBusinessEventType.SocialChatWindowOpened, new SocialChatWindowOpened
			{
				EventTime = DateTime.UtcNow,
				SceneId = SceneManager.GetActiveScene().name
			});
		});
	}

	public void BIMessage_DirectMessageRecieved(Conversation conversation)
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			_biLogger.Send(ClientBusinessEventType.SocialDirectMessageReceived, new SocialDirectMessageReceived
			{
				EventTime = DateTime.UtcNow,
				PlayerSocialId = LocalPlayer.SocialId,
				SceneId = SceneManager.GetActiveScene().name,
				ConversationId = conversation.ConversationId,
				RelationshipId = conversation.RelationshipId,
				SenderPlayerId = conversation.Friend.PlayerId,
				SenderPlayerSocialId = conversation.Friend.SocialId
			});
		});
	}

	public void BIMessage_DirectMessageSent(Conversation conversation)
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			_biLogger.Send(ClientBusinessEventType.SocialDirectMessageSent, new SocialDirectMessageSent
			{
				EventTime = DateTime.UtcNow,
				PlayerSocialId = LocalPlayer.SocialId,
				SceneId = SceneManager.GetActiveScene().name,
				ConversationId = conversation.ConversationId,
				RelationshipId = conversation.RelationshipId,
				RecipientPlayerId = conversation.Friend.PlayerId,
				RecipientSocialId = conversation.Friend.SocialId
			});
		});
	}

	public void BIMessage_FriendsListOpened()
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			_biLogger.Send(ClientBusinessEventType.SocialFriendsListOpened, new SocialFriendsListOpened
			{
				EventTime = DateTime.UtcNow,
				PlayerSocialId = LocalPlayer.SocialId,
				SceneId = SceneManager.GetActiveScene().name
			});
		});
	}

	public void BIMessage_NotificationBubbleShown(SocialMessage notification)
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			_biLogger.Send(ClientBusinessEventType.SocialNotificationBubbleShown, new SocialNotificationBubbleShown
			{
				EventTime = DateTime.UtcNow,
				PlayerSocialId = LocalPlayer.SocialId,
				TypeOfBubbleShown = notification.Type.ToString(),
				SceneId = SceneManager.GetActiveScene().name,
				SenderPlayerId = notification.Friend.PlayerId,
				SenderPlayerSocialId = notification.Friend.SocialId
			});
		});
	}

	private void BIMessage_OnlineFriendsOnConnect(int onlinePlayers, int totalPlayers)
	{
		_biLogger.Send(ClientBusinessEventType.SocialOnlineFriendsOnConnect, new SocialOnlineFriendsOnConnect
		{
			EventTime = DateTime.UtcNow,
			PlayerSocialId = LocalPlayer.SocialId,
			PlayersOnFriendsListOnline = onlinePlayers,
			PlayersOnFriendsListTotal = totalPlayers
		});
	}

	private void BIMessage_ToggleBlockIncomingFriendRequests(bool blockFriends)
	{
		_biLogger.Send(ClientBusinessEventType.SocialToggleBlockIncomingFriendRequests, new SocialToggleBlockIncomingFriendRequests
		{
			EventTime = DateTime.UtcNow,
			PlayerSocialId = LocalPlayer.SocialId,
			BlockFriends = blockFriends
		});
	}

	private void BIMessage_AddFriend(SocialEntity targetPlayer)
	{
		_biLogger.Send(ClientBusinessEventType.SocialAddFriend, new SocialAddFriend
		{
			EventTime = DateTime.UtcNow,
			PlayerSocialId = LocalPlayer.SocialId,
			TargetPlayerId = targetPlayer.PlayerId,
			TargetPlayerSocialId = targetPlayer.SocialId
		});
	}

	private void BIMessage_RemoveFriend(SocialEntity targetPlayer)
	{
		_biLogger.Send(ClientBusinessEventType.SocialRemoveFriend, new SocialRemoveFriend
		{
			EventTime = DateTime.UtcNow,
			PlayerSocialId = LocalPlayer.SocialId,
			TargetPlayerId = targetPlayer.PlayerId,
			TargetPlayerSocialId = targetPlayer.SocialId
		});
	}

	private void BIMessage_BlockPlayer(SocialEntity targetPlayer)
	{
		_biLogger.Send(ClientBusinessEventType.SocialBlockPlayer, new SocialBlockPlayer
		{
			EventTime = DateTime.UtcNow,
			PlayerSocialId = LocalPlayer.SocialId,
			TargetPlayerId = targetPlayer.PlayerId,
			TargetPlayerSocialId = targetPlayer.SocialId
		});
	}

	public void Dispose()
	{
		_clientFeatureToggleDataProvider.UnRegisterForToggleUpdates(UpdateSocialEnabled);
		SocialEnabledChanged -= HandleSocialEnabledChanged;
		Destroy();
	}
}
