using System;
using System.Collections.Generic;
using System.Net;
using AssetLookupTree;
using Core.Code.Promises;
using Core.Meta.MainNavigation.Challenge;
using Core.Shared.Code.DebugTools;
using GreClient.Network;
using Pooling;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wizards.Arena.Client.Logging;
using Wizards.Arena.Enums.System;
using Wizards.Arena.Promises;
using Wizards.Arena.TcpConnection;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Inventory;
using Wizards.Mtga.Logging;
using Wizards.Mtga.PlayBlade;
using Wizards.Mtga.PrivateGame;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtgo.Gre.External.Messaging;

public class Matchmaking
{
	private IObjectPool _objectPool = NullObjectPool.Default;

	private IMatchdoorServiceWrapper _matchdoorServiceWrapper;

	private MatchManager _matchManager;

	private IAccountClient _accountClient;

	private AccountInformation _accountInformation;

	private NPEState _npeState;

	private ICardDatabaseAdapter _cardDatabase;

	private bool _waitingForMatch;

	private EventContext _eventContext;

	private NewMatchCreatedConfig _cachedMatchConfig;

	private PVPChallengeData _challengeData;

	private UnityLogger _unityLogger;

	private ConnectionManager _connectionManager;

	private LoggingConfig _loggingConfig = new LoggingConfig
	{
		VerboseLogs = true
	};

	private Wizards.Arena.Client.Logging.ILogger _logger = new ConsoleLogger();

	private BotTool _botTool;

	private AssetLookupSystem _assetLookupSystem;

	private RecentlyPlayedDataProvider _recentlyPlayedDataProvider;

	private PVPChallengeController _challengeController;

	public event System.Action MatchReady;

	public event System.Action RemovedFromMatchmaking;

	public event Action<MatchManager> MatchManagerInitialized;

	public static Matchmaking Create()
	{
		return new Matchmaking();
	}

	public void Initialize(IObjectPool objectPool, IMatchdoorServiceWrapper matchmakingServiceWrapper, MatchManager matchManager, IAccountClient accountClient, NPEState npeState, ICardDatabaseAdapter cardDatabase, ConnectionManager connectionManager, LoggingConfig loggingConfig, Wizards.Arena.Client.Logging.ILogger logger, AssetLookupSystem assetLookupSystem)
	{
		_objectPool = objectPool;
		_matchManager = matchManager;
		_matchdoorServiceWrapper = matchmakingServiceWrapper;
		_accountClient = accountClient;
		_accountInformation = _accountClient.AccountInformation;
		_npeState = npeState;
		_cardDatabase = cardDatabase;
		_connectionManager = connectionManager;
		_loggingConfig = loggingConfig ?? new LoggingConfig
		{
			VerboseLogs = true
		};
		_logger = logger ?? new ConsoleLogger();
		_assetLookupSystem = assetLookupSystem;
		_botTool = Pantry.Get<BotTool>();
		_recentlyPlayedDataProvider = Pantry.Get<RecentlyPlayedDataProvider>();
		_matchdoorServiceWrapper.RegisterOnForcedDrop(OnForceDrop);
		_matchdoorServiceWrapper.RegisterOnChallengeForcedDrop(OnForceDrop_Challenge);
		_matchdoorServiceWrapper.RegisterOnMatchCreated(OnMatchCreated);
		_matchdoorServiceWrapper.RegisterOnAuthFailed(OnFrontDoorFailedToReconnect);
		_matchdoorServiceWrapper.RegisterOnConnectFailed(OnFrontDoorFailedToReconnect);
		_unityLogger = new UnityLogger("MatchMaking", LoggerLevel.Error);
		LoggerManager.Register(_unityLogger);
		ConnectionManager connectionManager2 = _connectionManager;
		connectionManager2.OnFdReconnected = (System.Action)Delegate.Combine(connectionManager2.OnFdReconnected, new System.Action(OnFdReconnected));
	}

	public void SetExpectedEvent(EventContext evt)
	{
		_eventContext = evt;
	}

	private void OnFdReconnected()
	{
		_accountInformation = _accountClient.AccountInformation;
		TryCancel(fdReconnected: true);
	}

	private void OnMatchCreated(NewMatchCreatedConfig matchConfig)
	{
		_ = _cachedMatchConfig;
		CleanupJoinMatchDataAndListeners();
		_cachedMatchConfig = matchConfig;
		this.MatchReady?.Invoke();
	}

	public bool IsMatchReady()
	{
		return _cachedMatchConfig != null;
	}

	public void JoinPendingMatch()
	{
		JoinMatch(_cachedMatchConfig);
	}

	private void OnFrontDoorFailedToReconnect(ServerErrors ErrorCode, string DebugErrorText)
	{
		_waitingForMatch = false;
	}

	private void OnFrontDoorFailedToReconnect(string _)
	{
		_waitingForMatch = false;
	}

	public void Destroy()
	{
		_matchdoorServiceWrapper.UnregisterOnForcedDrop(OnForceDrop);
		_matchdoorServiceWrapper.UnregisterOnChallengeForcedDrop(OnForceDrop_Challenge);
		_matchdoorServiceWrapper.UnregisterOnMatchCreated(OnMatchCreated);
		_matchdoorServiceWrapper.UnregisterOnAuthFailed(OnFrontDoorFailedToReconnect);
		_matchdoorServiceWrapper.UnregisterOnConnectFailed(OnFrontDoorFailedToReconnect);
		ConnectionManager connectionManager = _connectionManager;
		connectionManager.OnFdReconnected = (System.Action)Delegate.Remove(connectionManager.OnFdReconnected, new System.Action(OnFdReconnected));
	}

	private MatchManager InitializeMatchManager()
	{
		TcpConnection tcpConnection = new TcpConnection(_logger, 17, ServicePointManager.ServerCertificateValidationCallback);
		IGREConnection connection = new GREConnection(Translate.ToConnectMessage(_cardDatabase.VersionProvider.DataVersion), _loggingConfig, new MatchTcpConnection(tcpConnection), _logger, ClientType.User, RecordHistoryUtils.ShouldRecordHistory);
		_matchManager.Initialize(connection, _cardDatabase);
		_matchManager.MatchCompleted += OnMatchComplete;
		_matchManager.ConnectionFailed += OnMatchConnectionFailed;
		_matchManager.OnLostConnection += OnMatchConnectionLost;
		this.MatchManagerInitialized?.SafeInvoke(_matchManager);
		return _matchManager;
	}

	private void OnMatchConnectionFailed()
	{
		Debug.Log("Matchmaking: GRE connection failed");
		_connectionManager.RefreshMatchConnection();
	}

	private void OnMatchConnectionLost()
	{
		Debug.Log("Matchmaking: GRE connection lost");
		_connectionManager.RefreshMatchConnection();
	}

	private void OnMatchComplete()
	{
		if (_matchManager != null)
		{
			_matchManager.MatchCompleted -= OnMatchComplete;
			_matchManager.ConnectionFailed -= OnMatchConnectionFailed;
			_matchManager.OnLostConnection -= OnMatchConnectionLost;
		}
		if (!_connectionManager.Connected)
		{
			_connectionManager.Reconnect();
		}
	}

	public void SetupBotMatch(EventContext eventContext, LoadSceneMode loadMode)
	{
		_waitingForMatch = true;
		MatchManager matchManager = InitializeMatchManager();
		matchManager.StartAIBotMatch(LocalPlayerDisplayName());
		matchManager.SetEvent(eventContext);
		PAPA.SceneLoading.LoadDuelScene(loadMode);
	}

	public void SetupTournamentMatch()
	{
		_waitingForMatch = true;
		InitializeMatchManager();
		PAPA.SceneLoading.LoadDuelScene(LoadSceneMode.Additive);
	}

	public void SetupEventMatch(EventContext eventContext, LoadSceneMode loadMode = LoadSceneMode.Additive)
	{
		Guid deckId = eventContext?.PlayerEvent?.CourseData?.CourseDeck?.Id ?? Guid.Empty;
		_recentlyPlayedDataProvider.AddRecentlyPlayedGame(eventContext.PlayerEvent.EventInfo.EventId, deckId);
		_waitingForMatch = true;
		InitializeMatchManager().SetEvent(eventContext);
		PAPA.SceneLoading.LoadDuelScene(loadMode);
	}

	public void JoinChallenge(EventContext eventContext, PVPChallengeData challengeData)
	{
		if (_challengeController == null)
		{
			_challengeController = Pantry.Get<PVPChallengeController>();
		}
		Pantry.Get<ChallengeDataProvider>().RegisterForChallengeChanges(OnChallengeDataChanged);
		_challengeData = challengeData;
		_waitingForMatch = true;
		_eventContext = eventContext;
		MatchManager matchManager = InitializeMatchManager();
		matchManager.StartChallenge(challengeData);
		matchManager.SetEvent(eventContext);
		PAPA.SceneLoading.LoadDuelScene(LoadSceneMode.Additive);
		if (IsMatchReady())
		{
			this.MatchReady?.Invoke();
		}
	}

	private string LocalPlayerDisplayName()
	{
		if (_accountClient == null || _accountClient.AccountInformation == null)
		{
			return string.Empty;
		}
		return _accountClient.AccountInformation.DisplayName;
	}

	public void LaunchFromReconnect()
	{
		InitializeMatchManager().HasReconnected = true;
		PAPA.SceneLoading.LoadDuelScene(LoadSceneMode.Single);
	}

	public void JoinMatchFromReconnect(NewMatchCreatedConfig config, EventContext eventContext)
	{
		_eventContext = eventContext;
		_matchManager.SetEvent(eventContext);
		JoinMatch(config);
	}

	public void TryCancel(bool fdReconnected = false)
	{
		if (!_waitingForMatch)
		{
			if (!IsMatchReady() && MatchSceneManager.Instance != null)
			{
				MatchSceneManager.Instance.ExitMatchScene();
			}
			return;
		}
		if (_challengeData != null)
		{
			_challengeController.LeaveChallenge(_challengeData.ChallengeId, confirm: false);
			return;
		}
		_matchdoorServiceWrapper.DropFromMatchQueue(_eventContext.PlayerEvent.EventInfo.InternalEventName).IfSuccess(delegate
		{
			RemoveUserFromMatchmaking();
		}).ThenOnMainThreadIfError((Action<Error>)delegate
		{
			MatchSceneManager.Instance.ExitMatchScene();
		});
	}

	private void OnPrivateGameCancel(bool success)
	{
		if (success)
		{
			_matchManager.CancelPrivateGaming();
			RemoveUserFromMatchmaking();
		}
	}

	private void OnForceDrop(string eventId)
	{
		if (_eventContext == null || _eventContext.PlayerEvent.MatchMakingName == eventId)
		{
			Debug.Log("Force Dropped from matchmaking queue by Event (" + eventId + ")");
			RemoveUserFromMatchmaking();
		}
	}

	private void RemoveUserFromMatchmaking()
	{
		CleanupJoinMatchDataAndListeners();
		_eventContext = null;
		_cachedMatchConfig = null;
		this.RemovedFromMatchmaking?.Invoke();
	}

	private void CleanupJoinMatchDataAndListeners()
	{
		_waitingForMatch = false;
		if (_challengeData != null)
		{
			Pantry.Get<ChallengeDataProvider>().UnRegisterForChallengeChanges(OnChallengeDataChanged);
			_challengeController.SetLocalPlayerStatus(_challengeData.ChallengeId, SharedClientCore.SharedClientCore.Code.PVPChallenge.Models.PlayerStatus.NotReady);
		}
		_challengeData = null;
	}

	private void OnChallengeDataChanged(PVPChallengeData challenge)
	{
		if (challenge.ChallengeId == _challengeData.ChallengeId && (challenge.Status == ChallengeStatus.Removed || (!challenge.ChallengePlayers.ContainsKey(challenge.LocalPlayerId) && !challenge.Invites.ContainsKey(challenge.LocalPlayerId))))
		{
			if (_challengeController.ChallengePermissionState != ChallengeDataProvider.ChallengePermissionState.Restricted_ChallengesKillswitched)
			{
				_challengeController.ChallengePermissionState = ChallengeDataProvider.ChallengePermissionState.Normal;
			}
			OnPrivateGameCancel(success: true);
			MatchSceneManager.Instance?.ExitMatchScene();
		}
	}

	private void JoinMatch(NewMatchCreatedConfig matchConfig)
	{
		CleanupJoinMatchDataAndListeners();
		_cachedMatchConfig = null;
		SetPlayerInfo(_matchManager, matchConfig.YourSeatId, ConvertFromConfigs(matchConfig.PlayerInfos));
		_matchManager.BattlefieldId = ((string.IsNullOrEmpty(matchConfig.battlefield) && _matchManager.HasReconnected) ? MDNPlayerPrefs.ReconnectBattlefieldId : matchConfig.battlefield);
		bool flag = _eventContext?.PlayerEvent is IColorChallengePlayerEvent;
		_matchManager.IsPrivateGame = matchConfig.eventId == "DirectGame";
		_matchManager.IsPracticeGame = matchConfig.matchType == MatchType.Familiar && !flag;
		if (_matchManager.ChallengeData != null)
		{
			_matchManager.PrivateGameWaitingForMatchMade = false;
		}
		ConnectionConfig connectionConfig = new ConnectionConfig(matchConfig.matchEndpointHost, matchConfig.matchEndpointPort, _accountInformation.PersonaID, _accountInformation.Credentials.Jwt, Global.VersionInfo.ContentVersion.ToString(), MDNPlayerPrefs.InactivityTimeoutMs);
		_matchManager.ConnectAndJoinMatch(connectionConfig, matchConfig.controllerFabricUri, matchConfig.matchId);
		if (matchConfig.matchType == MatchType.NPE)
		{
			string nPEOpponentByGameNumber = AvatarIdConstants.GetNPEOpponentByGameNumber(_npeState.ActiveNPEGameNumber);
			_matchManager.LocalPlayerInfo.AvatarSelection = "Avatar_Basic_NPE";
			_matchManager.OpponentInfo.AvatarSelection = nPEOpponentByGameNumber;
			foreach (MatchManager.PlayerInfo player in _matchManager.Players)
			{
				player.AvatarSelection = ((player.SeatId == matchConfig.YourSeatId) ? "Avatar_Basic_NPE" : nPEOpponentByGameNumber);
			}
			HeadlessClient headlessClient = _npeState.NewNPEOpponent(_objectPool, _cardDatabase, _npeState.ActiveNPEGameNumber);
			headlessClient.ConnectAndJoinMatch(connectionConfig.CreateFamiliar(), matchConfig.controllerFabricUri, matchConfig.matchId);
			_matchManager.RegisterFamiliarClient(headlessClient);
		}
		else if (matchConfig.matchType == MatchType.Familiar)
		{
			HeadlessClient headlessClient2 = CreateFamiliarClient(_botTool, _cardDatabase);
			headlessClient2.ConnectAndJoinMatch(connectionConfig.CreateFamiliar(), matchConfig.controllerFabricUri, matchConfig.matchId);
			_matchManager.RegisterFamiliarClient(headlessClient2);
		}
	}

	private IEnumerable<MatchManager.PlayerInfo> ConvertFromConfigs(IEnumerable<MatchPlayerConfig> playerConfigs)
	{
		foreach (MatchPlayerConfig playerConfig in playerConfigs)
		{
			yield return ConvertFromConfig(playerConfig);
		}
	}

	private MatchManager.PlayerInfo ConvertFromConfig(MatchPlayerConfig playerConfig)
	{
		MatchManager.PlayerInfo playerInfo = new MatchManager.PlayerInfo(playerConfig.ScreenName);
		playerInfo.SeatId = playerConfig.SeatId;
		playerInfo.TeamId = playerConfig.TeamId;
		playerInfo.IsWotc = playerConfig.IsWotc;
		playerInfo.CommanderGrpIds = new List<uint>(playerConfig.CommanderGrpIds);
		playerInfo.RankingClass = (Enum.TryParse<RankingClassType>(playerConfig.RankClass, out var result) ? result : RankingClassType.None);
		playerInfo.RankingTier = playerConfig.RankTier;
		playerInfo.MythicPercentile = playerConfig.MythicPercentile;
		playerInfo.MythicPlacement = playerConfig.MythicLeaderboardPlace;
		playerInfo.AvatarSelection = playerConfig.AvatarSelection;
		playerInfo.SleeveSelection = playerConfig.CardBackSelection;
		playerInfo.TitleSelection = playerConfig.TitleSelection;
		playerInfo.EmoteSelection = playerConfig.EmoteSelections;
		string petName = playerConfig.PetName;
		if (!string.IsNullOrEmpty(petName))
		{
			playerInfo.PetSelection = new ClientPetSelection
			{
				name = petName,
				variant = playerConfig.PetVariant
			};
		}
		if (string.IsNullOrEmpty(ProfileUtilities.GetAvatarBustImagePath(_assetLookupSystem, playerInfo.AvatarSelection)))
		{
			playerInfo.AvatarSelection = ProfileUtilities.GetRandomAvatarId();
		}
		return playerInfo;
	}

	private static void SetPlayerInfo(MatchManager matchManager, uint mySeatId, IEnumerable<MatchManager.PlayerInfo> playerInfo)
	{
		matchManager.Players.Clear();
		matchManager.Players.AddRange(playerInfo);
		copyData(matchManager.LocalPlayerInfo, matchManager.Players.Find(mySeatId, (MatchManager.PlayerInfo x, uint seatId) => x.SeatId == seatId));
		copyData(matchManager.OpponentInfo, matchManager.Players.Find(mySeatId, (MatchManager.PlayerInfo x, uint seatId) => x.SeatId != seatId));
		static void copyData(MatchManager.PlayerInfo copyTo, MatchManager.PlayerInfo copyFrom)
		{
			if (copyTo != null)
			{
				copyTo.SeatId = copyFrom.SeatId;
				copyTo.TeamId = copyFrom.TeamId;
				copyTo.ScreenName = copyFrom.ScreenName;
				copyTo.IsWotc = copyFrom.IsWotc;
				copyTo.RankingClass = copyFrom.RankingClass;
				copyTo.RankingTier = copyFrom.RankingTier;
				copyTo.MythicPercentile = copyFrom.MythicPercentile;
				copyTo.MythicPlacement = copyFrom.MythicPlacement;
				copyTo.CommanderGrpIds = copyFrom.CommanderGrpIds;
				copyTo.AvatarSelection = copyFrom.AvatarSelection;
				copyTo.SleeveSelection = copyFrom.SleeveSelection;
				copyTo.TitleSelection = copyFrom.TitleSelection;
				copyTo.PetSelection = copyFrom.PetSelection;
				copyTo.EmoteSelection = copyFrom.EmoteSelection;
			}
		}
	}

	private void OnForceDrop_Challenge(EDirectChallengeMismatch[] reasons)
	{
		var (title, text) = Utils.GetChallengeErrorMessages(reasons);
		SystemMessageManager.Instance.ShowOk(title, text);
		if (_challengeController != null && _challengeData != null)
		{
			_challengeController.LeaveChallenge(_challengeData.ChallengeId, confirm: false);
		}
		if (MatchSceneManager.Instance != null)
		{
			MatchSceneManager.Instance.ExitMatchScene();
		}
	}

	private HeadlessClient CreateFamiliarClient(BotTool botConfig, ICardDatabaseAdapter cardDB)
	{
		UnityFamiliar unityFamiliar = UnityEngine.Object.FindObjectOfType<UnityFamiliar>();
		if (unityFamiliar == null)
		{
			GameObject gameObject = new GameObject("UnityFamiliar");
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			unityFamiliar = gameObject.AddComponent<UnityFamiliar>();
		}
		HeuristicStrategy strategy = new HeuristicStrategy(botConfig.MinWaitTime, botConfig.MaxWaitTime, botConfig.MaxIdleTime, botConfig.DeckHeuristic, botConfig.AttackConfig, botConfig.BlockConfig, unityFamiliar, cardDB);
		Wizards.Arena.Client.Logging.ILogger logger = new ConsoleLogger();
		TcpConnection tcpConnection = new TcpConnection(logger, 17, ServicePointManager.ServerCertificateValidationCallback);
		return new HeadlessClient(new GREConnection(Translate.ToConnectMessage(cardDB.VersionProvider.DataVersion, 2u), new LoggingConfig(), new MatchTcpConnection(tcpConnection), logger, ClientType.Familiar, RecordHistoryUtils.ShouldRecordHistory), strategy, cardDB);
	}
}
