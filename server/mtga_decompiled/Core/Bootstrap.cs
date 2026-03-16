using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AssetLookupTree;
using Assets.Core.Code.AssetBundles;
using Assets.Core.Meta.Utilities;
using Core;
using Core.BI;
using Core.Code.Promises;
using Core.Meta.MainNavigation;
using Core.Meta.MainNavigation.Challenge;
using Core.NPEStitcher;
using Core.Shared.Code.Connection;
using Core.Shared.Code.GreClientApi.Prompts;
using Core.Shared.Code.Network;
using Core.Shared.Code.Providers;
using Core.Shared.Code.ServiceFactories;
using Core.Shared.Code.Utilities;
using Cysharp.Threading.Tasks;
using Unity.Services.Core;
using UnityAsyncAwaitUtil;
using UnityEngine;
using UnityEngine.CrashReportHandler;
using Wizards.Arena.Promises;
using Wizards.MDN;
using Wizards.Models;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wizards.Mtga.Assets;
using Wizards.Mtga.BI;
using Wizards.Mtga.Deeplink;
using Wizards.Mtga.Diagnostics;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Installation;
using Wizards.Mtga.Platforms;
using Wizards.Mtga.Storage;
using Wizards.Mtga.Threading;
using Wizards.Unification.Models.Event;
using Wizards.Unification.Models.PlayBlade;
using Wotc.Mtga;
using Wotc.Mtga.AutoPlay;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Login;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;
using _3rdParty.Steam;

public class Bootstrap : MonoBehaviour, IProgress<AssetBundleProvisionProgress>
{
	[Serializable]
	public struct LogTypeTracePair
	{
		public LogType LogType;

		public StackTraceLogType TraceType;
	}

	public enum GameEntryType
	{
		Full,
		NPE,
		Autoplay
	}

	[SerializeField]
	private GameObject _environmentSelectorCanvasPrefab;

	[SerializeField]
	private GameObject _bundleSelectorCanvasPrefab;

	[SerializeField]
	private BuildInfoDebugDisplay _buildInfoDisplayPrefab;

	[SerializeField]
	private GameObject _wwiseGlobalListener;

	[SerializeField]
	private GameObject _mobileWwiseGlobalListener;

	private PAPA _papa;

	private ConnectionManager _connectionManager;

	private GameObject _environmentSelectorCanvas;

	private GameObject _bundleSelectorCanvas;

	private BuildInfoDebugDisplay _buildInfoDisplay;

	private IMGUIDrawer _onGUIDrawer;

	private CancellationTokenSource clientLifetimeTokenSource;

	private AssetBundleProvisioner _bundleProvisioner;

	private Task backgroundDownloadTask;

	private IProgress<AssetBundleProvisionProgress> bundleProgressReporter;

	private Coroutine GameStartUpCoroutine;

	private EmbeddedAssetPathResolver _embeddedPathResolver;

	private BacktraceIntegration _backtraceIntegration;

	private EnvironmentManager _environmentManager;

	private void Awake()
	{
		PantryInitializer.InitializePantry();
		LoggingUtils.Initialize();
		FileSystemUtils.Impl = new UnityWindowsFileSystemUtilsImpl();
		DefaultRiderPromptReplacer.Impl = new UnityRiderPromptReplacer();
		_environmentManager = Pantry.Get<EnvironmentManager>();
		_environmentManager.InitializeEnvironment();
		_environmentManager.InitializeEnvironmentSelector(GetEnvironmentSelector);
		BIEventTracker.Initialize();
		_backtraceIntegration = new BacktraceIntegration(PlatformContext.GetStorageContext(), () => Pantry.Get<IAccountClient>().AccountInformation?.PersonaID ?? "Unregistered User", this);
		Steam.Init();
		UnityServices.InitializeAsync();
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		GameStartUpCoroutine = StartCoroutine(Coroutine_GameStartup());
		CrashReportHandler.SetUserMetadata("installId", BILoggingUtils.InstallId);
		_onGUIDrawer = base.gameObject.AddComponent<IMGUIDrawer>();
		_onGUIDrawer.Init(0, "Bootstrap OnGUI", DrawOnGUI, 500, 100, PlatformContext.GetIMGUIScale());
		_onGUIDrawer.enabled = false;
		IClientVersionInfo versionInfo = Global.VersionInfo;
		string fullVersionString = versionInfo.GetFullVersionString();
		CrashReportHandler.SetUserMetadata("fullClientVersion", fullVersionString);
		SimpleLog.LogForRelease("Version: " + versionInfo.ApplicationVersion + " / " + fullVersionString + " / " + versionInfo.BuildInfo);
		clientLifetimeTokenSource = new CancellationTokenSource();
	}

	public void LogoutAndRestartGame(string context)
	{
		Pantry.Get<IAccountClient>().LogOut();
		BIEventType.LogoutSuccess.SendWithDefaults();
		RestartGame(context);
	}

	public void RestartGame(string context)
	{
		StartCoroutine(Coroutine_RestartGame());
	}

	private IEnumerator Coroutine_RestartGame()
	{
		UpdatePoliciesPanel.ResetPolicyAccepted();
		RegistrationPanel.ResetPolicyAccepted();
		if (GameStartUpCoroutine != null)
		{
			StopCoroutine(GameStartUpCoroutine);
			GameStartUpCoroutine = null;
		}
		yield return Pantry.Get<WrapperSceneManagement>().UnloadWrapper(new WrapperSceneLifeCycle[1] { WrapperSceneLifeCycle.LoggedIn });
		yield return Shutdown();
		Scenes.LoadScene("Bootstrap");
	}

	private IEnumerator Coroutine_GameStartup()
	{
		Task logDeletionTask = LoggingUtils.PurgeOldLogs();
		_papa = PAPA.Create();
		IStorageContext storageContext = PlatformContext.GetStorageContext();
		yield return PlatformContext.TryEstablishPlatformAuthentication();
		RuntimePlatform platform = Application.platform;
		if (platform == RuntimePlatform.IPhonePlayer || platform == RuntimePlatform.Android)
		{
			UnityEngine.Object.Instantiate(_mobileWwiseGlobalListener);
		}
		else
		{
			UnityEngine.Object.Instantiate(_wwiseGlobalListener);
		}
		FrontDoorConnectionManager frontDoorConnectionManager = Pantry.Get<FrontDoorConnectionManager>();
		frontDoorConnectionManager.RestartRequested += RestartGame;
		frontDoorConnectionManager.LogoutAndRestartRequested += LogoutAndRestartGame;
		yield return Coroutine_CheckForUpdate();
		new InitializeConnectionCommand().Execute(_papa.AssetLookupSystem, frontDoorConnectionManager, Pantry.Get<IFrontDoorConnectionServiceWrapper>(), _papa.MatchManager, _papa.Matchmaking, _papa.EventManager, _papa.KeyBoardManager, _papa.Actions, _papa.SettingsMenuHost);
		_connectionManager = Pantry.Get<ConnectionManager>();
		BIEventType.ClientStartup.SendWithDefaults(("BuildVersion", Global.VersionInfo.ContentVersion.ToString()));
		ResourceErrorLogger.SendBILogMessages(_papa.BILogger);
		yield return _connectionManager.RingDoorbell();
		AssetLoader.Initialize(_papa.BILogger, Pantry.Get<ResourceErrorMessageManager>());
		_embeddedPathResolver = storageContext.GetEmbeddedBundlePathResolver();
		PopulateAssetBundleSources();
		AudioManager.InitializeAudioManager(_papa.AssetLookupSystem, storageContext);
		yield return new WaitUntil(() => AudioManager.Instance.Initialized);
		AutoLoginState state = AutoLoginState.None;
		IAccountClient accountClient = Pantry.Get<IAccountClient>();
		if (accountClient.RememberMe)
		{
			yield return frontDoorConnectionManager.TryFastLogIn(delegate(AutoLoginState x)
			{
				state = x;
			});
		}
		if (state != AutoLoginState.Connected)
		{
			yield return Coroutine_FullLogInSequence();
		}
		if (accountClient.AccountInformation.HasRole_Debugging() || Debug.isDebugBuild)
		{
			_buildInfoDisplay = UnityEngine.Object.Instantiate(_buildInfoDisplayPrefab);
			AssetBundleSourcesModel assetBundleSourcesModel = Pantry.Get<AssetBundleSourcesModel>();
			_buildInfoDisplay.UpdateBundlesText(assetBundleSourcesModel.CurrentSource);
		}
		Task task = BIEventTracker.RequestTrackingAuthorization();
		StartCoroutine(Coroutine_PostLogInSequence());
		if (!task.IsCompleted)
		{
			Task taskResult;
			yield return task.WaitYield(out taskResult);
		}
		yield return logDeletionTask;
		GameStartUpCoroutine = null;
	}

	private void PopulateAssetBundleSources()
	{
		SetUpBundleSelectorCanvas();
		_bundleProvisioner = Pantry.Get<AssetBundleProvisioner>();
	}

	private void SetUpBundleSelectorCanvas()
	{
		_bundleSelectorCanvas = UnityEngine.Object.Instantiate(_bundleSelectorCanvasPrefab);
		UnityEngine.Object.DontDestroyOnLoad(_bundleSelectorCanvas);
		BundleEndpointSelector componentInChildren = _bundleSelectorCanvas.GetComponentInChildren<BundleEndpointSelector>();
		componentInChildren.NewBundleSourceSelected += OnBundleSourceSelected;
		AssetBundleSourcesModel sources = Pantry.Get<AssetBundleSourcesModel>();
		componentInChildren.SetSources(sources);
	}

	private void OnBundleSourceSelected(IAssetBundleSource newSource)
	{
		RestartGame("Bundle Selector");
	}

	private IEnumerator Coroutine_FullLogInSequence()
	{
		Task<AssetPrepScene> loadAssetPrep = AssetPrepScene.LoadAndInit();
		yield return loadAssetPrep.AsCoroutine();
		AssetPrepScene assetPrepScene = loadAssetPrep.Result;
		yield return assetPrepScene.Coroutine_AttemptWithRetries(() => _bundleProvisioner.PrepareDownload(_embeddedPathResolver, assetPrepScene, clientLifetimeTokenSource.Token), null);
		_buildInfoDisplay?.UpdateBundlesText(AssetBundleProvisioner.Source, _bundleProvisioner.ActiveManifests);
		long additionalRequiredDownloadBytes = _bundleProvisioner.GetAdditionalRequiredDownloadBytes(AssetPriority.Boot);
		if (additionalRequiredDownloadBytes > 0)
		{
			if (PlatformUtils.IsHandheld())
			{
				yield return assetPrepScene.WaitForAllowDownloadConfirmationYield(additionalRequiredDownloadBytes);
			}
			SendBIEvent_WaitingForAssets(1);
			_bundleProvisioner.AssetPriorityLimit = AssetPriority.Boot;
			_bundleProvisioner.ResetTimers();
			yield return assetPrepScene.Download_AttemptWithRetries(clientLifetimeTokenSource.Token, _bundleProvisioner.ErrorOccured, _bundleProvisioner.BundleCountRequired(AssetPriority.Boot));
			_bundleProvisioner.LogTimerResults(AssetPriority.Boot);
		}
		yield return PrepareLoginAssets(assetPrepScene);
		yield return _connectionManager.FullLogIn();
	}

	private IEnumerator Coroutine_PostLogInSequence()
	{
		INpeStrategy npe = Pantry.Get<INpeStrategy>();
		yield return new WaitUntil(() => npe.Initialized);
		Task<AssetPrepScene> loadAssetPrep = AssetPrepScene.LoadAndInit();
		yield return loadAssetPrep.AsCoroutine();
		AssetPrepScene assetPrepScene = loadAssetPrep.Result;
		AssetPriority maxPriority = ((npe.TutorialRequired || OverridesConfiguration.Local.GetFeatureToggleValue("npe.force_tutorial")) ? AssetPriority.NPE : AssetPriority.General);
		yield return assetPrepScene.Coroutine_AttemptWithRetries(() => _bundleProvisioner.PrepareDownload(_embeddedPathResolver, assetPrepScene, clientLifetimeTokenSource.Token), _bundleProvisioner.ErrorOccured);
		_buildInfoDisplay?.UpdateBundlesText(AssetBundleProvisioner.Source, _bundleProvisioner.ActiveManifests);
		if (_bundleProvisioner.CheckHasMissingBundlesInQueue(maxPriority))
		{
			if (PlatformUtils.IsHandheld())
			{
				if (npe.TutorialRequired)
				{
					yield return assetPrepScene.WaitForDownloadInBackgroundConfirmationYield(_bundleProvisioner.GetAdditionalRequiredDownloadBytes(AssetPriority.General));
				}
				else
				{
					yield return assetPrepScene.WaitForAllowDownloadConfirmationYield(_bundleProvisioner.GetAdditionalRequiredDownloadBytes(maxPriority));
				}
			}
			SendBIEvent_WaitingForAssets((int)maxPriority);
			_bundleProvisioner.AssetPriorityLimit = maxPriority;
			_bundleProvisioner.ResetTimers();
			yield return assetPrepScene.Download_AttemptWithRetries(clientLifetimeTokenSource.Token, delegate
			{
				_bundleProvisioner.ErrorOccured();
			}, _bundleProvisioner.BundleCountRequired(maxPriority));
			_bundleProvisioner.LogTimerResults(maxPriority);
		}
		yield return new WaitUntil(() => _papa.FrontDoorConnection.Connected);
		yield return PrepareAssets(assetPrepScene);
		yield return EnterIntoGame((int)maxPriority, postNPEFlow: false, assetPrepScene);
	}

	public void NpeCompleteStartFullGame()
	{
		StartCoroutine(Coroutine_EnterFullGameFromNpe());
	}

	private IEnumerator Coroutine_EnterFullGameFromNpe()
	{
		_papa.SocialUISetVisible(visible: false);
		Task<AssetPrepScene> loadAssetPrep = AssetPrepScene.LoadAndInit();
		yield return loadAssetPrep.AsCoroutine();
		AssetPrepScene assetPrepScene = loadAssetPrep.Result;
		if (backgroundDownloadTask != null && !backgroundDownloadTask.IsCompleted)
		{
			bundleProgressReporter = assetPrepScene;
			Task taskResult;
			yield return backgroundDownloadTask.WaitYield(out taskResult);
		}
		backgroundDownloadTask = null;
		AssetPriority maxPriority = AssetPriority.General;
		if (_bundleProvisioner.CheckHasMissingBundlesInQueue(maxPriority))
		{
			if (PlatformUtils.IsHandheld())
			{
				yield return assetPrepScene.WaitForAllowDownloadConfirmationYield(_bundleProvisioner.GetAdditionalRequiredDownloadBytes(maxPriority));
			}
			SendBIEvent_WaitingForAssets((int)maxPriority);
			_bundleProvisioner.AssetPriorityLimit = maxPriority;
			_bundleProvisioner.ResetTimers();
			yield return assetPrepScene.Download_AttemptWithRetries(clientLifetimeTokenSource.Token, _bundleProvisioner.ErrorOccured, _bundleProvisioner.BundleCountRequired(maxPriority));
			_bundleProvisioner.LogTimerResults(maxPriority);
		}
		yield return new WaitUntil(() => _papa.FrontDoorConnection.Connected);
		yield return PrepareAssets(assetPrepScene);
		yield return EnterIntoGame((int)maxPriority, postNPEFlow: true, assetPrepScene);
		_papa.SocialUISetVisible(visible: true);
	}

	private Task StartSilentBackgroundDownload(bool hasAllowedDownloading)
	{
		RuntimePlatform platform = Application.platform;
		if ((platform == RuntimePlatform.IPhonePlayer || platform == RuntimePlatform.Android) && !hasAllowedDownloading)
		{
			return null;
		}
		bundleProgressReporter = null;
		_bundleProvisioner.AssetPriorityLimit = AssetPriority.General;
		return Task.Run(async delegate
		{
			await _bundleProvisioner.DoDownload(this, clientLifetimeTokenSource.Token);
		});
	}

	void IProgress<AssetBundleProvisionProgress>.Report(AssetBundleProvisionProgress value)
	{
		bundleProgressReporter?.Report(value);
	}

	private IEnumerator PrepareLoginAssets(AssetPrepScene assetPrepScene)
	{
		ResourceErrorLogger.OnAssetBundleError += OnResourceError;
		SendBIEvent_PrepareAssetsStart();
		assetPrepScene.SetPreparingAssetsString("Boot/BootScene_InitializingBundleManager");
		StartCoroutine(PAPA.ClearPapaCache());
		AssetLoader.PrepareAssets(_bundleProvisioner, _embeddedPathResolver);
		SendBIEvent_PrepareAssetsStep("AssetBundleManager initialized");
		assetPrepScene.SetPreparingAssetsString("Boot/BootScene_LoadingLocDatabase");
		yield return new LoadLocDatabaseUniTask().Load().ToCoroutine();
		SendBIEvent_PrepareAssetsStep("Localization database loaded");
		SendBIEvent_PrepareAssetsEnd();
		ResourceErrorLogger.OnAssetBundleError -= OnResourceError;
	}

	private IEnumerator PrepareAssets(AssetPrepScene assetPrepScene)
	{
		ResourceErrorLogger.OnAssetBundleError += OnResourceError;
		SendBIEvent_PrepareAssetsStart();
		assetPrepScene.SetPreparingAssetsString("Boot/BootScene_InitializingBundleManager");
		AssetLoader.PrepareAssets(_bundleProvisioner, _embeddedPathResolver);
		SendBIEvent_PrepareAssetsStep("AssetBundleManager initialized");
		assetPrepScene.SetPreparingAssetsString("Boot/BootScene_LoadingLocDatabase");
		yield return new LoadLocDatabaseUniTask().Load().ToCoroutine();
		SendBIEvent_PrepareAssetsStep("Localization database loaded");
		assetPrepScene.SetPreparingAssetsString("Boot/BootScene_LoadingCardDatabase");
		yield return _papa.LoadCardDatabase();
		_papa.CreateFormatManager();
		SendBIEvent_PrepareAssetsStep("Card database loaded");
		_papa.SetupDeckFormatFactory();
		assetPrepScene.SetPreparingAssetsString("Boot/BootScene_InitializingCardBuilders");
		_papa.InitCardBuilders();
		SendBIEvent_PrepareAssetsStep("Card builders initialized");
		if (AutoPlayManager.CanRunAutoPlay())
		{
			_papa.InitAutoPlay();
		}
		assetPrepScene.SetPreparingAssetsString("Boot/BootScene_RequestingFormatData");
		FormatManager formatManager = Pantry.Get<FormatManager>();
		yield return formatManager.RefreshFormatsYield();
		LoadSetMetadataUniTask loadSetMetadataUniTask = new LoadSetMetadataUniTask();
		yield return loadSetMetadataUniTask.Load().ToCoroutine();
		SendBIEvent_PrepareAssetsStep("Formats setup");
		assetPrepScene.SetPreparingAssetsString("Boot/BootScene_InitializingAudio");
		AudioManager.InitializeAudio(_papa.AssetLookupSystem);
		yield return new WaitUntil(() => AudioManager.Instance.Initialized);
		SendBIEvent_PrepareAssetsStep("Audio initialized");
		assetPrepScene.SetPreparingAssetsString("Boot/BootScene_InitializingNPE");
		_papa.InitializeNpe();
		SendBIEvent_PrepareAssetsStep("NPE initialized");
		assetPrepScene.SetPreparingAssetsString("Boot/BootScene_InitializingMatchmaking");
		_papa.InitializeMatchMaking();
		SendBIEvent_PrepareAssetsStep("Matchmaking initialized");
		SendBIEvent_PrepareAssetsEnd();
		ResourceErrorLogger.OnAssetBundleError -= OnResourceError;
	}

	private static void SendBIEvent_EnteringGame(GameEntryType gameType)
	{
		BIEventType.EnteringGame.SendWithDefaults(("Type", gameType.ToString()));
	}

	private static void SendBIEvent_PrepareAssetsStart()
	{
		BIEventType.PreparingAssetsStart.SendWithDefaults();
	}

	private static void SendBIEvent_PrepareAssetsEnd()
	{
		BIEventType.PreparingAssetsEnd.SendWithDefaults();
	}

	private static void SendBIEvent_RejoiningActiveMatch(string activeMatchId)
	{
		BIEventType.RejoiningActiveMatch.SendWithDefaults(("MatchId", activeMatchId));
	}

	private static void SendBIEvent_PrepareAssetsStep(string step)
	{
		BIEventType.PreparingAssetsStep.SendWithDefaults(("Step", step));
	}

	private static void SendBIEvent_WaitingForAssets(int section)
	{
		BIEventType.WaitingForAssets.SendWithDefaults(("Section", section.ToString()));
	}

	private void OnResourceError(string error, Dictionary<string, string> details)
	{
		SyncContextUtil.RunOnMainUnityThread(delegate
		{
			Pantry.Get<ResourceErrorMessageManager>().ShowError("Error loading resource", error, details.AsTuples());
		});
	}

	private IEnumerator EnterIntoGame(int maxPriority, bool postNPEFlow, AssetPrepScene assetPrepScene)
	{
		Task altPreloadTask = TreePreloaderFactory.Create().PreloadTreesAsync(_papa.AssetTreeLoader);
		Task taskResult;
		if (maxPriority == 25)
		{
			assetPrepScene.SetPreparingAssetsString("Boot/BootScene_LoadingInitialScene");
			yield return altPreloadTask.WaitYield(out taskResult);
			if (taskResult.Exception != null)
			{
				OnPreloadTreesException(taskResult.Exception);
			}
			Promise<StaticContentResponse> promise = StaticContentController.GetStaticContent(new List<EStaticContent> { EStaticContent.QueueTips }, staticContentProviders: new StaticContentProviders
			{
				QueueTipProvider = Pantry.Get<IQueueTipProvider>()
			}, staticContentController: Pantry.Get<StaticContentController>(), fdc: Pantry.Get<IFrontDoorConnectionServiceWrapper>().FDCAWS);
			yield return promise.AsCoroutine();
			SendBIEvent_EnteringGame(GameEntryType.NPE);
			_papa.NpeState.SetStateToEngageTutorialEventFlow(thisIsFirstTimeDoingTutorial: true);
			backgroundDownloadTask = StartSilentBackgroundDownload(assetPrepScene.hasAllowedDownloading);
			SendBIEvent_PrepareAssetsEnd();
			PAPA.SceneLoading.LoadNPEScene();
			yield break;
		}
		SendBIEvent_EnteringGame(GameEntryType.Full);
		_papa.NpeState.ConsiderTutorialCompleted();
		assetPrepScene.SetPreparingAssetsString("Boot/BootScene_CheckingForActiveMatches");
		IActiveMatchesServiceWrapper activeMatchesServiceWrapper = Pantry.Get<IActiveMatchesServiceWrapper>();
		Promise<NewMatchCreatedConfig> activeMatchesPromise = activeMatchesServiceWrapper.GetActiveMatches().Convert((List<NewMatchCreatedConfig> p) => p?.FirstOrDefault());
		yield return activeMatchesPromise.AsCoroutine();
		if (activeMatchesPromise.Result != null)
		{
			NewMatchCreatedConfig activeMatch = activeMatchesPromise.Result;
			assetPrepScene.SetPreparingAssetsString("Boot/BootScene_LoadingInitialScene");
			SendBIEvent_PrepareAssetsEnd();
			List<EStaticContent> requestedStaticContent = new List<EStaticContent> { EStaticContent.AvailableCosmetics };
			StaticContentProviders staticContentProviders = new StaticContentProviders
			{
				CosmeticsProvider = Pantry.Get<CosmeticsProvider>()
			};
			Promise<StaticContentResponse> staticContentPromise = StaticContentController.GetStaticContent(requestedStaticContent, Pantry.Get<StaticContentController>(), Pantry.Get<IFrontDoorConnectionServiceWrapper>().FDCAWS, staticContentProviders);
			IEmoteEntryLoader emoteEntryLoader = _papa.EmoteEntryLoader;
			Promise<MercantileCollections> emotesRequest = (emoteEntryLoader.IsLoaded ? new SimplePromise<MercantileCollections>(null) : staticContentPromise.Then((Promise<StaticContentResponse> _) => emoteEntryLoader.Load()));
			Promise<CardsAndCacheVersion> refreshCardsRequest = _papa.InventoryManager.RefreshCards();
			Promise<CombinedRankInfo> rankRequest = Pantry.Get<IPlayerRankServiceWrapper>().GetPlayerRankInfo();
			Promise<CosmeticsClient> inventoryRequest = _papa.CosmeticsProvider.GetPlayerOwnedCosmetics();
			PlayerPrefsDataProvider playerPrefsDataProvider = Pantry.Get<PlayerPrefsDataProvider>();
			Promise<DTO_PlayerPreferences> playerPrefsPromise = playerPrefsDataProvider.Initialize();
			Promise<bool> challengesPromise = Pantry.Get<PVPChallengeController>().ReconnectAndCleanupOldChallenges();
			_papa.Matchmaking.LaunchFromReconnect();
			yield return new WaitUntil(() => staticContentPromise.IsDone && rankRequest.IsDone && inventoryRequest.IsDone && refreshCardsRequest.IsDone && playerPrefsPromise.IsDone && emotesRequest.IsDone && challengesPromise.IsDone);
			if (rankRequest.Successful)
			{
				Pantry.Get<IPlayerRankServiceWrapper>().CombinedRank = rankRequest.Result;
			}
			SendBIEvent_RejoiningActiveMatch(activeMatch.matchId);
			string eventId = activeMatch.eventId;
			yield return _papa.EventManager.Coroutine_GetEventsAndCourses();
			EventContext currentEvent = _papa.EventManager.EventContexts.FirstOrDefault((EventContext e) => e.PlayerEvent.MatchMakingName == eventId);
			assetPrepScene.SetPreparingAssetsString("Boot/BootScene_JoiningMatch");
			SendBIEvent_PrepareAssetsEnd();
			yield return altPreloadTask.WaitYield(out taskResult);
			if (taskResult.Exception != null)
			{
				OnPreloadTreesException(taskResult.Exception);
			}
			_papa.Matchmaking.JoinMatchFromReconnect(activeMatch, currentEvent);
		}
		else
		{
			assetPrepScene.SetPreparingAssetsString("Boot/BootScene_LoadingInitialScene");
			SendBIEvent_PrepareAssetsEnd();
			yield return altPreloadTask.WaitYield(out taskResult);
			if (taskResult.Exception != null)
			{
				OnPreloadTreesException(taskResult.Exception);
			}
			AltAssetReference.ReferencesToDeDupe.Clear();
			PreloadFutureBundles(GetSceneLoadUniTask(postNPEFlow)).Forget();
		}
		void OnPreloadTreesException(Exception exception)
		{
			SimpleLog.LogException(exception);
			_papa.BILogger.Send(ClientBusinessEventType.ResourceError, new ResourceError
			{
				Message = "Failed to preload trees",
				Error = exception.Message,
				EventTime = DateTime.UtcNow
			});
		}
	}

	private UniTask? GetSceneLoadUniTask(bool postNPEFlow)
	{
		return PAPA.SceneLoading.LoadWrapperScene(new HomePageContext
		{
			CameFromNPE = postNPEFlow
		});
	}

	private UniTask PreloadFutureBundles(UniTask? first)
	{
		if (!_bundleProvisioner.CheckHasMissingBundlesInQueue(AssetPriority.Future) || !first.HasValue)
		{
			return UniTask.CompletedTask;
		}
		return first.Value.ContinueWith(delegate
		{
			_bundleProvisioner.AssetPriorityLimit = AssetPriority.Future;
			return _bundleProvisioner.DoDownload(null, clientLifetimeTokenSource.Token);
		});
	}

	public IEnumerator Shutdown()
	{
		Scenes.LoadScene("EmptyScene");
		yield return null;
		Pantry.Get<FrontDoorConnectionManager>().ClearActionsOnRestart();
		yield return _papa.Shutdown();
		yield return null;
		UnityEngine.Object.Destroy(_papa.gameObject);
		MainThreadDispatcher.Instance.Shutdown();
		if (_environmentSelectorCanvas != null)
		{
			UnityEngine.Object.Destroy(_environmentSelectorCanvas);
			_environmentSelectorCanvas = null;
		}
		if (_bundleSelectorCanvas != null)
		{
			UnityEngine.Object.Destroy(_bundleSelectorCanvas);
			_bundleSelectorCanvas = null;
		}
		if (_buildInfoDisplay != null)
		{
			UnityEngine.Object.Destroy(_buildInfoDisplay.gameObject);
			_buildInfoDisplay = null;
		}
		SystemMessageManager.Instance.Shutdown();
		Steam.Shutdown();
		_backtraceIntegration?.Dispose();
		_backtraceIntegration = null;
		_papa = null;
		Pantry.Restart();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private IEnumerator Coroutine_CheckForUpdate()
	{
		return Coroutine_CheckForUpdate(TimeSpan.FromSeconds(30.0));
	}

	private IEnumerator Coroutine_CheckForUpdate(TimeSpan timeout)
	{
		IInstallationController controller = PlatformContext.GetInstallationController();
		Task<UpdateCheckResults> task = controller.CheckForUpdate();
		task.ConfigureAwait(continueOnCapturedContext: false);
		if (MDNPlayerPrefs.ForceUpdate)
		{
			SimpleLog.LogForRelease("Forcing client update because of hack");
			yield return ShowOutdatedClientWarningDialogYield(controller, delegate
			{
				RestartGame("ForceUpdated");
			});
		}
		else
		{
			if (task.IsCompleted && task.Result.HasFlag(UpdateCheckResults.NotSupported))
			{
				yield break;
			}
			DateTime cutoff = DateTime.UtcNow + timeout;
			UpdateCheckResults result = UpdateCheckResults.Pending;
			while (!task.IsCompleted)
			{
				if (DateTime.UtcNow > cutoff)
				{
					result = UpdateCheckResults.AtLatest;
					break;
				}
				yield return null;
			}
			if (result == UpdateCheckResults.Pending)
			{
				result = task.Result;
			}
			if (result.HasFlag(UpdateCheckResults.UpdateRequired))
			{
				SimpleLog.LogForRelease("Client is out of date. Starting update process...");
				yield return ShowOutdatedClientWarningDialogYield(controller, delegate
				{
					RestartGame("Updated");
				});
			}
			else if (!result.HasFlag(UpdateCheckResults.AtLatest) && !result.HasFlag(UpdateCheckResults.NotSupported))
			{
				SimpleLog.LogError("Unknown error while checking for update");
			}
		}
	}

	private IEnumerator ShowOutdatedClientWarningDialogYield(IInstallationController controller, Action okAction)
	{
		bool waiting = true;
		List<SystemMessageManager.SystemMessageButtonData> list = new List<SystemMessageManager.SystemMessageButtonData>();
		SystemMessageManager.SystemMessageButtonData item = new SystemMessageManager.SystemMessageButtonData
		{
			Text = Languages.ActiveLocProvider.GetLocalizedText("DuelScene/EscapeMenu/CheckStatus"),
			Callback = delegate
			{
				UrlOpener.OpenURL(Languages.ActiveLocProvider.GetLocalizedText("MainNav/WebLink/StatusPage"));
			},
			HideOnClick = false
		};
		SystemMessageManager.SystemMessageButtonData item2 = new SystemMessageManager.SystemMessageButtonData
		{
			Text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Popups/Modal/ModalOptions_Cancel"),
			Callback = delegate
			{
				waiting = false;
			}
		};
		list.Add(item2);
		list.Add(item);
		if (controller.CanForceStartExternalUpdate || MDNPlayerPrefs.ForceUpdate)
		{
			SystemMessageManager.SystemMessageButtonData item3 = new SystemMessageManager.SystemMessageButtonData
			{
				Text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Popups/Modal/ModalOptions_OK"),
				Callback = delegate
				{
					controller.StartExternalUpdate();
					waiting = false;
					okAction();
				}
			};
			list.Add(item3);
		}
		SystemMessageManager.Instance.ShowMessage(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Invalid_Client_Version_Title"), PlatformContext.GetDistributionServiceString(), list);
		DeepLinking.SaveCurrentDeeplink();
		while (waiting)
		{
			yield return null;
		}
	}

	public void DrawOnGUI(int windowId)
	{
		if (!(_papa == null))
		{
			EnvironmentDescription currentEnvironment = Pantry.Get<FrontDoorConnectionManager>().CurrentEnvironment;
			if (currentEnvironment != null)
			{
				GUI.color = Color.red;
				GUILayout.Box(currentEnvironment.name + " - " + currentEnvironment.GetFullUri());
			}
		}
	}

	private void Update()
	{
		_onGUIDrawer.enabled = Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.RightAlt);
	}

	private void OnApplicationQuit()
	{
		clientLifetimeTokenSource.Cancel();
		clientLifetimeTokenSource.Dispose();
		clientLifetimeTokenSource = null;
		BIEventType.ClientClosed.SendWithDefaults();
		Pantry.Restart();
	}

	private void OnNewEnvironmentSelected(string envName)
	{
		_environmentManager.FindEnvironment(envName);
		MDNPlayerPrefs.PreviousFDServer = envName;
		PlayerPrefsExt.Save();
		RestartGame("Environment Switcher");
	}

	private EnvironmentSelector GetEnvironmentSelector()
	{
		if (_environmentSelectorCanvas == null)
		{
			_environmentSelectorCanvas = UnityEngine.Object.Instantiate(_environmentSelectorCanvasPrefab);
			UnityEngine.Object.DontDestroyOnLoad(_environmentSelectorCanvas);
			EnvironmentSelector componentInChildren = _environmentSelectorCanvas.GetComponentInChildren<EnvironmentSelector>();
			componentInChildren.NewEnvironmentSelected += OnNewEnvironmentSelected;
			return componentInChildren;
		}
		return _environmentSelectorCanvas.GetComponentInChildren<EnvironmentSelector>();
	}
}
