using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AssetLookupTree;
using Core;
using Core.Code.AssetBundles;
using Core.Code.ClientFeatureToggle;
using Core.Code.Familiar;
using Core.Shared.Code.Connection;
using Core.Shared.Code.DebugTools;
using Core.Shared.Code.Providers;
using Core.Shared.Code.ServiceFactories;
using Core.Shared.Code.Utilities;
using Cysharp.Threading.Tasks;
using GreClient.Network;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Wizards.Arena.Client.Logging;
using Wizards.Arena.Promises;
using Wizards.Arena.TcpConnection;
using Wizards.Mtga;
using Wizards.Mtga.Assets;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Inventory;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.NPE;
using Wotc.Mtga.DuelScene.UI.DEBUG;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Replays;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga;

public class View_Login : MonoBehaviour
{
	[Serializable]
	public struct UIPrefabs
	{
		public DebugConfigEditor DebugConfigEditor;

		public Debug_ReplayLauncher ReplayLauncher;
	}

	[Serializable]
	public readonly struct Server
	{
		public readonly string Name;

		public readonly string MatchDoorHost;

		public readonly int MatchDoorPort;

		public Server(string name, string host, int port)
		{
			Name = name;
			MatchDoorHost = host;
			MatchDoorPort = port;
		}
	}

	private string _debugConfigDirectory = string.Empty;

	private string _matchConfigDirectory = string.Empty;

	private string _deckDirectory = string.Empty;

	[SerializeField]
	private CanvasGroup _mainPanel;

	[SerializeField]
	private Button _joinButton;

	[SerializeField]
	private Button _createButton;

	[SerializeField]
	private Button _familiarButton;

	[SerializeField]
	private ValidationOutput _createMatchValidationOutput;

	[SerializeField]
	private ValidationOutput _familiarMatchValidationOutput;

	[SerializeField]
	private DebugRoomClipboard RoomClipboard;

	[SerializeField]
	private InputField _roomUriOverwrite;

	[SerializeField]
	private Dropdown _serverAddressSelectDropdown;

	[SerializeField]
	private Text _currentServerText;

	[SerializeField]
	private TextMeshProUGUI _screenName;

	[FormerlySerializedAs("_loadingAssetsLabel")]
	[SerializeField]
	private GameObject _loadingAssetsRoot;

	private Server[] _servers = Array.Empty<Server>();

	private Server _selectedServer;

	private GreClient.Network.MatchConfig _matchConfig;

	private IMatchConfigValidator _familiarConfigValidator;

	private IMatchConfigValidator _createMatchValidator;

	private PAPA _papa;

	private IAccountClient _accountClient;

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	private CardMaterialBuilder _cardMaterialBuilder;

	private AssetLookupSystem _assetLookupSystem;

	private MatchManager _matchManager;

	private IDeckConfigProvider _deckConfigProvider = NullDeckConfigProvider.Default;

	private IMatchConfigProvider _matchConfigProvider = NullMatchConfigProvider.Default;

	private IBattlefieldDataProvider _battlefieldDataProvider = NullBattlefieldDataProvider.Default;

	private IEmblemDataProvider _emblemDataProvider = NullEmblemDataProvider.Default;

	private IAvatarDataProvider _avatarDataProvider = NullAvatarDataProvider.Default;

	private ISleeveDataProvider _sleeveDataProvider = NullSleeveDataProvider.Default;

	private IPetDataProvider _petDataProvider = NullPetDataProvider.Default;

	private ITitlesDataProvider _titlesDataProvider = NullTitlesDataProvider.Default;

	private ICardStyleDataProvider _cardStyleDataProvider = NullCardStyleDataProvider.Default;

	[SerializeField]
	private GameObject _activeConfigPanelParent;

	[SerializeField]
	private Button _debugConfigButton;

	[SerializeField]
	private Button _replayButton;

	[SerializeField]
	private UIPrefabs _prefabs;

	private DebugConfigEditor _debugConfigEditor;

	private Debug_ReplayLauncher _replayLauncher;

	private string MyPlayerId => _accountClient?.AccountInformation?.PersonaID;

	private void Awake()
	{
		_debugConfigDirectory = Path.Combine(Application.persistentDataPath, "Debug");
		_matchConfigDirectory = Path.Combine(_debugConfigDirectory, "MatchConfigs");
		_deckDirectory = Path.Combine(_debugConfigDirectory, "Decks");
		_familiarButton.onClick.AddListener(OnPlayAgainstFamiliarClicked);
		_createButton.onClick.AddListener(OnCreateRoomClicked);
		_joinButton.onClick.AddListener(OnJoinRoomClicked);
		_debugConfigButton.onClick.AddListener(OpenDebugConfigEditor);
		_replayButton.onClick.AddListener(OpenReplayLauncher);
		_roomUriOverwrite.onValueChanged.AddListener(UpdateJoinRoomButton);
	}

	private IEnumerator Start()
	{
		_mainPanel.interactable = false;
		_loadingAssetsRoot.SetActive(value: true);
		_papa = UnityEngine.Object.FindObjectOfType<PAPA>();
		if (_papa == null)
		{
			PantryInitializer.InitializePantry();
			LoggingUtils.Initialize();
			EnvironmentManager environmentManager = Pantry.Get<EnvironmentManager>();
			environmentManager.InitializeEnvironment();
			environmentManager.SetEnvironmentByName(MDNPlayerPrefs.PreviousFDServer);
			_papa = PAPA.Create();
			yield return null;
			FrontDoorConnectionManager frontDoorConnectionManager = Pantry.Get<FrontDoorConnectionManager>();
			new InitializeConnectionCommand().Execute(_papa.AssetLookupSystem, frontDoorConnectionManager, Pantry.Get<IFrontDoorConnectionServiceWrapper>(), _papa.MatchManager, _papa.Matchmaking, _papa.EventManager, _papa.KeyBoardManager, _papa.Actions, _papa.SettingsMenuHost);
			yield return Pantry.Get<ConnectionManager>().RingDoorbell();
			AutoLoginState state = AutoLoginState.None;
			yield return frontDoorConnectionManager.TryFastLogIn(delegate(AutoLoginState x)
			{
				state = x;
			});
			if (state != AutoLoginState.Connected)
			{
				Debug.LogError("Failed to log in. Better go do that (Or make this page support it).");
				yield break;
			}
			yield return PrepareAndDownloadAssets();
			yield return new LoadLocDatabaseUniTask().Load().ToCoroutine();
			yield return _papa.LoadCardDatabase();
			_papa.CreateFormatManager();
			_papa.SetupDeckFormatFactory();
			_papa.InitCardBuilders();
			AudioManager.InitializeAudio(_papa.AssetLookupSystem);
			_papa.InitializeNpe();
			_papa.InitializeMatchMaking();
			_papa.SetupDeckFormatFactory();
			TreePreloaderFactory.Create().PreloadTreesAsync(_papa.AssetTreeLoader);
			Pantry.Get<ClientFeatureToggleDataProvider>().InjectFrontDoor(Pantry.Get<IFrontDoorConnectionServiceWrapper>());
		}
		_cardDatabase = _papa.CardDatabase;
		_cardViewBuilder = _papa.CardViewBuilder;
		_cardMaterialBuilder = _papa.CardMaterialBuilder;
		_accountClient = Pantry.Get<IAccountClient>();
		_matchManager = _papa.MatchManager;
		_assetLookupSystem = _papa.AssetLookupSystem;
		_papa.FormatManager.RefreshFormats();
		_screenName.text = "Screen Name: " + _accountClient.AccountInformation.DisplayName;
		EnsureDebugDirectories();
		_deckConfigProvider = new DeckConfigProvider(_cardDatabase, _deckDirectory);
		_matchConfigProvider = new MatchConfigProvider(_matchConfigDirectory, CreateDefaultMatchConfig());
		_matchConfig = _matchConfigProvider.GetMatchConfigByName(MDNPlayerPrefs.DEBUG_MatchConfigSubDirectory, MDNPlayerPrefs.DEBUG_MatchConfigName);
		_emblemDataProvider = new EmblemDataProvider(_cardDatabase);
		_battlefieldDataProvider = new BattlefieldDataProvider(_assetLookupSystem);
		_avatarDataProvider = new AvatarDataProvider(_assetLookupSystem.TreeLoader);
		_sleeveDataProvider = new SleeveDataProvider(_assetLookupSystem.TreeLoader);
		_cardStyleDataProvider = new CardStyleDataProvider(_cardDatabase.CardDataProvider, _cardDatabase.GreLocProvider);
		_petDataProvider = new PetDataProvider(_assetLookupSystem.TreeLoader);
		_titlesDataProvider = new TitlesDataProvider(_papa.CosmeticsProvider);
		_familiarConfigValidator = MatchConfigValidator.CreateFamiliarValidator(_battlefieldDataProvider);
		_createMatchValidator = MatchConfigValidator.CreatePlayerVsPlayerValidator(_battlefieldDataProvider);
		BattlefieldUtil.SetBattlefieldById(_assetLookupSystem, _matchConfig.BattlefieldSelection);
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		HostPlatform platform = currentEnvironment?.HostPlatform ?? HostPlatform.Unknown;
		_servers = DebugServers(Pantry.Get<EnvironmentManager>(), platform);
		if (_servers.Length != 0)
		{
			for (int num = 0; num < _servers.Length; num++)
			{
				_serverAddressSelectDropdown.options.Add(new Dropdown.OptionData(_servers[num].Name));
			}
			int num2 = InitialServerIdx(_servers, currentEnvironment?.fdHost);
			_serverAddressSelectDropdown.value = num2;
			SetSelectedServer(_servers[num2]);
			_serverAddressSelectDropdown.onValueChanged.AddListener(OnServerAddressDropdownChanged);
		}
		else
		{
			SetSelectedServer(default(Server));
		}
		RoomClipboard.Init(_matchManager);
		_mainPanel.interactable = true;
		_loadingAssetsRoot.SetActive(value: false);
		GetComponent<CanvasGroup>().alpha = 1f;
		UpdateMatchButtons(_matchConfig);
	}

	private IEnumerator PrepareAndDownloadAssets()
	{
		AssetLoader.Initialize(_papa.BILogger, Pantry.Get<ResourceErrorMessageManager>());
		AssetBundleProvisioner bundleProvisioner = Pantry.Get<AssetBundleProvisioner>();
		yield return bundleProvisioner.PrepareDownload().AsCoroutine();
		if (bundleProvisioner.CheckHasMissingBundlesInQueue(AssetPriority.General))
		{
			SimpleAssetBundleProvisionProgressReporter reporter = _loadingAssetsRoot.GetComponentInChildren<SimpleAssetBundleProvisionProgressReporter>(includeInactive: true);
			reporter.gameObject.SetActive(value: true);
			bundleProvisioner.AssetPriorityLimit = AssetPriority.General;
			Debug.Log($"Downloading {bundleProvisioner.BundleCountRequired(AssetPriority.General)} assets");
			Task<AssetBundleDownloadResult> downloadTask = bundleProvisioner.DoDownload(reporter);
			yield return downloadTask.AsCoroutine();
			reporter.gameObject.SetActive(value: false);
			Debug.Log($"Downloaded {downloadTask.Result.TotalCompleted} assets");
		}
		else
		{
			Debug.Log("No download required.");
		}
		AssetLoader.PrepareAssets(bundleProvisioner);
	}

	private void EnsureDebugDirectories()
	{
		if (!Directory.Exists(_debugConfigDirectory))
		{
			Directory.CreateDirectory(_debugConfigDirectory);
		}
		if (!Directory.Exists(_matchConfigDirectory))
		{
			Directory.CreateDirectory(_matchConfigDirectory);
			MDNPlayerPrefs.DEBUG_MatchConfigName = string.Empty;
		}
		if (!Directory.Exists(_deckDirectory))
		{
			Directory.CreateDirectory(_deckDirectory);
		}
		if (!Directory.Exists(Path.Combine(_matchConfigDirectory, MDNPlayerPrefs.DEBUG_MatchConfigSubDirectory)))
		{
			MDNPlayerPrefs.DEBUG_MatchConfigSubDirectory = string.Empty;
		}
	}

	private Server[] DebugServers(EnvironmentManager environmentManager, HostPlatform platform)
	{
		if (platform == HostPlatform.AWS)
		{
			List<string> debugEnvironmentNames = environmentManager.GetDebugEnvironmentNames();
			List<Server> list = new List<Server>(debugEnvironmentNames.Count);
			for (int i = 0; i < debugEnvironmentNames.Count; i++)
			{
				EnvironmentDescription environmentDescription = environmentManager.FindEnvironment(debugEnvironmentNames[i]);
				list.Add(new Server(environmentDescription.name, environmentDescription.mdHost, environmentDescription.mdPort));
			}
			return list.ToArray();
		}
		return new Server[2]
		{
			new Server("Localhost SF cluster", "localhost", 9403),
			new Server("Localhost standalone MC", "localhost", 8888)
		};
	}

	private int InitialServerIdx(Server[] servers, string frontDoorUri)
	{
		int result = 0;
		string dEBUG_MatchDoorHost = MDNPlayerPrefs.DEBUG_MatchDoorHost;
		int dEBUG_MatchDoorPort = MDNPlayerPrefs.DEBUG_MatchDoorPort;
		string value = ((!string.IsNullOrWhiteSpace(frontDoorUri)) ? frontDoorUri.Replace("frontdoor-", string.Empty) : null);
		for (int i = 0; i < servers.Length; i++)
		{
			if (!string.IsNullOrWhiteSpace(value) && servers[i].MatchDoorHost.EndsWith(value, StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
			if (servers[i].MatchDoorHost == dEBUG_MatchDoorHost && servers[i].MatchDoorPort == dEBUG_MatchDoorPort)
			{
				result = i;
			}
		}
		return result;
	}

	private void OnDestroy()
	{
		_familiarButton.onClick.RemoveListener(OnPlayAgainstFamiliarClicked);
		_createButton.onClick.RemoveListener(OnCreateRoomClicked);
		_joinButton.onClick.RemoveListener(OnJoinRoomClicked);
		_roomUriOverwrite.onValueChanged.RemoveListener(UpdateJoinRoomButton);
		_replayButton.onClick.RemoveAllListeners();
		_serverAddressSelectDropdown.onValueChanged.RemoveAllListeners();
		if (_debugConfigEditor != null)
		{
			_debugConfigEditor.ViewModelChanged -= DebugConfigUpdated;
			_debugConfigEditor.Closed -= CloseDebugConfigEditor;
		}
	}

	private ConnectionConfig LookupGreConnectionConfig()
	{
		return new ConnectionConfig(_selectedServer.MatchDoorHost, _selectedServer.MatchDoorPort, _accountClient.AccountInformation?.PersonaID, _accountClient.AccountInformation?.Credentials.Jwt, Global.VersionInfo.ContentVersion.ToString(), MDNPlayerPrefs.InactivityTimeoutMs);
	}

	private void OnCreateRoomClicked()
	{
		MatchManager matchManager = InitializeDebugMatchManager(1u);
		MatchSceneManager.Load(_papa, "DuelSceneDebugLauncher", _cardDatabase, _cardViewBuilder, _cardMaterialBuilder, connectToMatch);
		void connectToMatch()
		{
			matchManager.OnConnectedToService += createMatch;
			matchManager.ConnectToMatchService(LookupGreConnectionConfig());
		}
		void createMatch()
		{
			_matchManager.OnConnectedToService -= createMatch;
			_matchManager.CreateMatch(_matchConfig);
		}
	}

	private void OnJoinRoomClicked()
	{
		(string, string) tuple = JoinMatchCredentials();
		string uri = tuple.Item1;
		string matchId = tuple.Item2;
		MatchManager matchManager = InitializeDebugMatchManager(2u);
		MatchSceneManager.Load(_papa, "DuelSceneDebugLauncher", _cardDatabase, _cardViewBuilder, _cardMaterialBuilder, connectToMatch);
		void connectToMatch()
		{
			matchManager.OnConnectedToService += joinMatch;
			_matchManager.ConnectToMatchService(LookupGreConnectionConfig());
		}
		void joinMatch()
		{
			matchManager.OnConnectedToService -= joinMatch;
			matchManager.JoinMatch(uri, matchId);
		}
	}

	private (string, string) JoinMatchCredentials()
	{
		if (_roomUriOverwrite == null)
		{
			return (string.Empty, string.Empty);
		}
		string text = _roomUriOverwrite.text;
		string value = Regex.Match(text, "/([0-9a-z-]+)$").Groups[1].Value;
		return (text, value);
	}

	private static void SetPlayerInfo(MatchManager matchManager, uint mySeatId, GreClient.Network.MatchConfig matchConfig)
	{
		matchManager.Players.Clear();
		uint num = 0u;
		uint num2 = 0u;
		foreach (GreClient.Network.TeamConfig team in matchConfig.Teams)
		{
			num++;
			foreach (GreClient.Network.PlayerConfig player in team.Players)
			{
				num2++;
				MatchManager.PlayerInfo playerInfo = new MatchManager.PlayerInfo(player.Name);
				playerInfo.SeatId = num2;
				playerInfo.TeamId = num;
				playerInfo.CommanderGrpIds = player.Deck.Commander;
				playerInfo.AvatarSelection = player.Avatar;
				playerInfo.SleeveSelection = player.Sleeve;
				playerInfo.TitleSelection = player.Title;
				playerInfo.PetSelection = new ClientPetSelection
				{
					name = player.Pet.petId,
					variant = player.Pet.variantId
				};
				RankingClassType rankingClass = (Enum.IsDefined(typeof(RankingClassType), player.Rank.Class) ? ((RankingClassType)player.Rank.Class) : RankingClassType.None);
				playerInfo.RankingClass = rankingClass;
				playerInfo.RankingTier = player.Rank.Tier;
				playerInfo.MythicPlacement = player.Rank.MythicPlacement;
				playerInfo.MythicPercentile = player.Rank.MythicPercent;
				matchManager.Players.Add(playerInfo);
			}
		}
		copyData(matchManager.LocalPlayerInfo, matchManager.Players.Find(mySeatId, (MatchManager.PlayerInfo x, uint mSeatId) => x.SeatId == mSeatId));
		copyData(matchManager.OpponentInfo, matchManager.Players.Find(mySeatId, (MatchManager.PlayerInfo x, uint mSeatId) => x.SeatId != mSeatId));
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
			}
		}
	}

	private void OnPlayAgainstFamiliarClicked()
	{
		uint num = 0u;
		List<(uint seatId, GreClient.Network.PlayerConfig config)> familiars = new List<(uint, GreClient.Network.PlayerConfig)>();
		foreach (GreClient.Network.PlayerConfig item in _matchConfig.Teams.SelectMany((GreClient.Network.TeamConfig team) => team.Players))
		{
			num++;
			if (item.PlayerType == PlayerType.Bot)
			{
				familiars.Add((num, item));
			}
		}
		ConnectionConfig connectionConfig = LookupGreConnectionConfig();
		MatchManager matchManager = InitializeDebugMatchManager(_matchConfig.CreatorSeatId());
		matchManager.ServerExceptionReceived += handleConnectToMatchError;
		MatchSceneManager.Load(_papa, "DuelSceneDebugLauncher", _cardDatabase, _cardViewBuilder, _cardMaterialBuilder, connectToMatch);
		void connectToMatch()
		{
			matchManager.OnConnectedToService += createMatch;
			matchManager.OnRoomCreated += joinMatch;
			matchManager.ConnectToMatchService(connectionConfig);
		}
		void createMatch()
		{
			matchManager.OnConnectedToService -= createMatch;
			matchManager.CreateMatch(_matchConfig);
		}
		void handleConnectToMatchError(string context, string errorCode, string message)
		{
			SceneManager.GetSceneByName("MatchScene").GetSceneComponent<MatchSceneManager>().ExitMatchScene();
			string text = ((errorCode == MatchServiceErrorCode.NotAuthorized.ToString()) ? "This player account is probably missing the MTGA_DEBUG role - contact Production/QA to add it to this account." : (context + " - " + errorCode + " - " + message));
			SystemMessageManager.Instance.ShowOk("Failed to Create Match", text);
			matchManager.ServerExceptionReceived -= handleConnectToMatchError;
		}
		void joinMatch(string uri, string matchId)
		{
			matchManager.OnRoomCreated -= joinMatch;
			matchManager.ServerExceptionReceived -= handleConnectToMatchError;
			matchManager.IsPracticeGame = familiars.Exists(((uint seatId, GreClient.Network.PlayerConfig config) x) => x.config.Avatar == "Avatar_Basic_Sparky");
			Wizards.Arena.Client.Logging.ILogger logger = new ConsoleLogger();
			foreach (var item2 in familiars)
			{
				TcpConnection tcpConnection = new TcpConnection(logger, 17, ServicePointManager.ServerCertificateValidationCallback);
				HeadlessClient headlessClient = new HeadlessClient(new GREConnection(Translate.ToConnectMessage(_cardDatabase.VersionProvider.DataVersion, item2.seatId), new LoggingConfig(), new MatchTcpConnection(tcpConnection), logger, ClientType.Familiar, RecordHistoryUtils.ShouldRecordHistory), FamiliarTypeToStrategy(item2.config.FamiliarStrategy), _cardDatabase);
				headlessClient.ConnectAndJoinMatch(connectionConfig.CreateFamiliar(item2.seatId), uri, matchId);
				matchManager.RegisterFamiliarClient(headlessClient);
			}
		}
	}

	private MatchManager InitializeDebugMatchManager(uint seatId)
	{
		Wizards.Arena.Client.Logging.ILogger logger = new ConsoleLogger();
		TcpConnection tcpConnection = new TcpConnection(logger, 17, ServicePointManager.ServerCertificateValidationCallback);
		IGREConnection connection = new GREConnection(Translate.ToConnectMessage(_cardDatabase.VersionProvider.DataVersion, seatId), new LoggingConfig
		{
			VerboseLogs = true
		}, new MatchTcpConnection(tcpConnection), logger, ClientType.User, RecordHistoryUtils.ShouldRecordHistory);
		_matchManager.Initialize(connection, _cardDatabase);
		SetPlayerInfo(_matchManager, seatId, _matchConfig);
		_papa.TimedReplayRecorder.StartMatch(_matchManager);
		return _matchManager;
	}

	private IHeadlessClientStrategy FamiliarTypeToStrategy(FamiliarStrategyType familiarStrategyType)
	{
		return familiarStrategyType switch
		{
			FamiliarStrategyType.Generic => new GenericStrategy(_cardDatabase), 
			FamiliarStrategyType.Random => new RequestHandlerStrategy(RandomRequestHandlers.Create(_cardDatabase)), 
			FamiliarStrategyType.Sparky => CreateSparky(_cardDatabase, _assetLookupSystem), 
			FamiliarStrategyType.NPE_Game1 => new RequestHandlerStrategy(new RequestHandlerFactory_NPE_Game1(RandomRequestHandlers.Create(_cardDatabase))), 
			FamiliarStrategyType.NPE_Game2 => new RequestHandlerStrategy(new RequestHandlerFactory_NPE_Game2(RandomRequestHandlers.Create(_cardDatabase), _papa.ObjectPool, 500f)), 
			_ => new GoldfishStrategy(100u), 
		};
	}

	private static IHeadlessClientStrategy CreateSparky(ICardDatabaseAdapter cardDatabase, AssetLookupSystem assetLookupSystem)
	{
		UnityFamiliar unityFamiliar = UnityEngine.Object.FindObjectOfType<UnityFamiliar>();
		if (unityFamiliar == null)
		{
			GameObject obj = new GameObject("UnityFamiliar");
			UnityEngine.Object.DontDestroyOnLoad(obj);
			unityFamiliar = obj.AddComponent<UnityFamiliar>();
		}
		BotControlConfigurationSO botControlConfigurationSO = BotControlManager.FetchBotConfig(assetLookupSystem);
		return new HeuristicStrategy(botControlConfigurationSO._minWaitTime, botControlConfigurationSO._maxWaitTime, botControlConfigurationSO._maxIdleTime, botControlConfigurationSO._deckHeuristics.SelectRandom(), new AttackConfig(botControlConfigurationSO._maxAttackConfigurationsToExplore, botControlConfigurationSO._maxAttackCalculationTime, botControlConfigurationSO._aiCreatureDensityFactor, botControlConfigurationSO._playerCreatureDensityFactor, botControlConfigurationSO._idleAttackTurnsDensityFactor), new BlockConfig(botControlConfigurationSO._maxBlockConfigurationsToExplore, botControlConfigurationSO._maxBlockCalculationTime), unityFamiliar, cardDatabase);
	}

	public void OnServerAddressDropdownChanged(int index)
	{
		Server selectedServer = ((_servers.Length >= index) ? _servers[index] : default(Server));
		SetSelectedServer(selectedServer);
	}

	private void SetSelectedServer(Server server)
	{
		if (server.Equals(default(Server)))
		{
			_selectedServer = default(Server);
			_currentServerText.text = "UNKOWN SERVER";
			return;
		}
		_selectedServer = server;
		_currentServerText.text = $"{_selectedServer.MatchDoorHost}:{_selectedServer.MatchDoorPort}";
		MDNPlayerPrefs.DEBUG_MatchDoorHost = _selectedServer.MatchDoorHost;
		MDNPlayerPrefs.DEBUG_MatchDoorPort = _selectedServer.MatchDoorPort;
		BuildInfoDebugDisplay buildInfoDebugDisplay = UnityEngine.Object.FindObjectOfType<BuildInfoDebugDisplay>();
		if (buildInfoDebugDisplay != null)
		{
			buildInfoDebugDisplay.SetMatchDoorHostText(MDNPlayerPrefs.DEBUG_MatchDoorHost);
		}
	}

	private void ShowConfigPanelOverlay()
	{
		_activeConfigPanelParent.gameObject.SetActive(value: true);
	}

	private void HideConfigPanelOverlay()
	{
		_activeConfigPanelParent.gameObject.UpdateActive(active: false);
	}

	private void OpenReplayLauncher()
	{
		ShowConfigPanelOverlay();
		if (_replayLauncher == null)
		{
			_replayLauncher = UnityEngine.Object.Instantiate(_prefabs.ReplayLauncher, _activeConfigPanelParent.transform);
		}
		else
		{
			_replayLauncher.gameObject.UpdateActive(active: true);
		}
		_replayLauncher.CloseClicked += onCloseClicked;
		_replayLauncher.StartReplayClicked += onStartReplayClicked;
		void onCloseClicked()
		{
			_replayLauncher.CloseClicked -= onCloseClicked;
			_replayLauncher.StartReplayClicked -= onStartReplayClicked;
			CloseReplayLauncher();
		}
		void onStartReplayClicked(ReplayInfo replayInfo)
		{
			_replayLauncher.CloseClicked -= onCloseClicked;
			_replayLauncher.StartReplayClicked -= onStartReplayClicked;
			CloseReplayLauncher();
			ReplayUtilities.StartReplay(replayInfo, _papa, _matchManager, _cardDatabase, _cardViewBuilder, _cardMaterialBuilder, null, "DuelSceneDebugLauncher");
		}
	}

	private void CloseReplayLauncher()
	{
		HideConfigPanelOverlay();
		if (_replayLauncher != null)
		{
			_replayLauncher.gameObject.UpdateActive(active: false);
		}
	}

	private void OpenDebugConfigEditor()
	{
		ShowConfigPanelOverlay();
		if (_debugConfigEditor == null)
		{
			_debugConfigEditor = UnityEngine.Object.Instantiate(_prefabs.DebugConfigEditor, _activeConfigPanelParent.transform);
			_debugConfigEditor.ViewModelChanged += DebugConfigUpdated;
			_debugConfigEditor.Closed += CloseDebugConfigEditor;
			_debugConfigEditor.DecklistEditor.Initialize(new DebugDecklistEditorPanel.ContextProviders(_deckConfigProvider as DeckConfigProvider, _cardDatabase, _cardDatabase.CardDataProvider, _cardDatabase.CardTitleProvider));
		}
		else
		{
			_debugConfigEditor.gameObject.UpdateActive(active: true);
		}
		UpdateDebugConfigEditor(_matchConfig);
	}

	private void CloseDebugConfigEditor()
	{
		HideConfigPanelOverlay();
		if (_debugConfigEditor != null)
		{
			_debugConfigEditor.gameObject.UpdateActive(active: false);
		}
	}

	private void RefreshDeckConfigProvider()
	{
		DeckConfigProvider deckConfigProvider = (DeckConfigProvider)(_deckConfigProvider = new DeckConfigProvider(_cardDatabase, _deckDirectory));
		_debugConfigEditor.DecklistEditor.UpdateDeckConfigProvider(deckConfigProvider);
	}

	private void DebugConfigUpdated(DebugConfigEditor.ViewModel old, DebugConfigEditor.ViewModel updated)
	{
		if (updated.ConfigSelector.CreateNewConfig)
		{
			MDNPlayerPrefs.DEBUG_MatchConfigName = getNewConfigName("New Config", 0, _matchConfigProvider.GetAllMatchConfigs(MDNPlayerPrefs.DEBUG_MatchConfigSubDirectory));
			GreClient.Network.MatchConfig baseConfig = CreateDefaultMatchConfig();
			IMatchConfigProvider matchConfigProvider = _matchConfigProvider;
			string dEBUG_MatchConfigSubDirectory = MDNPlayerPrefs.DEBUG_MatchConfigSubDirectory;
			string dEBUG_MatchConfigName = MDNPlayerPrefs.DEBUG_MatchConfigName;
			string dEBUG_MatchConfigName2 = MDNPlayerPrefs.DEBUG_MatchConfigName;
			matchConfigProvider.SaveMatchConfig(dEBUG_MatchConfigSubDirectory, dEBUG_MatchConfigName, new GreClient.Network.MatchConfig(baseConfig, null, dEBUG_MatchConfigName2));
			_matchConfig = _matchConfigProvider.GetMatchConfigByName(MDNPlayerPrefs.DEBUG_MatchConfigSubDirectory, MDNPlayerPrefs.DEBUG_MatchConfigName);
		}
		else if (updated.ConfigSelector.RefreshDirectory)
		{
			EnsureDebugDirectories();
			RefreshDeckConfigProvider();
			_matchConfigProvider = new MatchConfigProvider(_matchConfigDirectory, CreateDefaultMatchConfig());
			_matchConfig = _matchConfigProvider.GetMatchConfigByName(MDNPlayerPrefs.DEBUG_MatchConfigSubDirectory, MDNPlayerPrefs.DEBUG_MatchConfigName);
		}
		else if (old.SubDirectory != updated.SubDirectory)
		{
			_matchConfig = _matchConfigProvider.GetAllMatchConfigs(updated.SubDirectory).First();
			MDNPlayerPrefs.DEBUG_MatchConfigSubDirectory = updated.SubDirectory;
		}
		else if (!old.SelectedConfig.Equals(updated.SelectedConfig))
		{
			_matchConfig = updated.SelectedConfig;
			MDNPlayerPrefs.DEBUG_MatchConfigName = updated.SelectedConfig.Name;
		}
		else if (!old.MatchConfig.Equals(updated.MatchConfig))
		{
			_matchConfig = updated.MatchConfig.ConvertFromViewModel();
			if (old.MatchConfig.Name != updated.MatchConfig.Name)
			{
				_matchConfigProvider.RenameMatchConfig(MDNPlayerPrefs.DEBUG_MatchConfigSubDirectory, old.MatchConfig.Name, updated.MatchConfig.Name, _matchConfig);
				MDNPlayerPrefs.DEBUG_MatchConfigName = updated.MatchConfig.Name;
			}
			else
			{
				_matchConfigProvider.SaveMatchConfig(MDNPlayerPrefs.DEBUG_MatchConfigSubDirectory, _matchConfig.Name, _matchConfig);
			}
			if (old.SelectedBattlefield != updated.SelectedBattlefield)
			{
				BattlefieldUtil.SetBattlefieldById(_assetLookupSystem, updated.SelectedBattlefield);
			}
		}
		UpdateMatchButtons(_matchConfig);
		UpdateDebugConfigEditor(_matchConfig);
		static string getNewConfigName(string name, int identifier, IReadOnlyList<GreClient.Network.MatchConfig> allConfigs)
		{
			if (allConfigs.Exists((GreClient.Network.MatchConfig x) => x.Name == name))
			{
				identifier++;
				return getNewConfigName($"New Config ({identifier})", identifier, allConfigs);
			}
			return name;
		}
	}

	private void UpdateMatchButtons(GreClient.Network.MatchConfig matchConfig)
	{
		bool canConnect = _accountClient.AccountInformation != null && !_selectedServer.Equals(default(Server));
		updateMatchButton(canConnect, matchConfig, _familiarConfigValidator, _familiarButton, _familiarMatchValidationOutput);
		updateMatchButton(canConnect, matchConfig, _createMatchValidator, _createButton, _createMatchValidationOutput);
		UpdateJoinRoomButton(_roomUriOverwrite.text);
		static void updateMatchButton(bool flag, GreClient.Network.MatchConfig matchConfig2, IMatchConfigValidator validator, Button button, ValidationOutput validationOutput)
		{
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			foreach (var result in validator.GetResults(matchConfig2))
			{
				if (result.resultType == IMatchConfigValidator.Result.Error)
				{
					list.Add(result.reason);
				}
				else if (result.resultType == IMatchConfigValidator.Result.Warning)
				{
					list2.Add(result.reason);
				}
			}
			button.interactable = list.Count == 0 && flag;
			validationOutput.SetModel(list, list2);
		}
	}

	private void UpdateJoinRoomButton(string value)
	{
		bool flag = _accountClient.AccountInformation != null && !_selectedServer.Equals(default(Server));
		_joinButton.interactable = !string.IsNullOrEmpty(value) && flag;
	}

	private void UpdateDebugConfigEditor(GreClient.Network.MatchConfig matchConfig)
	{
		if (_debugConfigEditor == null)
		{
			return;
		}
		Dictionary<GreClient.Network.DeckConfig, IReadOnlyList<CardStyle>> dictionary = new Dictionary<GreClient.Network.DeckConfig, IReadOnlyList<CardStyle>>();
		foreach (GreClient.Network.DeckConfig item in from y in matchConfig.Teams.SelectMany((GreClient.Network.TeamConfig x) => x.Players)
			select y.Deck)
		{
			dictionary[item] = _cardStyleDataProvider.GetCardStylesForDeck(item);
		}
		_debugConfigEditor.SetModel(new DebugConfigEditor.ViewModel(new MatchConfigSelectionEditor.ViewModel(_debugConfigDirectory, MDNPlayerPrefs.DEBUG_MatchConfigSubDirectory, _matchConfigProvider.GetMatchConfigByName(MDNPlayerPrefs.DEBUG_MatchConfigSubDirectory, MDNPlayerPrefs.DEBUG_MatchConfigName), _matchConfigProvider.GetAllDirectories(), _matchConfigProvider.GetAllMatchConfigs(MDNPlayerPrefs.DEBUG_MatchConfigSubDirectory)), matchConfig.ConvertToViewModel(MyPlayerId, _deckConfigProvider.GetAllDecks(), _battlefieldDataProvider.GetAllBattlefields(), _emblemDataProvider.GetAllEmblems(), _avatarDataProvider.GetAllAvatars(), _sleeveDataProvider.GetAllSleeves(), _petDataProvider.GetAllPetData(), dictionary, _titlesDataProvider.GetAllTitles())));
	}

	private GreClient.Network.MatchConfig CreateDefaultMatchConfig()
	{
		return CreateDefaultMatchConfig(_deckConfigProvider.GetDefaultDeck(), _battlefieldDataProvider.GetDefaultBattlefield());
	}

	private GreClient.Network.MatchConfig CreateDefaultMatchConfig(GreClient.Network.DeckConfig defaultDeck, string defaultBattlefield)
	{
		return GreClient.Network.MatchConfig.Default(new List<GreClient.Network.TeamConfig>
		{
			new GreClient.Network.TeamConfig(new List<GreClient.Network.PlayerConfig> { GreClient.Network.PlayerConfig.Default(PlayerType.You, MyPlayerId, defaultDeck) }),
			new GreClient.Network.TeamConfig(new List<GreClient.Network.PlayerConfig> { GreClient.Network.PlayerConfig.Default(PlayerType.Bot, MyPlayerId, defaultDeck) })
		}, defaultBattlefield);
	}
}
