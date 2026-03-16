#define UNITY_STANDALONE
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.AssetLookupTree.AssetLookup;
using Core.Code.ClientFeatureToggle;
using Core.Code.Input;
using Core.Code.Promises;
using Core.MainNavigation.RewardTrack;
using Core.MatchScene;
using Core.Meta.MainNavigation;
using Core.Meta.MainNavigation.PopUps;
using Core.Meta.MainNavigation.SystemMessage;
using Core.NPEStitcher;
using Core.Shared.Code;
using Core.Shared.Code.Connection;
using Core.Shared.Code.DebugTools;
using Core.Shared.Code.Providers;
using Core.Shared.Code.ServiceFactories;
using Core.Shared.Code.Utilities;
using Cysharp.Threading.Tasks;
using GreClient.CardData;
using MTGA.KeyboardManager;
using MTGA.Social;
using Pooling;
using SharedClientCore.SharedClientCore.Code.Providers;
using UnityEngine;
using UnityEngine.CrashReportHandler;
using UnityEngine.SceneManagement;
using Wizards.Arena.Promises;
using Wizards.Arena.TcpConnection;
using Wizards.MDN;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wizards.Mtga.Assets;
using Wizards.Mtga.Logging;
using Wizards.Mtga.Platforms;
using Wizards.Unification.Models.Event;
using Wotc.Mtga.AutoPlay;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;
using Wotc.Mtga.Quality;
using Wotc.Mtga.TimedReplays;
using _3rdParty.Discord;
using _3rdParty.ExternalChat;

public class PAPA : MonoBehaviour
{
	public enum MdnScene
	{
		None,
		Login,
		Preparing,
		Wrapper,
		Match,
		NpeStitcher
	}

	public static class SceneLoading
	{
		public static Action<object> OnWrapperSceneLoaded = delegate
		{
		};

		public static Action<LoadSceneMode> OnDuelSceneLoaded = delegate
		{
		};

		public static MdnScene CurrentScene
		{
			get
			{
				if (!(_instance != null))
				{
					return MdnScene.None;
				}
				return _instance._currentScene;
			}
		}

		public static void LoadNPEScene(INpeStrategy npeDataProvider = null)
		{
			_instance._currentScene = MdnScene.NpeStitcher;
			_instance.StartCoroutine(NPEStitcherScene.Coroutine_Load(_instance.AccountClient.AccountInformation, _instance.NpeState, _instance.BILogger, _instance.KeyBoardManager, _instance.Actions, _instance.CardDatabase, _instance.CardViewBuilder, _instance.AssetLookupSystem, npeDataProvider, _instance._fdConnectionManager, _instance.SettingsMenuHost));
		}

		public static UniTask LoadWrapperScene(object panelContext = null)
		{
			UniTaskCompletionSource uniTaskCompletionSource = new UniTaskCompletionSource();
			Debug.Log($"{DateTime.Now} LoadWrapperScene");
			Scene sceneByName = SceneManager.GetSceneByName("MainNavigation");
			Coroutine coroutine = null;
			coroutine = (sceneByName.isLoaded ? _instance.StartCoroutine(ReloadWrapperRoutine(panelContext, uniTaskCompletionSource)) : _instance.StartCoroutine(LoadWrapperRoutine(panelContext, uniTaskCompletionSource)));
			if (CurrentScene == MdnScene.Match)
			{
				_instance.StartCoroutine(WaitForWrapperLoadThenUnloadMatch(uniTaskCompletionSource.Task, coroutine));
			}
			_instance._currentScene = MdnScene.Wrapper;
			return uniTaskCompletionSource.Task;
		}

		private static IEnumerator WaitForWrapperLoadThenUnloadMatch(UniTask wrapperTask, Coroutine wrapperCoroutine)
		{
			yield return wrapperTask.ToCoroutine();
			SceneManager.UnloadSceneAsync("MatchScene");
		}

		private static IEnumerator LoadWrapperRoutine(object panelContext, UniTaskCompletionSource tcs)
		{
			ClearCaches();
			yield return WrapperController.Coroutine_Load(_instance, _instance.CardMaterialBuilder.TextureLoader, _instance.CardMaterialBuilder.CropDatabase, _instance._crossThreadLogger, _instance._motdSession, _instance._emoteDataProvider, _instance.BILogger, panelContext, _instance.CosmeticsProvider);
			InitSocialManager();
			if (Pantry.Get<ClientFeatureToggleDataProvider>().GetToggleValueById("SocialV2Status"))
			{
				SceneLoader.GetSceneLoader().InitSocial();
			}
			else
			{
				CreateSocialUI();
			}
			OnWrapperSceneLoaded(panelContext);
			tcs.TrySetResult();
		}

		private static IEnumerator ReloadWrapperRoutine(object panelContext, UniTaskCompletionSource tcs)
		{
			yield return WrapperController.Coroutine_Reload(panelContext);
			if (Pantry.Get<ClientFeatureToggleDataProvider>().GetToggleValueById("SocialV2Status"))
			{
				SceneLoader.GetSceneLoader().InitSocial();
			}
			else
			{
				CreateSocialUI();
			}
			OnWrapperSceneLoaded(panelContext);
			tcs.TrySetResult();
		}

		public static void LoadDuelScene(LoadSceneMode loadMode)
		{
			_instance.StartCoroutine(Coroutine_LoadDuelScene());
			IEnumerator Coroutine_LoadDuelScene()
			{
				bool unloadMainNav = false;
				if (CurrentScene == MdnScene.Wrapper)
				{
					yield return WrapperController.Instance.Coroutine_TransitionOut();
					unloadMainNav = true;
				}
				_instance._currentScene = MdnScene.Match;
				UniTask uniTask = new LoadMatchSceneUniTask(new MatchSceneManager.MatchSceneInitData(loadMode, _instance, _instance._fdConnectionManager, _instance.CardDatabase, _instance.CardViewBuilder, _instance.CardMaterialBuilder)).Load(_instance.destroyCancellationToken);
				yield return uniTask;
				if (unloadMainNav && SceneManager.GetSceneByName("MainNavigation").isLoaded)
				{
					yield return UnloadWrapperAndMainNav();
					yield return ClearPapaCache();
				}
				InitSocialManager();
				CreateSocialUI();
				OnDuelSceneLoaded(loadMode);
			}
			static IEnumerator UnloadWrapperAndMainNav()
			{
				yield return Pantry.Get<WrapperSceneManagement>().UnloadWrapper(new WrapperSceneLifeCycle[1] { WrapperSceneLifeCycle.LoggedIn });
				yield return SceneManager.UnloadSceneAsync("MainNavigation");
			}
		}

		private static void CreateSocialUI()
		{
			if (_instance._socialUI != null && _instance._socialUI.MissingAssetBundleRefrences)
			{
				UnityEngine.Object.Destroy(_instance._socialUI.gameObject);
				_instance._socialUI = null;
			}
			if (_instance._socialUI == null)
			{
				string prefabPath = _instance.AssetLookupSystem.GetPrefabPath<SocialUIPrefab, SocialUI>();
				_instance._socialUI = AssetLoader.Instantiate<SocialUI>(prefabPath);
				_instance._socialUI.Init(_instance._socialManager, _instance.KeyBoardManager, _instance.AssetLookupSystem, _instance.Actions);
			}
			SceneLoader.GetSceneLoader()?.SetSocialClient(_instance._socialManager, _instance._socialUI);
		}
	}

	private static PAPA _instance;

	public static readonly Guid ClientSessionId = Guid.NewGuid();

	private FrontDoorConnectionManager _fdConnectionManager;

	private ConnectionManager _connectionManager;

	private GlobalCoroutineExecutor _coroutineExecutor;

	private PopupManager _popupManager;

	private PerformanceCSVLogger _dssLogger;

	private AssetLoadCountLogger _assetLoadCountLogger;

	private UnityCrossThreadLogger _crossThreadLogger;

	private FormatManager _formatManager;

	private MemoryManager _memoryManager;

	private IMatchdoorServiceWrapper _matchdoorServiceWrapper;

	private IEmoteDataProvider _emoteDataProvider;

	private ISocialManager _socialManager;

	private SocialUI _socialUI;

	private MOTDSession _motdSession;

	private AutoPlayManager _autoPlayManager;

	private ISystemMessageManager _systemMessageManager;

	private CardDatabaseLoader _cardDatabaseLoader;

	private bool _shuttingDown;

	private bool _applicationQuitting;

	private MdnScene _currentScene;

	public FormatManager FormatManager => _formatManager;

	public CardViewBuilder CardViewBuilder { get; private set; }

	public IObjectPool ObjectPool { get; private set; }

	public IUnityObjectPool UnityPool { get; private set; }

	public CardDatabase CardDatabase { get; private set; }

	public SettingsMenuHost SettingsMenuHost { get; private set; }

	public CardMaterialBuilder CardMaterialBuilder { get; private set; }

	public KeyboardManager KeyBoardManager { get; private set; }

	public AssetLookupSystem AssetLookupSystem { get; private set; }

	public AssetLookupTreeLoader AssetTreeLoader => AssetLookupSystem.TreeLoader;

	public IPreconDeckServiceWrapper PreconDeckManager { get; private set; }

	public IBILogger BILogger { get; private set; }

	public IFrontDoorConnectionServiceWrapper FrontDoorConnection { get; private set; }

	public Matchmaking Matchmaking { get; private set; }

	public IAccountClient AccountClient { get; private set; }

	public InventoryManager InventoryManager { get; private set; }

	public CosmeticsProvider CosmeticsProvider { get; private set; }

	public SetMasteryDataProvider MasteryPassProvider { get; private set; }

	public IEmoteEntryLoader EmoteEntryLoader { get; private set; }

	public EventManager EventManager { get; private set; }

	public NPEState NpeState { get; private set; }

	public MatchManager MatchManager { get; private set; }

	public IExternalChatManager ExternalChatManager { get; private set; }

	public TimedReplayRecorder TimedReplayRecorder { get; private set; }

	public IActionSystem Actions { get; private set; }

	public static event Action UpdateEvent;

	public static PAPA Create()
	{
		if (_instance != null)
		{
			Debug.LogError("!!! We already have a PAPA instance. Something has gone very wrong.");
			UnityEngine.Object.Destroy(_instance.gameObject);
		}
		GameObject obj = new GameObject("PAPA");
		UnityEngine.Object.DontDestroyOnLoad(obj);
		_instance = obj.AddComponent<PAPA>();
		obj.AddComponent<MainThreadDispatcher>();
		_instance.Initialize();
		return _instance;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		QualitySettingsHelpers.ForceAllowableResolution();
		QualitySettingsUtil.Instance.ApplySettings();
	}

	private void Initialize()
	{
		Actions = Pantry.Get<IActionSystem>();
		KeyBoardManager = Pantry.Get<KeyboardManager>();
		BILogger = Pantry.Get<IBILogger>();
		LoggingUtils.InjectBiLogger(BILogger);
		_popupManager = Pantry.Get<PopupManager>();
		_coroutineExecutor = Pantry.Get<GlobalCoroutineExecutor>();
		MDNPlayerPrefs.SetupVersion();
		if (QualityModeProvider.ForceMinSpec)
		{
			QualitySettingsUtil.Instance.GlobalQualityLevel = 0;
		}
		else if (PlayerPrefs.HasKey("Graphics.GlobalQualityLevel"))
		{
			QualitySettingsUtil.Instance.ApplySettings();
			QualitySettingsUtil.Instance.LoadCustomSettings();
		}
		else
		{
			QualitySettingsUtil.Instance.GlobalQualityLevel = PlatformContext.GetQualitySelector().GetDefaultQualityLevel();
		}
		SceneManager.sceneLoaded += OnSceneLoaded;
		SceneManager.sceneLoaded += Scenes.SceneLoaded;
		ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
		_crossThreadLogger = new UnityCrossThreadLogger();
		_crossThreadLogger.InjectBILogger(BILogger);
		LoggerManager.Register(_crossThreadLogger);
		MatchManager = Pantry.Get<MatchManager>();
		MatchManager.SetDebugFunc(HasDebugRole);
		Matchmaking = Pantry.Get<Matchmaking>();
		_connectionManager = Pantry.Get<ConnectionManager>();
		_fdConnectionManager = Pantry.Get<FrontDoorConnectionManager>();
		ExternalChatManager = new DiscordManager();
		_systemMessageManager = Pantry.Get<ISystemMessageManager>();
		_crossThreadLogger.InjectMatchManager(MatchManager);
		EnvironmentManager environmentManager = Pantry.Get<EnvironmentManager>();
		AssetBundleManager.Create(environmentManager.AssetsConfiguration);
		SetupAssetLookupSystem();
		UnityPool = Pantry.Get<IUnityObjectPool>();
		ObjectPool = Pantry.Get<IObjectPool>();
		if (PerformanceCSVLogger.ShouldRunDSS)
		{
			_dssLogger = new PerformanceCSVLogger(folderPath: Path.Combine(PlatformContext.GetStorageContext().LocalPersistedStoragePath, "DSSReports"), assetBundleManager: AssetBundleManager.Instance, matchManager: MatchManager, thermalStatusProvider: Pantry.Get<IThermalStatusProvider>(), cpuUsageProvider: Pantry.Get<ICPUUsageProvider>(), nameOverride: MDNPlayerPrefs.DSSReportName);
		}
		if (AssetLoadCountLogger.Enabled)
		{
			_assetLoadCountLogger = new AssetLoadCountLogger(AssetBundleManager.Instance);
		}
		CrashReportHandler.SetUserMetadata("clientSessionId", ClientSessionId.ToString());
		_emoteDataProvider = Pantry.Get<IEmoteDataProvider>();
		_fdConnectionManager.Initialize(Pantry.Get<IClientLocProvider>(), MatchManager, LoggingUtils.LoggingConfig, _crossThreadLogger);
		_fdConnectionManager.OnConnected += OnFrontDoorConnected;
		TimedReplayRecorder = new TimedReplayRecorder(Matchmaking);
		_memoryManager = new MemoryManager();
		OnEnvironmentSet();
		environmentManager.OnEnvironmentSet = (Action)Delegate.Combine(environmentManager.OnEnvironmentSet, new Action(OnEnvironmentSet));
		StartCoroutine(Coroutine_ValidateToken());
		DebugInfoIMGUI debugInfoIMGUI = Resources.Load<DebugInfoIMGUI>("DebugInfoIMGUI");
		if ((bool)debugInfoIMGUI)
		{
			UnityEngine.Object.Instantiate(debugInfoIMGUI, base.transform).Init(this, HasDebugRole);
		}
		base.gameObject.AddComponent<DebugLogGUI>().Init(HasDebugRole);
	}

	private void SetupAssetLookupSystem()
	{
		AssetLookupSystem = Pantry.Get<AssetLookupManager>().AssetLookupSystem;
	}

	private void OnEnvironmentSet()
	{
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		AccountClient = Pantry.Get<IAccountClient>();
		AccountClient.SetCredentials(currentEnvironment);
		_fdConnectionManager.SetEnvironment(currentEnvironment, AccountClient);
		createServiceWrappers();
		SettingsMenuHost = new SettingsMenuHost(LoggingUtils.LoggingConfig, AccountClient, _socialManager, KeyBoardManager, Actions, MatchManager, AssetLookupSystem, () => NpeState, Pantry.Get<ISetMetadataProvider>());
		_popupManager.SetMenuHost(SettingsMenuHost);
		PlatformContext.GetNotificationsContext().InitializePushNotifications(AccountClient);
	}

	private void createServiceWrappers()
	{
		FrontDoorConnection = Pantry.Get<IFrontDoorConnectionServiceWrapper>();
		_systemMessageManager.SetFDConnectionWrapper(FrontDoorConnection);
		PreconDeckManager = Pantry.Get<IPreconDeckServiceWrapper>();
		EventManager = Pantry.Get<EventManager>();
		InventoryManager = Pantry.Get<InventoryManager>();
		CosmeticsProvider = Pantry.Get<CosmeticsProvider>();
		MasteryPassProvider = Pantry.Get<SetMasteryDataProvider>();
		IInventoryServiceWrapper inventoryServiceWrapper = Pantry.Get<IInventoryServiceWrapper>();
		ConnectionManager connectionManager = _connectionManager;
		connectionManager.OnFdReconnected = (Action)Delegate.Combine(connectionManager.OnFdReconnected, new Action(inventoryServiceWrapper.OnReconnect));
		_motdSession = new MOTDSession();
		EmoteEntryLoader = new MercantileEmoteEntryLoader(Pantry.Get<IMercantileServiceWrapper>());
		EmoteEntryLoader.OnEmoteEntryLoaded += OnEmoteEntryLoaded;
	}

	private IEnumerator loadDesignerMetadata()
	{
		DesignerMetadataProvider designerMetadataProvider = Pantry.Get<DesignerMetadataProvider>();
		Promise<DTO_CardMetadataInfo> promise = designerMetadataProvider.Initialize();
		yield return promise.AsCoroutine();
		DTO_CardMetadataInfo designerMetadata = designerMetadataProvider.GetDesignerMetadata();
		if (designerMetadata != null)
		{
			CardUtilities.NonCraftableCardList = designerMetadata.NonCraftableCardList.Select((int x) => (uint)x).ToList();
			CardUtilities.NonCollectibleCardList = designerMetadata.NonCollectibleCardList.Select((int x) => (uint)x).ToList();
			if (designerMetadata.UnreleasedSets != null)
			{
				CardUtilities.UnreleasedSets = designerMetadata.UnreleasedSets;
			}
		}
	}

	public IEnumerator LoadCardDatabase()
	{
		yield return loadDesignerMetadata();
		yield return new LoadCardDatabaseUniTask().Load().ToCoroutine();
		CardDatabase = Pantry.Get<CardDatabase>();
		EventManager.Inject(CardDatabase);
	}

	public void CreateFormatManager()
	{
		if (_formatManager == null)
		{
			_formatManager = Pantry.Get<FormatManager>();
			_formatManager.Initialize(CardDatabase);
		}
	}

	public void SetupDeckFormatFactory()
	{
		if (CardDatabase == null)
		{
			SimpleLog.LogError("[PAPA] Incorrect initialization, make sure you have a card database before initializing deck formats.");
		}
		if (_formatManager == null)
		{
			SimpleLog.LogError("[PAPA] Incorrect initialization, make sure format manager exists before initializing deck formats.");
		}
	}

	public void OnEmoteEntryLoaded(IReadOnlyCollection<ClientEmoteEntry> emoteEntries)
	{
		_emoteDataProvider.Update(emoteEntries);
	}

	public void InitCardBuilders()
	{
		CardViewBuilder = Pantry.Get<CardViewBuilder>();
		CardMaterialBuilder = Pantry.Get<CardMaterialBuilder>();
	}

	public void InitAutoPlay()
	{
		MDNPlayerPrefs.SetUseVerboseLogs(newValue: true);
		LoggingUtils.LoggingConfig.VerboseLogs = true;
		if (!(UnityEngine.Object.FindObjectOfType<AutoPlayHolder>() != null))
		{
			_autoPlayManager = new AutoPlayManager(CardDatabase, CardViewBuilder, CardMaterialBuilder, AccountClient, ClientSessionId);
			AutoPlayHolder autoPlayHolder = new GameObject("AutoPlayHolder").AddComponent<AutoPlayHolder>();
			autoPlayHolder.SetManager(_autoPlayManager);
			autoPlayHolder.SetFontSize(MDNPlayerPrefs.DebugAutoplayHighlightLogFontSize);
		}
	}

	public void InitializeNpe()
	{
		NpeState = Pantry.Get<NPEState>();
	}

	public void SocialUISetVisible(bool visible)
	{
		if (_socialUI != null)
		{
			_socialUI.SetVisible(visible);
		}
	}

	public void InitializeMatchMaking()
	{
		_matchdoorServiceWrapper = Pantry.Get<IMatchdoorServiceWrapper>();
		Matchmaking.Initialize(ObjectPool, _matchdoorServiceWrapper, MatchManager, AccountClient, NpeState, CardDatabase, _connectionManager, LoggingUtils.LoggingConfig, _crossThreadLogger, AssetLookupSystem);
	}

	private void Update()
	{
		if (!_shuttingDown)
		{
			PAPA.UpdateEvent?.Invoke();
			LoggingUtils.LogToFile?.Update();
			CardMaterialBuilder?.UpdateDecayTimers(Time.deltaTime);
			BoosterPayloadUtilities.UpdateDecayTimers(Time.deltaTime);
			_autoPlayManager?.Update();
			KeyBoardManager.Update();
			MatchManager?.Update();
			_dssLogger?.Update(Time.unscaledDeltaTime);
			if (FrontDoorConnection != null)
			{
				FrontDoorConnection.ProcessMessages();
				_fdConnectionManager?.Update();
				Actions?.Update();
			}
		}
	}

	private bool HasDebugRole()
	{
		if (Debug.isDebugBuild)
		{
			return true;
		}
		return (AccountClient?.AccountInformation)?.HasRole_Debugging() ?? false;
	}

	public static void DebugUnloadAllScenes()
	{
		Scene scene = SceneManager.CreateScene("NewScene");
		for (int num = SceneManager.sceneCount - 1; num >= 0; num--)
		{
			Scene sceneAt = SceneManager.GetSceneAt(num);
			if (!(sceneAt.name == scene.name))
			{
				SceneManager.UnloadSceneAsync(sceneAt.name);
			}
		}
	}

	public static void DebugUnloadBundlesAndScenes()
	{
		AssetBundle.UnloadAllAssetBundles(unloadAllObjects: true);
		AssetBundleManager.Instance.UnloadAll();
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i).name);
		}
		SceneManager.CreateScene("NewScene");
	}

	public static void DebugClearCache()
	{
		ClearCaches();
		Resources.UnloadUnusedAssets().completed += delegate
		{
			GC.Collect();
			Debug.Log("Caches cleared - Debug");
		};
	}

	private IEnumerator Coroutine_ValidateToken()
	{
		while (true)
		{
			if (Pantry.CurrentEnvironment != EnvironmentDescription.NullEnvironment)
			{
				IFrontDoorConnectionServiceWrapper frontDoorConnectionServiceWrapper = Pantry.Get<IFrontDoorConnectionServiceWrapper>();
				if (frontDoorConnectionServiceWrapper.ConnectionState == FDConnectionState.Connected && AccountClient != null && NpeState != null)
				{
					bool flag = _currentScene == MdnScene.Match;
					bool flag2 = NpeState.TutorialState != NPEState.TutorialStates.Completed;
					TokenManager.ValidateToken(AccountClient, frontDoorConnectionServiceWrapper, _fdConnectionManager, flag || flag2);
				}
			}
			yield return new WaitForSeconds(1f);
		}
	}

	public void ShutdownImmediate()
	{
		if (_cardDatabaseLoader != null)
		{
			_cardDatabaseLoader.Cancel();
		}
		ShutDownShared();
	}

	public IEnumerator Shutdown()
	{
		if (_cardDatabaseLoader != null)
		{
			_cardDatabaseLoader.Cancel();
			yield return new WaitUntil(() => _cardDatabaseLoader == null || _cardDatabaseLoader.IsComplete);
		}
		ShutDownShared();
	}

	private void ShutDownShared()
	{
		_shuttingDown = true;
		MatchManager?.Dispose();
		SettingsMenuHost?.Destroy();
		_crossThreadLogger.ClearMatchManager();
		if (UnityPool != null)
		{
			UnityPool.Destroy();
			UnityPool = null;
		}
		CardViewBuilder = null;
		if (BILogger is IMutableBILogger mutableBILogger)
		{
			mutableBILogger.ClearFdServiceWrapper();
		}
		FrontDoorConnection?.DestroyFDConnection(TcpConnectionCloseType.NormalClosure, "OnDestroy");
		if (_crossThreadLogger != null)
		{
			_crossThreadLogger.Shutdown();
			_crossThreadLogger = null;
		}
		if (MasteryPassProvider != null)
		{
			MasteryPassProvider.OnDestroy();
			MasteryPassProvider = null;
		}
		_socialManager?.Destroy();
		if (_socialUI != null)
		{
			UnityEngine.Object.Destroy(_socialUI.gameObject);
		}
		AudioManager.UnInitializeAudio();
		SceneManager.sceneLoaded -= OnSceneLoaded;
		SceneManager.sceneLoaded -= Scenes.SceneLoaded;
		AssetLookupSystem.Blackboard.Cleanup();
		LoggingUtils.Shutdown();
		if (_memoryManager != null)
		{
			_memoryManager.Dispose();
		}
		_instance = null;
		TimedReplayRecorder?.CleanUp();
		TimedReplayRecorder = null;
	}

	public void OnDestroy()
	{
		_dssLogger?.Dispose();
		_assetLoadCountLogger?.Dispose();
		EnvironmentManager environmentManager = Pantry.Get<EnvironmentManager>();
		environmentManager.OnEnvironmentSet = (Action)Delegate.Remove(environmentManager.OnEnvironmentSet, new Action(OnEnvironmentSet));
		if (_instance != null)
		{
			if (!_applicationQuitting)
			{
				Debug.LogWarning("PAPA.OnDestroy() is happening without Shutdown() being called. Let me fix that for you.");
			}
			ShutdownImmediate();
		}
	}

	public void OnApplicationQuit()
	{
		_applicationQuitting = true;
		LoggingUtils.Flush();
		Actions?.DisableLogs();
	}

	public void OnApplicationPause(bool pause)
	{
		if (pause)
		{
			_dssLogger?.Flush();
			LoggingUtils.Flush();
		}
	}

	public static Coroutine StartGlobalCoroutine(IEnumerator routine, bool stopExisting = false)
	{
		return _instance._coroutineExecutor.StartGlobalCoroutine(routine, stopExisting);
	}

	public static void StopGlobalCoroutine(Coroutine coroutine)
	{
		_instance._coroutineExecutor.StopCoroutine(coroutine);
	}

	public static void ClearCaches()
	{
		_instance.ObjectPool.Clear();
		_instance.UnityPool.Clear();
		_instance.AssetLookupSystem.Blackboard.Clear();
		_instance.CardMaterialBuilder?.DecayUnreferencedMaterialBlocks();
		BoosterPayloadUtilities.ClearUnusedMaterials();
		AssetBundleManager.Instance.ClearCache();
		AssetBundleManager.Instance.ForceCull();
		Debug.Log("Caches cleared");
	}

	public static IEnumerator ClearPapaCache()
	{
		ClearCaches();
		AsyncOperation unloadOp = Resources.UnloadUnusedAssets();
		while (!unloadOp.isDone)
		{
			yield return null;
		}
		GC.Collect();
	}

	private static void InitSocialManager()
	{
		PAPA instance = _instance;
		if (instance._socialManager == null)
		{
			instance._socialManager = Pantry.Get<ISocialManager>();
		}
		_instance.SettingsMenuHost?.SetSocialClient(_instance._socialManager);
		_instance.ExternalChatManager.Init(_instance.FrontDoorConnection);
		_instance._socialManager.Connect();
	}

	private void OnFrontDoorConnected()
	{
		AccountClient = Pantry.Get<IAccountClient>();
		FrontDoorConnection = Pantry.Get<IFrontDoorConnectionServiceWrapper>();
		if (BILogger is IMutableBILogger mutableBILogger)
		{
			mutableBILogger.SetFrontDoorServiceWrapper(FrontDoorConnection);
		}
		MainThreadDispatcher.Dispatch(delegate
		{
			CrashReportHandler.SetUserMetadata("frontDoorSessionId", FrontDoorConnection.SessionId);
			CrashReportHandler.SetUserMetadata("playerId", AccountClient.AccountInformation?.PersonaID);
			LoggingUtils.UpdateVerbosity(AccountClient);
			ClientUserDeviceSpecs payload = BILogFactory.CreateClientUserDeviceSpecs(ClientSessionId);
			BILogger.Send(ClientBusinessEventType.ClientUserDeviceSpecs, payload);
			ClientConnected payload2 = BILogFactory.CreateClientConnected(ClientSessionId);
			BILogger.Send(ClientBusinessEventType.ClientConnected, payload2);
		});
	}
}
