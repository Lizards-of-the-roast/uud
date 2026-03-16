using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.Decks;
using Core.Code.Input;
using Core.Code.PlayerInbox;
using Core.Code.PrizeWall;
using Core.MainNavigation.RewardTrack;
using Core.Meta.MainNavigation;
using Core.Meta.MainNavigation.Achievements;
using Core.Meta.MainNavigation.Challenge;
using Core.Meta.MainNavigation.EventPageV2;
using Core.Meta.MainNavigation.PopUps;
using Core.Meta.MainNavigation.Profile;
using Core.Meta.MainNavigation.RewardTrack;
using Core.Meta.MainNavigation.Store;
using Core.Meta.Social.Tables;
using Core.Meta.UI;
using Core.Shared.Code.Connection;
using DG.Tweening;
using EventPage;
using EventPage.CampaignGraph;
using MTGA.KeyboardManager;
using MTGA.Social;
using Newtonsoft.Json.Linq;
using Pooling;
using ProfileUI;
using SharedClientCore.SharedClientCore.Code.Providers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using Wizards.Arena.Client.Logging;
using Wizards.MDN;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wizards.Mtga.Diagnostics;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Logging;
using Wizards.Mtga.PlayBlade;
using Wizards.Mtga.PreferredPrinting;
using Wotc.Mtga;
using Wotc.Mtga.Cards.ArtCrops;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;
using Wotc.Mtga.Wrapper.BonusPack;
using Wotc.Mtga.Wrapper.Draft;
using Wotc.Mtga.Wrapper.Mailbox;
using Wotc.Mtga.Wrapper.PacketSelect;

public class SceneLoader : MonoBehaviour
{
	public enum NavMethod
	{
		Unknown,
		Carousel,
		Banner,
		PlayButton,
		Deeplink
	}

	[SerializeField]
	private Camera _mainCamera;

	[SerializeField]
	private bool _motdOncePerSession;

	private NavBarController _navBarInstance;

	private GameObject _loadingInstance;

	private ICardRolloverZoom _cardZoomViewInstance;

	private ContentControllerObjectives _objectivesContentControllerInstance;

	private ContentControllerRewards _rewardsContentControllerInstance;

	private ContentControllerPlayerInbox _playerInboxContentControllerInstance;

	private AchievementsContentController _achievementsContentController;

	private SceneUITransforms _sceneUITransforms;

	private Dictionary<Type, PopupBase> _popupDictionary = new Dictionary<Type, PopupBase>();

	[Header("Transition")]
	[SerializeField]
	private float _transitionFadeDuration;

	[SerializeField]
	private Ease _transitionFadeEaseMethod;

	[SerializeField]
	private CanvasGroup _transitionCanvasGroup;

	private ISocialManager _socialManager;

	private SocialUI _socialUI;

	private AssetLookupSystem _assetLookupSystem;

	private SettingsMenuHost _settingsMenuHost;

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	private CardMaterialBuilder _cardMaterialBuilder;

	private IUnityObjectPool _unityObjectPool;

	private IObjectPool _genericObjectPool;

	private IClientLocProvider _locManager;

	private InventoryManager _invManager;

	private IFrontDoorConnectionServiceWrapper _frontDoorWrapper;

	private IPreconDeckServiceWrapper _preconDeckServiceWrapper;

	private IArtCropProvider _cropDatabase;

	private CardArtTextureLoader _artTextureLoader;

	private MOTDSession _motdSession;

	private UnityCrossThreadLogger _unityCrossThreadLogger;

	private DateTime _lastSceneChangeTime = DateTime.Now;

	private IAccountClient _accountClient;

	private SetMasteryDataProvider _masteryPassProvider;

	private NPEState _npeState;

	private MatchManager _matchManager;

	private PopupManager _popupManager;

	private WrapperCompass _wrapperCompass;

	private IEmoteDataProvider _emoteDataProvider;

	private static UnityLogger _logger;

	private IBILogger _biLogger;

	private CosmeticsProvider _cosmetics;

	private KeyboardManager _keyboardManager;

	private IActionSystem _actionSystem;

	private IGreLocProvider _greLocalizationManager;

	private ISetMetadataProvider _setMetadataProvider;

	private ITitleCountManager _titleCountManager;

	private List<string> _loadingIndicatorIds = new List<string>();

	private const string LOADING_TRANSITION_ID = "SceneLoader.LeaveEnter";

	private GameObject _sparkyMetaOnboarding;

	private WrapperSceneManagement _wrapperSceneManager;

	private bool _isLoading;

	private int _loadingCount;

	private Coroutine _landingSequence;

	private bool _isLoadingMotD;

	private NavContentLoader _navContentLoader = new NavContentLoader();

	private NavContentType _contentPending;

	private ICustomTokenProvider _customTokenProvider;

	private VoucherDataProvider _voucherDataProvider;

	private Transform _contentParent => _sceneUITransforms.ContentParent;

	public Transform _popupsParent => _sceneUITransforms.PopupsParent;

	private Transform _overlayParent => _sceneUITransforms.OverlayParent;

	public Camera MainCamera => _mainCamera;

	public IBILogger BILogger => _biLogger;

	public NavContentController CurrentNavContent { get; private set; }

	public NavContentType CurrentContentType => CurrentNavContent?.NavContentType ?? NavContentType.None;

	public bool IsLoading
	{
		get
		{
			if (!_isLoading)
			{
				return _isLoadingMotD;
			}
			return true;
		}
	}

	public NavContentType ActivatingContent { get; private set; }

	public bool IsInDuelScene => PAPA.SceneLoading.CurrentScene == PAPA.MdnScene.Match;

	public SystemMessageManager SystemMessages => SystemMessageManager.Instance;

	public bool IsCardViewerEnabled
	{
		get
		{
			Type typeFromHandle = typeof(CardViewerController);
			if (!_popupDictionary.TryGetValue(typeFromHandle, out var value))
			{
				return false;
			}
			return value.IsShowing;
		}
	}

	public event Action SceneLoaded;

	public event Action<BladeEventInfo> PlayBladeQueueSelected;

	public event Action<BladeEventFilter> PlayBladeFilterSelected;

	public NavBarController GetNavBar()
	{
		if (!_wrapperSceneManager.IsSceneLoaded("NavBar"))
		{
			_wrapperSceneManager.LoadScene(new WrapperSceneInformation("NavBar", WrapperSceneLifeCycle.Wrapper));
		}
		return UnityUtilities.FindObjectOfType<NavBarController>(includeInactive: true);
	}

	public ICardRolloverZoom GetCardZoomView()
	{
		return Pantry.Get<ICardRolloverZoom>();
	}

	public void Init(PAPA papa, MOTDSession motdSession, UnityCrossThreadLogger unityCrossThreadLogger, IArtCropProvider cropDatabase, CardArtTextureLoader artTextureLoader, IEmoteDataProvider emoteDataProvider, CosmeticsProvider cosmetics, IGreLocProvider greLocalizationManager, ICustomTokenProvider customTokenProvider, VoucherDataProvider voucherDataProvider)
	{
		_assetLookupSystem = papa.AssetLookupSystem;
		_motdSession = motdSession;
		_unityCrossThreadLogger = unityCrossThreadLogger;
		_cardDatabase = papa.CardDatabase;
		_cardViewBuilder = papa.CardViewBuilder;
		_cardMaterialBuilder = papa.CardMaterialBuilder;
		_cropDatabase = cropDatabase;
		_artTextureLoader = artTextureLoader;
		_keyboardManager = papa.KeyBoardManager;
		_actionSystem = papa.Actions;
		_biLogger = Pantry.Get<IBILogger>();
		_cosmetics = cosmetics;
		_greLocalizationManager = greLocalizationManager;
		_unityObjectPool = papa.UnityPool;
		_genericObjectPool = papa.ObjectPool;
		_emoteDataProvider = emoteDataProvider;
		_locManager = Languages.ActiveLocProvider;
		_invManager = papa.InventoryManager;
		_frontDoorWrapper = papa.FrontDoorConnection;
		_accountClient = Pantry.Get<IAccountClient>();
		_masteryPassProvider = papa.MasteryPassProvider;
		_npeState = papa.NpeState;
		_matchManager = papa.MatchManager;
		_preconDeckServiceWrapper = papa.PreconDeckManager;
		_settingsMenuHost = papa.SettingsMenuHost;
		_customTokenProvider = customTokenProvider;
		_voucherDataProvider = voucherDataProvider;
		_sceneUITransforms = Pantry.Get<SceneUITransforms>();
		_setMetadataProvider = Pantry.Get<ISetMetadataProvider>();
		_titleCountManager = Pantry.Get<ITitleCountManager>();
		_wrapperCompass = Pantry.Get<WrapperCompass>();
		_wrapperSceneManager = Pantry.Get<WrapperSceneManagement>();
		if (_logger == null)
		{
			_logger = new UnityLogger("SceneLoader", LoggerLevel.Error);
			LoggerManager.Register(_logger);
		}
		_popupManager = Pantry.Get<PopupManager>();
		InitNavContentData();
		GetNavBar();
		GetCardZoomView();
		GetObjectivesController();
		GetRewardsContentController();
		GetAchievementToastsController();
	}

	private void OnDestroy()
	{
		this.SceneLoaded = null;
		this.PlayBladeQueueSelected = null;
		Pantry.ResetScope(Pantry.Scope.Wrapper);
		SetSocialClient(null, null);
		_sceneUITransforms = null;
	}

	public IEnumerator Coroutine_TransitionOut()
	{
		NavContentController content = GetContent(CurrentContentType);
		yield return Coroutine_LeaveScreen(content, unloadAllWrapperScenes: true);
		EnableMainNavCamera(enable: false);
		GetNavBar()?.UpdateCurrentScreenIndicator(null);
		if ((bool)_navBarInstance)
		{
			UnityEngine.Object.Destroy(_navBarInstance.gameObject);
			_navBarInstance = null;
		}
		if ((bool)_objectivesContentControllerInstance)
		{
			UnityEngine.Object.Destroy(_objectivesContentControllerInstance.gameObject);
			_objectivesContentControllerInstance = null;
		}
		if ((bool)_playerInboxContentControllerInstance)
		{
			UnityEngine.Object.Destroy(_playerInboxContentControllerInstance.gameObject);
			_playerInboxContentControllerInstance = null;
		}
		if (_cardZoomViewInstance != null)
		{
			_cardZoomViewInstance.Destroy();
			_cardZoomViewInstance = null;
		}
		foreach (var (_, popupBase2) in _popupDictionary)
		{
			if ((bool)popupBase2)
			{
				UnityEngine.Object.Destroy(popupBase2.gameObject);
			}
		}
		_popupDictionary.Clear();
		_navContentLoader?.CleanupContent();
		BoosterPayloadUtilities.ClearUnusedMaterials();
		_transitionCanvasGroup.blocksRaycasts = false;
		_transitionCanvasGroup.interactable = false;
		_transitionCanvasGroup.alpha = 0f;
		CurrentNavContent = null;
		_contentPending = NavContentType.None;
		_isLoading = false;
		_loadingCount = 0;
		yield return Resources.UnloadUnusedAssets();
		GC.Collect();
		yield return new WaitForEndOfFrame();
	}

	private IEnumerator Coroutine_LeaveHome()
	{
		NavContentController homeController = null;
		float endTime = Time.realtimeSinceStartup + 30f;
		while (homeController != null || endTime >= Time.realtimeSinceStartup)
		{
			homeController = GetContent(NavContentType.Home);
			if (homeController != null)
			{
				break;
			}
			yield return null;
		}
		if (homeController != null)
		{
			yield return Coroutine_LeaveScreen(homeController);
		}
	}

	public IEnumerator Coroutine_LeaveScreen(NavContentController oldContent, bool unloadAllWrapperScenes = false)
	{
		EnableUniqueLoadingIndicator("SceneLoader.LeaveEnter");
		yield return Coroutine_ShowTransitionBlocker();
		if (oldContent != null)
		{
			Stopwatch watch = StartWatch();
			oldContent.BeginClose();
			yield return null;
			oldContent.FinishClose();
			oldContent.gameObject.UpdateActive(active: false);
			LogWatch(watch, "leave", oldContent.name);
			_navContentLoader.UnloadNavContent(oldContent.NavContentType, oldContent.NavContentType);
		}
		if (unloadAllWrapperScenes)
		{
			yield return _wrapperSceneManager.UnloadUnnecessaryScenes(WrapperSceneLifeCycle.IndividualPage);
		}
	}

	public IEnumerator Coroutine_EnterScreen(NavContentController newContent)
	{
		if (newContent == null)
		{
			yield break;
		}
		GetNavBar().UpdateCurrentScreenIndicator(newContent);
		GetNavBar().RefreshLocks(_frontDoorWrapper?.Killswitch);
		EnableMainNavCamera(enable: true);
		Stopwatch watch = StartWatch();
		newContent.gameObject.UpdateActive(active: true);
		newContent.BeginOpen();
		yield return new WaitUntil(() => newContent.IsReadyToShow || newContent.SkipScreen);
		if (!_wrapperSceneManager.AllScenesReady)
		{
			bool ready = false;
			_wrapperSceneManager.OnAllScenesReady += delegate
			{
				ready = true;
			};
			while (!ready)
			{
				yield return null;
			}
		}
		if (!newContent.SkipScreen)
		{
			newContent.FinishOpen();
			LogWatch(watch, "enter", newContent.name);
			yield return null;
			DisableUniqueLoadingIndicator("SceneLoader.LeaveEnter", warnOnEmpty: false);
			this.SceneLoaded?.Invoke();
			yield return Coroutine_HideTransitionBlocker();
		}
		else
		{
			newContent.Skipped();
		}
	}

	public void EnableMainNavCamera(bool enable)
	{
		_mainCamera.gameObject.SetActive(enable);
	}

	public string GetCurrentSceneName()
	{
		if (!IsInDuelScene)
		{
			return CurrentContentType.ToString();
		}
		return "DuelScene";
	}

	private void LoadContentInternal(NavContentType contentType, SceneChangeInitiator initiator, string context, bool reloadIfAlreadyLoaded = false, Action initAction = null, bool alwaysInit = false)
	{
		bool flag = CurrentContentType == contentType;
		bool flag2 = _contentPending != NavContentType.None && _contentPending == contentType;
		if (!reloadIfAlreadyLoaded && (flag || flag2))
		{
			if (alwaysInit)
			{
				initAction?.Invoke();
			}
			return;
		}
		_contentPending = contentType;
		string currentSceneName = GetCurrentSceneName();
		JObject jObject = new JObject();
		jObject["fromSceneName"] = currentSceneName;
		jObject["toSceneName"] = contentType.ToString();
		jObject["initiator"] = initiator.ToString();
		jObject["context"] = context;
		_unityCrossThreadLogger.Info("Client.SceneChange", jObject);
		StartCoroutine(Coroutine_TransitionToScreen(contentType, initAction, currentSceneName, initiator, context));
		if ((bool)AudioManager.Instance)
		{
			AudioManager.Instance.interacted = true;
			AudioManager.PlayMusic(contentType.ToString());
			AudioManager.PlayAmbiance(contentType.ToString());
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_page_state_change, AudioManager.Instance.gameObject);
		}
	}

	private IEnumerator Coroutine_TransitionToScreen(NavContentType contentType, Action initAction, string fromScene, SceneChangeInitiator initiator, string context)
	{
		_loadingCount++;
		if (_isLoading)
		{
			yield return new WaitUntil(() => !_isLoading);
		}
		_isLoading = true;
		DateTime startTime = DateTime.Now;
		TimeSpan timeSpentOnFromScene = startTime - _lastSceneChangeTime;
		_lastSceneChangeTime = startTime;
		ActivatingContent = CurrentContentType;
		_contentPending = NavContentType.None;
		NavContentController currentNavContent = CurrentNavContent;
		yield return Coroutine_LeaveScreen(currentNavContent);
		initAction?.Invoke();
		yield return new WaitUntil(delegate
		{
			NavContentController navContentController = (CurrentNavContent = GetContent(contentType));
			return navContentController != null;
		});
		yield return Coroutine_EnterScreen(CurrentNavContent);
		TimeSpan transitionTimeSeconds = DateTime.Now - startTime;
		BI_SceneChange(fromScene, contentType, initiator, context, timeSpentOnFromScene, transitionTimeSeconds);
		BacktraceIntegration.AddSceneBreadcrumb(contentType.ToString());
		_isLoading = false;
		_loadingCount--;
	}

	private void BI_SceneChange(string fromScene, NavContentType contentType, SceneChangeInitiator initiator, string context, TimeSpan timeSpentOnFromScene, TimeSpan transitionTimeSeconds)
	{
		SceneChange payload = new SceneChange
		{
			EventTime = DateTime.UtcNow,
			fromSceneName = fromScene,
			toSceneName = contentType.ToString(),
			initiator = initiator.ToString(),
			context = context,
			duration = timeSpentOnFromScene,
			transitionTimeSeconds = transitionTimeSeconds
		};
		_biLogger.Send(ClientBusinessEventType.SceneChange, payload);
	}

	public void ResetTransitionBlocker()
	{
		_transitionCanvasGroup.blocksRaycasts = true;
		_transitionCanvasGroup.interactable = true;
		_transitionCanvasGroup.alpha = 1f;
	}

	public IEnumerator Coroutine_ShowTransitionBlocker()
	{
		_transitionCanvasGroup.blocksRaycasts = true;
		_transitionCanvasGroup.interactable = true;
		yield return _transitionCanvasGroup.DOFade(1f, _transitionFadeDuration).SetEase(_transitionFadeEaseMethod).WaitForCompletion();
	}

	public IEnumerator Coroutine_HideTransitionBlocker()
	{
		yield return _transitionCanvasGroup.DOFade(0f, _transitionFadeDuration).SetEase(_transitionFadeEaseMethod).WaitForCompletion();
		_transitionCanvasGroup.blocksRaycasts = false;
		_transitionCanvasGroup.interactable = false;
	}

	private static Stopwatch StartWatch()
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		return stopwatch;
	}

	private static void LogWatch(Stopwatch watch, params string[] messages)
	{
		watch.Stop();
		_ = watch.Elapsed.TotalMilliseconds;
	}

	private HomePageContentController InitHome()
	{
		_wrapperSceneManager.LoadScene(new WrapperSceneInformation("HomePage", WrapperSceneLifeCycle.IndividualPage));
		return UnityUtilities.FindObjectOfType<HomePageContentController>(includeInactive: true);
	}

	public void OnPlayBladeQueueSelected(BladeEventInfo selectedBladeQueueInfo)
	{
		this.PlayBladeQueueSelected?.Invoke(selectedBladeQueueInfo);
	}

	public void OnPlayBladeFilterSelected(BladeEventFilter selectedBladeFilterInfo)
	{
		this.PlayBladeFilterSelected?.Invoke(selectedBladeFilterInfo);
	}

	private ProfileContentController InitProfile()
	{
		_wrapperSceneManager.LoadScene(new WrapperSceneInformation("Profile", WrapperSceneLifeCycle.IndividualPage));
		SceneManager.sceneLoaded += ProfileSceneFinishedLoading;
		return UnityEngine.Object.FindObjectOfType<ProfileContentController>();
	}

	private void ProfileSceneFinishedLoading(Scene sceneLoaded, LoadSceneMode loadingMode)
	{
		if (!(sceneLoaded.name != "Profile"))
		{
			SceneManager.sceneLoaded -= ProfileSceneFinishedLoading;
			ProfileContentController controller = UnityEngine.Object.FindObjectOfType<ProfileContentController>();
			_navContentLoader.SetController(NavContentType.Profile, controller);
		}
	}

	private WrapperDeckBuilder InitDeckBuilder()
	{
		bool num = CurrentContentType == NavContentType.DeckListViewer;
		WrapperSceneLifeCycle sceneLifeCycle = ((!num) ? WrapperSceneLifeCycle.IndividualPage : WrapperSceneLifeCycle.SubPage);
		if (num)
		{
			DeckManagerController deckManager = GetDeckManager();
			if (deckManager != null)
			{
				deckManager.gameObject.SetActive(value: false);
			}
		}
		_wrapperSceneManager.LoadScene(new WrapperSceneInformation("DeckBuilder", sceneLifeCycle));
		return UnityEngine.Object.FindObjectOfType<WrapperDeckBuilder>();
	}

	private DraftContentController InitDraft()
	{
		Stopwatch watch = StartWatch();
		string prefabPath = _assetLookupSystem.GetPrefabPath<DraftPrefab, DraftContentController>();
		LogWatch(watch, "load", prefabPath);
		Stopwatch watch2 = StartWatch();
		DraftContentController draftContentController = AssetLoader.Instantiate<DraftContentController>(prefabPath, _contentParent);
		draftContentController.Init(this, _mainCamera, GetCardZoomView(), _popupsParent, WrapperController.Instance.CardDatabase, WrapperController.Instance.AccountClient, WrapperController.Instance.FormatManager, WrapperController.Instance.InventoryManager, Pantry.Get<ITitleCountManager>(), WrapperController.Instance.AssetLookupSystem, _actionSystem, _cosmetics, Pantry.Get<IPreferredPrintingDataProvider>(), WrapperController.Instance.CardViewBuilder, WrapperController.Instance.Store, WrapperController.Instance.Store.CardbackCatalog, WrapperController.Instance.Store.PetCatalog, WrapperController.Instance.Store.AvatarCatalog, WrapperController.Instance.DecksManager, _biLogger, WrapperController.Instance.EmoteDataProvider, _locManager, WrapperController.Instance.UnityObjectPool);
		draftContentController.gameObject.UpdateActive(active: false);
		LogWatch(watch2, "instantiate", prefabPath);
		return draftContentController;
	}

	private TableDraftQueueContentController InitTableDraftQueue()
	{
		Stopwatch watch = StartWatch();
		string prefabPath = _assetLookupSystem.GetPrefabPath<TableDraftPrefab, TableDraftQueueContentController>();
		LogWatch(watch, "load", prefabPath);
		Stopwatch watch2 = StartWatch();
		TableDraftQueueContentController tableDraftQueueContentController = AssetLoader.Instantiate<TableDraftQueueContentController>(prefabPath, _contentParent);
		tableDraftQueueContentController.gameObject.UpdateActive(active: false);
		tableDraftQueueContentController.Init(this, Pantry.Get<IEventsServiceWrapper>(), _cosmetics, WrapperController.Instance.AccountClient, WrapperController.Instance.NavBarController, _assetLookupSystem, _settingsMenuHost);
		LogWatch(watch2, "instantiate", prefabPath);
		return tableDraftQueueContentController;
	}

	private ContentController_StoreCarousel InitStore()
	{
		_wrapperSceneManager.LoadScene(new WrapperSceneInformation("Store", WrapperSceneLifeCycle.IndividualPage));
		return UnityEngine.Object.FindObjectOfType<ContentController_StoreCarousel>();
	}

	private RewardTreeController InitRewardTree()
	{
		Stopwatch watch = StartWatch();
		string prefabPath = _assetLookupSystem.GetPrefabPath<RewardTreePrefab, RewardTreeController>();
		LogWatch(watch, "load", prefabPath);
		Stopwatch watch2 = StartWatch();
		RewardTreeController rewardTreeController = AssetLoader.Instantiate<RewardTreeController>(prefabPath, _contentParent);
		rewardTreeController.Init(GetObjectivesController(), GetRewardsContentController(), GetCardZoomView(), _biLogger, _cardDatabase, _cardViewBuilder, _cardMaterialBuilder, _assetLookupSystem);
		rewardTreeController.gameObject.UpdateActive(active: false);
		LogWatch(watch2, "instantiate", prefabPath);
		return rewardTreeController;
	}

	private ProgressionTracksContentController InitRewardTrack()
	{
		_wrapperSceneManager.LoadScene(new WrapperSceneInformation("RewardTrack", WrapperSceneLifeCycle.IndividualPage));
		return UnityEngine.Object.FindObjectOfType<ProgressionTracksContentController>();
	}

	private BoosterChamberController InitBoosterChamber()
	{
		_wrapperSceneManager.LoadScene(new WrapperSceneInformation("BoosterChamber", WrapperSceneLifeCycle.IndividualPage));
		return UnityEngine.Object.FindObjectOfType<BoosterChamberController>();
	}

	private ConstructedDeckSelectController InitConstructedDeckSelect()
	{
		Stopwatch watch = StartWatch();
		string prefabPath = _assetLookupSystem.GetPrefabPath<ConstructedDeckSelectPrefab, ConstructedDeckSelectController>();
		LogWatch(watch, "load", prefabPath);
		Stopwatch watch2 = StartWatch();
		ConstructedDeckSelectController constructedDeckSelectController = AssetLoader.Instantiate<ConstructedDeckSelectController>(prefabPath, _contentParent);
		constructedDeckSelectController.Init(_assetLookupSystem, _keyboardManager, _actionSystem);
		constructedDeckSelectController.gameObject.UpdateActive(active: false);
		LogWatch(watch2, "instantiate", prefabPath);
		return constructedDeckSelectController;
	}

	private DeckManagerController InitDeckManager()
	{
		_wrapperSceneManager.LoadScene(new WrapperSceneInformation("Decks", WrapperSceneLifeCycle.IndividualPage));
		return UnityEngine.Object.FindObjectOfType<DeckManagerController>();
	}

	private EventPageContentController InitEventPage()
	{
		Stopwatch watch = StartWatch();
		string prefabPath = _assetLookupSystem.GetPrefabPath<EventPagePrefab, EventPageContentController>();
		LogWatch(watch, "load", prefabPath);
		Stopwatch watch2 = StartWatch();
		EventPageContentController eventPageContentController = AssetLoader.Instantiate<EventPageContentController>(prefabPath, _contentParent);
		IPreferredPrintingDataProvider preferredPrintingDataProvider = Pantry.Get<IPreferredPrintingDataProvider>();
		PVPChallengeController challengeController = Pantry.Get<PVPChallengeController>();
		WrapperController instance = WrapperController.Instance;
		SharedEventPageClasses sharedClasses = new SharedEventPageClasses(this, instance.AccountClient.AccountInformation, instance.Matchmaking, instance.EventManager, instance.InventoryManager, instance.DecksManager, instance.FormatManager, instance.Store, _socialManager, instance.PreconDeckManager, instance.CardDatabase, instance.CardViewBuilder, instance.CardMaterialBuilder, instance.Store.CardSkinCatalog, instance.UnityCrossThreadLogger, _assetLookupSystem, _biLogger, _cosmetics, preferredPrintingDataProvider, _locManager, _customTokenProvider, _keyboardManager, _actionSystem, instance.PrizeWallDataProvider, challengeController);
		eventPageContentController.Init(sharedClasses);
		eventPageContentController.gameObject.UpdateActive(active: false);
		LogWatch(watch2, "instantiate", prefabPath);
		return eventPageContentController;
	}

	private FactionalizedEventTemplate InitFactionalizedEventPage()
	{
		IPreferredPrintingDataProvider preferredPrintingDataProvider = Pantry.Get<IPreferredPrintingDataProvider>();
		PVPChallengeController challengeController = Pantry.Get<PVPChallengeController>();
		WrapperController instance = WrapperController.Instance;
		SharedEventPageClasses sharedClasses = new SharedEventPageClasses(this, instance.AccountClient.AccountInformation, instance.Matchmaking, instance.EventManager, instance.InventoryManager, instance.DecksManager, instance.FormatManager, instance.Store, _socialManager, instance.PreconDeckManager, instance.CardDatabase, instance.CardViewBuilder, instance.CardMaterialBuilder, instance.Store.CardSkinCatalog, instance.UnityCrossThreadLogger, _assetLookupSystem, _biLogger, _cosmetics, preferredPrintingDataProvider, _locManager, _customTokenProvider, _keyboardManager, _actionSystem, instance.PrizeWallDataProvider, challengeController);
		FactionalizedEventTemplate factionalizedEventTemplate = AssetLoader.Instantiate<FactionalizedEventTemplate>(_assetLookupSystem.GetPrefabPath<FactionalizedEventPagePrefab, FactionalizedEventTemplate>(), _contentParent);
		factionalizedEventTemplate.Init(_assetLookupSystem, sharedClasses);
		factionalizedEventTemplate.gameObject.UpdateActive(active: false);
		return factionalizedEventTemplate;
	}

	private CampaignGraphContentController InitChallengeEventPage()
	{
		Stopwatch watch = StartWatch();
		string prefabPath = _assetLookupSystem.GetPrefabPath<CampaignGraphPrefab, CampaignGraphContentController>();
		LogWatch(watch, "load", prefabPath);
		Stopwatch watch2 = StartWatch();
		CampaignGraphContentController campaignGraphContentController = AssetLoader.Instantiate<CampaignGraphContentController>(prefabPath, _contentParent);
		campaignGraphContentController.Init(GetObjectivesController(), GetRewardsContentController(), GetCardZoomView(), _assetLookupSystem, _keyboardManager, _actionSystem, _cosmetics, _cardDatabase, _cardViewBuilder);
		campaignGraphContentController.gameObject.UpdateActive(active: false);
		LogWatch(watch2, "instantiate", prefabPath);
		return campaignGraphContentController;
	}

	private SealedBoosterOpenController InitSealedOpen()
	{
		Stopwatch watch = StartWatch();
		string prefabPath = _assetLookupSystem.GetPrefabPath<SealedBoosterOpenV2Prefab, SealedBoosterOpenController>();
		LogWatch(watch, "load", prefabPath);
		Stopwatch watch2 = StartWatch();
		SealedBoosterOpenController sealedBoosterOpenController = AssetLoader.Instantiate<SealedBoosterOpenController>(prefabPath, _contentParent);
		sealedBoosterOpenController.Init(GetCardZoomView(), GetRewardsContentController(), _cardDatabase, _cardViewBuilder, _cosmetics, _assetLookupSystem, _setMetadataProvider, _invManager);
		sealedBoosterOpenController.gameObject.UpdateActive(active: false);
		LogWatch(watch2, "instantiate", prefabPath);
		return sealedBoosterOpenController;
	}

	private LearnToPlayControllerV2 InitLearnToPlay()
	{
		_wrapperSceneManager.LoadScene(new WrapperSceneInformation("LearnToPlay", WrapperSceneLifeCycle.IndividualPage));
		return UnityEngine.Object.FindObjectOfType<LearnToPlayControllerV2>();
	}

	public void InitSocial()
	{
		_wrapperSceneManager.LoadScene(new WrapperSceneInformation("Social", WrapperSceneLifeCycle.LoggedIn));
	}

	private AchievementsContentController InitAchievements()
	{
		_wrapperSceneManager.LoadScene(new WrapperSceneInformation("Achievements", WrapperSceneLifeCycle.IndividualPage));
		return UnityEngine.Object.FindObjectOfType<AchievementsContentController>();
	}

	private ContentController_PrizeWall InitPrizeWall()
	{
		Stopwatch watch = StartWatch();
		string prefabPath = _assetLookupSystem.GetPrefabPath<PrizeWallPrefab, ContentController_PrizeWall>();
		LogWatch(watch, "load", prefabPath);
		Stopwatch watch2 = StartWatch();
		ContentController_PrizeWall contentController_PrizeWall = AssetLoader.Instantiate<ContentController_PrizeWall>(prefabPath, _contentParent);
		contentController_PrizeWall.gameObject.UpdateActive(active: false);
		contentController_PrizeWall.Init(_assetLookupSystem, GetCardZoomView(), _biLogger, _greLocalizationManager, _locManager, _cardDatabase, _cardViewBuilder, _settingsMenuHost, _setMetadataProvider, _contentParent);
		LogWatch(watch2, "instantiate", prefabPath);
		return contentController_PrizeWall;
	}

	private PacketSelectContentController LoadPacketSelect()
	{
		Stopwatch watch = StartWatch();
		string prefabPath = _assetLookupSystem.GetPrefabPath<PacketSelectPrefab, PacketSelectContentController>();
		LogWatch(watch, "load", prefabPath);
		Stopwatch watch2 = StartWatch();
		PacketSelectContentController packetSelectContentController = AssetLoader.Instantiate<PacketSelectContentController>(prefabPath, _contentParent);
		packetSelectContentController.Init(_locManager, _cardDatabase.CardDataProvider, _cardViewBuilder, _invManager, new PacketArtProvider(_artTextureLoader, _cropDatabase), new PacketAudioProvider());
		packetSelectContentController.gameObject.UpdateActive(active: false);
		LogWatch(watch2, "instantiate", prefabPath);
		return packetSelectContentController;
	}

	private NavContentController GetContent(NavContentType type)
	{
		return _navContentLoader.GetController(type);
	}

	public StoreTabType GetStoreCurrentTab()
	{
		if (_navContentLoader.HasNavContentController(NavContentType.Store))
		{
			return _navContentLoader.GetController<ContentController_StoreCarousel>(NavContentType.Store).CurrentTab;
		}
		return StoreTabType.None;
	}

	public void SetSocialClient(ISocialManager socialManager, SocialUI socialUI)
	{
		_socialManager = socialManager;
		_socialUI = socialUI;
	}

	public void SetSocialVisible(bool visible)
	{
		if ((bool)_socialUI)
		{
			_socialUI.SetVisible(visible);
		}
	}

	public bool GetRewardTreeUpgradeDeckShown()
	{
		if (_navContentLoader.HasNavContentController(NavContentType.RewardTree))
		{
			RewardTreeController controller = _navContentLoader.GetController<RewardTreeController>(NavContentType.RewardTree);
			if (controller.DeckUpgrade != null)
			{
				return controller.DeckUpgrade.isActiveAndEnabled;
			}
		}
		return false;
	}

	public void SetRewardTreeAnimationPaused(bool paused)
	{
		if (_navContentLoader.HasNavContentController(NavContentType.RewardTree))
		{
			_navContentLoader.GetController<RewardTreeController>(NavContentType.RewardTree).PauseAnimation = paused;
		}
	}

	public bool GetBoosterOpenInChamber()
	{
		if (_navContentLoader.HasNavContentController(NavContentType.BoosterChamber))
		{
			return _navContentLoader.GetController<BoosterChamberController>(NavContentType.BoosterChamber).ThereIsABoosterOpened;
		}
		return false;
	}

	public DeckInfo GetSelectedDeckInfo()
	{
		if (_navContentLoader.HasNavContentController(NavContentType.DeckListViewer))
		{
			return _navContentLoader.GetController<DeckManagerController>(NavContentType.DeckListViewer).GetSelectedDeckInfo();
		}
		return null;
	}

	public DeckManagerController GetDeckManager()
	{
		if (!_navContentLoader.HasNavContentController(NavContentType.DeckListViewer))
		{
			return null;
		}
		return _navContentLoader.GetController<DeckManagerController>(NavContentType.DeckListViewer);
	}

	public WrapperDeckBuilder GetDeckBuilder()
	{
		if (!_navContentLoader.HasNavContentController(NavContentType.DeckBuilder))
		{
			return null;
		}
		return _navContentLoader.GetController<WrapperDeckBuilder>(NavContentType.DeckBuilder);
	}

	public bool GetHomeEventBladeShown()
	{
		if (_navContentLoader.HasNavContentController(NavContentType.Home))
		{
			return _navContentLoader.GetController<HomePageContentController>(NavContentType.Home).IsEventBladeActive;
		}
		return false;
	}

	public bool GetHomeDeckSelected()
	{
		if (_navContentLoader.HasNavContentController(NavContentType.Home))
		{
			return _navContentLoader.GetController<HomePageContentController>(NavContentType.Home).IsEventBladeDeckSelected();
		}
		return false;
	}

	public void ShowPlayBladeAndSelect(string eventPublicName)
	{
		if (CurrentContentType == NavContentType.Home)
		{
			_navContentLoader.GetController<HomePageContentController>(NavContentType.Home).ShowBladeAndSelect(eventPublicName);
		}
	}

	public void ShowPlayBladeEventsAndFilter(string dynamicFilter)
	{
		if (CurrentContentType == NavContentType.Home)
		{
			_navContentLoader.GetController<HomePageContentController>(NavContentType.Home).ShowPlayBladeEventsAndFilter(dynamicFilter);
		}
	}

	public void TestRewardTreeDeckUpgrade()
	{
		_navContentLoader.GetController<RewardTreeController>(NavContentType.RewardTree).TEST_DeckUpgrade();
	}

	public void ForceDisableLoadingIndicator()
	{
		_loadingIndicatorIds.Clear();
		LoadingIndicatorSetActive(shouldEnable: false);
	}

	public void EnableLoadingIndicator(bool shouldEnable)
	{
		if (shouldEnable)
		{
			EnableUniqueLoadingIndicator("SceneLoader.Default");
		}
		else
		{
			DisableUniqueLoadingIndicator("SceneLoader.Default");
		}
	}

	public void EnableUniqueLoadingIndicator(string id, bool warnOnDuplicate = true)
	{
		if (!_loadingIndicatorIds.Contains(id))
		{
			_loadingIndicatorIds.Add(id);
		}
		else if (warnOnDuplicate)
		{
			UnityEngine.Debug.LogWarning("Opening a loading indicator while it was already opened. Id: " + id);
		}
		LoadingIndicatorSetActive(shouldEnable: true);
	}

	public void DisableUniqueLoadingIndicator(string id, bool warnOnEmpty = true)
	{
		if (!_loadingIndicatorIds.Remove(id) && warnOnEmpty)
		{
			UnityEngine.Debug.LogWarning("Closing a loading indicator when it was never opened. Id: " + id);
		}
		if (_loadingIndicatorIds.Count == 0)
		{
			LoadingIndicatorSetActive(shouldEnable: false);
		}
	}

	private void LoadingIndicatorSetActive(bool shouldEnable)
	{
		if (_loadingInstance == null && shouldEnable)
		{
			string prefabPath = _assetLookupSystem.GetPrefabPath<LoadingPanelPrefab, GameObject>();
			_loadingInstance = AssetLoader.Instantiate(prefabPath, _overlayParent);
		}
		if (_loadingInstance != null)
		{
			_loadingInstance.UpdateActive(shouldEnable);
		}
	}

	public static void ApplicationQuit()
	{
		Application.Quit();
	}

	public static SceneLoader GetSceneLoader()
	{
		if (!(WrapperController.Instance == null))
		{
			return WrapperController.Instance.SceneLoader;
		}
		return null;
	}

	public void GoToEventScreen(EventContext evt, bool reloadIfAlreadyLoaded = false, NavMethod navMethod = NavMethod.Unknown)
	{
		if (navMethod != NavMethod.Unknown)
		{
			BI_HomeEventNavigation(evt.PlayerEvent.EventUXInfo.PublicEventName, navMethod);
		}
		evt.PlayerEvent.EventUXInfo.OpenedFromPlayBlade = navMethod == NavMethod.PlayButton;
		int num = evt.PlayerEvent.EventUXInfo.FactionSealedUXInfo?.Count ?? 0;
		if (evt.PlayerEvent.EventInfo.FormatType == MDNEFormatType.Sealed && num > 0)
		{
			Action initAction = delegate
			{
				_navContentLoader.GetController<FactionalizedEventTemplate>(NavContentType.FactionalizedEvent).SetEvent(evt);
			};
			LoadContentInternal(NavContentType.FactionalizedEvent, SceneChangeInitiator.User, evt.PlayerEvent.EventUXInfo.PublicEventName, reloadIfAlreadyLoaded, initAction);
		}
		else if (evt.PlayerEvent is IColorChallengePlayerEvent)
		{
			Action initAction2 = delegate
			{
				_navContentLoader.GetController<CampaignGraphContentController>(NavContentType.ChallengeEventLanding).RefreshEvent();
				SpawnNPEOnboarding();
			};
			LoadContentInternal(NavContentType.ChallengeEventLanding, SceneChangeInitiator.User, evt.PlayerEvent.EventUXInfo.PublicEventName, reloadIfAlreadyLoaded, initAction2);
		}
		else if (evt.PlayerEvent.EventInfo.InternalEventName == "DirectGame")
		{
			HomePageContext homePageContext = new HomePageContext();
			homePageContext.PostMatchContext = evt.PostMatchContext;
			GoToLanding(homePageContext);
		}
		else
		{
			Action initAction3 = delegate
			{
				_navContentLoader.GetController<EventPageContentController>(NavContentType.EventLanding).SetEvent(evt);
			};
			LoadContentInternal(NavContentType.EventLanding, SceneChangeInitiator.User, evt.PlayerEvent.EventUXInfo.PublicEventName, reloadIfAlreadyLoaded, initAction3);
		}
		if (navMethod == NavMethod.Unknown)
		{
			StartCoroutine(Coroutine_LeaveHome());
		}
	}

	public void BI_HomeEventNavigation(string publicEventName, NavMethod navMethod)
	{
		HomeEventNavigation payload = new HomeEventNavigation
		{
			EventTime = DateTime.UtcNow,
			PublicEventName = publicEventName,
			NavMethod = navMethod.ToString()
		};
		_biLogger.Send(ClientBusinessEventType.HomeEventNavigation, payload);
	}

	public void GoToProfileScreen(SceneChangeInitiator initiator, string context, ProfileScreenModeEnum screenMode = ProfileScreenModeEnum.Unknown, RankType rankType = RankType.Unknown, bool forceReload = false, bool alwaysInit = false)
	{
		if (screenMode != ProfileScreenModeEnum.Unknown || rankType != RankType.Unknown)
		{
			_wrapperCompass.SetGuide(new ProfileCompassGuide(screenMode, rankType));
		}
		Action initAction = delegate
		{
			_navContentLoader.GetController<ProfileContentController>(NavContentType.Profile);
		};
		LoadContentInternal(NavContentType.Profile, initiator, context, forceReload, initAction, alwaysInit);
	}

	public void GoToLearnToPlay(string context)
	{
		SpawnNPEOnboarding();
		LoadContentInternal(NavContentType.LearnToPlay, SceneChangeInitiator.User, context);
	}

	public void GoToCinematic(string cinematicSceneName, string videoUrl = "", string videoPlayLookupMode = "", string videoPlayAudioMode = "")
	{
		StartCoroutine(CinematicScripting.GoToCinematicCoroutine(WrapperController.Instance, cinematicSceneName, videoUrl, videoPlayLookupMode, videoPlayAudioMode));
	}

	public void GoToBoosterChamber(string context)
	{
		Action initAction = delegate
		{
			_navContentLoader.GetController(NavContentType.BoosterChamber);
		};
		LoadContentInternal(NavContentType.BoosterChamber, SceneChangeInitiator.User, context, reloadIfAlreadyLoaded: false, initAction);
	}

	public void GoToDraftScene(EventContext evt)
	{
		Action initAction = delegate
		{
			Pantry.Get<DeckBuilderContextProvider>().Context = DraftContentController.GenerateDraftDeckBuilderContext(Pantry.Get<FormatManager>(), evt, new Deck(Pantry.Get<CardDatabase>()));
			GetDraftContentController().SetDraftData(evt);
		};
		LimitedPlayerEvent limitedPlayerEvent = (LimitedPlayerEvent)evt.PlayerEvent;
		LoadContentInternal(NavContentType.Draft, SceneChangeInitiator.System, limitedPlayerEvent.DraftPod.DraftMode.ToString(), reloadIfAlreadyLoaded: false, initAction);
	}

	public void GoToPrizeWall(string prizeWallId, PrizeWallContext prizeWallContext)
	{
		Client_PrizeWall prizeWallById = Pantry.Get<PrizeWallDataProvider>().GetPrizeWallById(prizeWallId);
		if (prizeWallById == null)
		{
			_logger.LogError("Can't navigate to " + prizeWallId + ". Prize Wall doesn't exist or is disabled");
			return;
		}
		if (prizeWallById.AppearsAsStoreTab)
		{
			GoToStore(StoreTabType.PrizeWall, "Navigating to Prize Wall Tab");
			return;
		}
		Action initAction = delegate
		{
			_navContentLoader.GetController<ContentController_PrizeWall>(NavContentType.PrizeWall).SetPrizeWallData(prizeWallId, prizeWallContext);
		};
		LoadContentInternal(NavContentType.PrizeWall, SceneChangeInitiator.System, prizeWallId, reloadIfAlreadyLoaded: false, initAction);
	}

	public void GoToTableDraftQueueScene(EventContext evt)
	{
		Action initAction = delegate
		{
			_navContentLoader.GetController<TableDraftQueueContentController>(NavContentType.TableDraftQueue).SetEvent(evt);
		};
		LoadContentInternal(NavContentType.TableDraftQueue, SceneChangeInitiator.System, "Entering table draft queue: " + evt.PlayerEvent.EventUXInfo.PublicEventName, reloadIfAlreadyLoaded: false, initAction);
	}

	public void GoToPacketSelect(EventContext evt)
	{
		if (evt == null)
		{
			return;
		}
		Action initAction = delegate
		{
			PacketSelectContentController controller = _navContentLoader.GetController<PacketSelectContentController>(NavContentType.PacketSelect);
			if (controller != null)
			{
				controller.Init(_locManager, _cardDatabase.CardDataProvider, _cardViewBuilder, _invManager, new PacketArtProvider(_artTextureLoader, _cropDatabase), new PacketAudioProvider());
				ServiceInterface serviceInterface = new ServiceInterface(evt, this, _invManager, _frontDoorWrapper);
				controller.SetInterface(serviceInterface);
			}
		};
		LoadContentInternal(NavContentType.PacketSelect, SceneChangeInitiator.System, "Load Packet Selection UI", reloadIfAlreadyLoaded: false, initAction);
	}

	public DraftContentController GetDraftContentController()
	{
		return _navContentLoader.GetController<DraftContentController>(NavContentType.Draft);
	}

	public void GoToStore(StoreTabType context, string navContext, bool forceReload = false, bool alwaysInit = false)
	{
		_wrapperCompass.SetGuide(new StoreScreenWrapperCompassGuide(context));
		Action initAction = delegate
		{
			_navContentLoader.GetController<ContentController_StoreCarousel>(NavContentType.Store);
		};
		LoadContentInternal(NavContentType.Store, SceneChangeInitiator.System, navContext, forceReload, initAction, alwaysInit);
	}

	public void GoToStoreItem(string itemId, StoreTabType fallbackContext, string navContext, bool forceReload = false, bool alwaysInit = false)
	{
		_wrapperCompass.SetGuide(new StoreScreenWrapperCompassGuide(itemId, fallbackContext));
		Action initAction = delegate
		{
			_navContentLoader.GetController<ContentController_StoreCarousel>(NavContentType.Store);
		};
		LoadContentInternal(NavContentType.Store, SceneChangeInitiator.System, navContext, forceReload, initAction, alwaysInit);
	}

	public void GoToStoreSetPacks(string expansionCode, string navContext, bool forceReload = false, bool alwaysInit = false)
	{
		if (expansionCode != null)
		{
			_wrapperCompass.SetGuide(new StoreScreenWrapperCompassGuide(expansionCode));
		}
		Action initAction = delegate
		{
			_navContentLoader.GetController<ContentController_StoreCarousel>(NavContentType.Store);
		};
		LoadContentInternal(NavContentType.Store, SceneChangeInitiator.System, navContext, forceReload, initAction, alwaysInit);
	}

	public void GoToDeckManager()
	{
		LoadContentInternal(NavContentType.DeckListViewer, SceneChangeInitiator.System, "Navigate to Deck Manager");
	}

	public void GoToConstructedDeckSelect(DeckSelectContext context)
	{
		Action initAction = delegate
		{
			_navContentLoader.GetController<ConstructedDeckSelectController>(NavContentType.ConstructedDeckSelect).SetDeckSelectContext(context);
		};
		LoadContentInternal(NavContentType.ConstructedDeckSelect, SceneChangeInitiator.System, "Navigate to Constructed Deck Select", reloadIfAlreadyLoaded: false, initAction);
	}

	public void GoToDeckBuilder(DeckBuilderContext context, bool reloadIfAlreadyLoaded = false)
	{
		Action initAction = delegate
		{
			Pantry.Get<DeckBuilderContextProvider>().Context = context;
			_navContentLoader.GetController<WrapperDeckBuilder>(NavContentType.DeckBuilder);
		};
		LoadContentInternal(NavContentType.DeckBuilder, SceneChangeInitiator.System, "deck builder", reloadIfAlreadyLoaded, initAction);
	}

	public void GoToProgressionTrackScene(ProgressionTrackPageContext trackPageContext, string callerContext, bool forceReload = true, bool alwaysInit = false)
	{
		if (!_masteryPassProvider.FailedInitializing)
		{
			if (trackPageContext.TrackName == null)
			{
				trackPageContext.TrackName = _masteryPassProvider.CurrentBpName ?? _masteryPassProvider.PreviousBpName;
			}
			_wrapperCompass.SetGuide(new RewardTrackScreenWrapperCompassGuide(trackPageContext));
			Action initAction = delegate
			{
				_navContentLoader.GetController<ProgressionTracksContentController>(NavContentType.RewardTrack);
			};
			LoadContentInternal(NavContentType.RewardTrack, SceneChangeInitiator.User, callerContext, forceReload, initAction, alwaysInit);
		}
	}

	public void GoToRewardTreeScene(RewardTreePageContext rewardTreePageContext)
	{
		if (_masteryPassProvider.IsEnabled(rewardTreePageContext.TrackName))
		{
			if (rewardTreePageContext.PostMatchContext != null)
			{
				LoadContentInternal(NavContentType.RewardTree, SceneChangeInitiator.System, "Heading to WebScene from Duelscene", reloadIfAlreadyLoaded: false, InitAction);
			}
			else
			{
				LoadContentInternal(NavContentType.RewardTree, SceneChangeInitiator.User, "Opening Web from Track Scene", reloadIfAlreadyLoaded: false, InitAction);
			}
		}
		void InitAction()
		{
			_navContentLoader.GetController<RewardTreeController>(NavContentType.RewardTree).SetContext(rewardTreePageContext);
		}
	}

	public void GoToAchievementsScene(string context, IClientAchievement achievement = null)
	{
		if (achievement != null)
		{
			_wrapperCompass.SetGuide(new AchievementsScreenWrapperCompassGuide(achievement));
		}
		LoadContentInternal(NavContentType.Achievements, SceneChangeInitiator.User, context);
	}

	public AchievementsContentController GetAchievementsNavController()
	{
		return InitAchievements();
	}

	public void SpawnNPEOnboarding()
	{
		if (_sparkyMetaOnboarding == null)
		{
			_assetLookupSystem.Blackboard.Clear();
			SparkyMetaAltPrefab payload = _assetLookupSystem.TreeLoader.LoadTree<SparkyMetaAltPrefab>(returnNewTree: false).GetPayload(_assetLookupSystem.Blackboard);
			_sparkyMetaOnboarding = AssetLoader.Instantiate(payload.Prefab).gameObject;
			_assetLookupSystem.Blackboard.Clear();
		}
	}

	public void Cleanup()
	{
		DestroyOnboarding();
	}

	private void DestroyOnboarding()
	{
		if (_sparkyMetaOnboarding != null)
		{
			UnityEngine.Object.Destroy(_sparkyMetaOnboarding);
			_sparkyMetaOnboarding = null;
		}
	}

	public void GoToLanding(HomePageContext context, bool forceReload = false)
	{
		if (_landingSequence == null)
		{
			_landingSequence = StartCoroutine(LandingSequence(context, forceReload));
		}
	}

	private IEnumerator LandingSequence(HomePageContext context, bool forceReload = false)
	{
		RemoteSettings.ForceUpdate();
		Pantry.Get<TooltipSystem>();
		if (!_motdOncePerSession || !_motdSession.Requested)
		{
			SystemMessageDataProvider systemMessageDataProvider = Pantry.Get<SystemMessageDataProvider>();
			yield return new WaitUntil(() => systemMessageDataProvider.Initialized);
			MessageOfTheDay motd = systemMessageDataProvider.MessageOfTheDay;
			if (motd != null && !string.IsNullOrEmpty(motd.Title) && _motdSession.LastMessage != motd.Message)
			{
				_motdSession.LastMessage = motd.Message;
				LoadContentInternal(NavContentType.None, SceneChangeInitiator.System, "Emergency Broadcast", forceReload);
				yield return new WaitUntil(() => !_isLoading);
				MOTDPopup motdPopup = GetMOTDPopup();
				motdPopup.Activate(motd.Title, motd.Message);
				yield return new WaitUntil(() => !motdPopup.IsShowing);
			}
		}
		if (string.IsNullOrWhiteSpace(_cosmetics.PlayerAvatarSelection))
		{
			_cosmetics.SetAvatarSelection("Avatar_Basic_AjaniGoldmane");
		}
		_wrapperCompass.SetGuide(new HomePageCompassGuide(context));
		Action initAction = delegate
		{
			HomePageContentController controller = _navContentLoader.GetController<HomePageContentController>(NavContentType.Home);
			if (controller != null)
			{
				HomePageCompassGuide guide = _wrapperCompass.GetGuide<HomePageCompassGuide>();
				if (guide != null)
				{
					controller?.SetContext(guide);
				}
			}
		};
		LoadContentInternal(NavContentType.Home, SceneChangeInitiator.System, "Landing Page 'Home'", forceReload, initAction);
		Scene sceneByName = SceneManager.GetSceneByName("AssetPrep");
		if (sceneByName.isLoaded)
		{
			yield return SceneManager.UnloadSceneAsync(sceneByName);
		}
		_landingSequence = null;
	}

	public void GoToSealedBoosterOpen(DeckBuilderContext context, int gemsAdded)
	{
		Action initAction = delegate
		{
			_navContentLoader.GetController<SealedBoosterOpenController>(NavContentType.SealedBoosterOpen).SetContext(context, gemsAdded);
		};
		LoadContentInternal(NavContentType.SealedBoosterOpen, SceneChangeInitiator.System, "Opening sealed boosters after event join", reloadIfAlreadyLoaded: false, initAction);
	}

	public BattlePassPurchaseConfirmation GetBattlePassPurchaseConfirmation()
	{
		Type typeFromHandle = typeof(BattlePassPurchaseConfirmation);
		if (!_popupDictionary.ContainsKey(typeFromHandle))
		{
			Stopwatch watch = StartWatch();
			string prefabPath = _assetLookupSystem.GetPrefabPath<BattlePassPurchasePrefab, BattlePassPurchaseConfirmation>();
			LogWatch(watch, "load", prefabPath);
			Stopwatch watch2 = StartWatch();
			BattlePassPurchaseConfirmation battlePassPurchaseConfirmation = AssetLoader.Instantiate<BattlePassPurchaseConfirmation>(prefabPath, _popupsParent);
			battlePassPurchaseConfirmation.Init(_biLogger, _cardDatabase, _cardMaterialBuilder, _assetLookupSystem);
			LogWatch(watch2, "instantiate", prefabPath);
			_popupDictionary.Add(typeFromHandle, battlePassPurchaseConfirmation);
		}
		return (BattlePassPurchaseConfirmation)_popupDictionary[typeFromHandle];
	}

	public PetPopUp GetPetPopup()
	{
		Type typeFromHandle = typeof(PetPopUp);
		if (!_popupDictionary.ContainsKey(typeFromHandle))
		{
			Stopwatch watch = StartWatch();
			string prefabPath = _assetLookupSystem.GetPrefabPath<PetPopupPrefab, PetPopUp>();
			LogWatch(watch, "load", prefabPath);
			Stopwatch watch2 = StartWatch();
			PetPopUp value = AssetLoader.Instantiate<PetPopUp>(prefabPath, _popupsParent);
			LogWatch(watch2, "instantiate", prefabPath);
			_popupDictionary.Add(typeFromHandle, value);
		}
		return (PetPopUp)_popupDictionary[typeFromHandle];
	}

	private MOTDPopup GetMOTDPopup()
	{
		Type typeFromHandle = typeof(MOTDPopup);
		if (!_popupDictionary.ContainsKey(typeFromHandle))
		{
			Stopwatch watch = StartWatch();
			string prefabPath = _assetLookupSystem.GetPrefabPath<MOTDPrefab, MOTDPopup>();
			LogWatch(watch, "load", prefabPath);
			Stopwatch watch2 = StartWatch();
			MOTDPopup value = AssetLoader.Instantiate<MOTDPopup>(prefabPath, _overlayParent);
			LogWatch(watch2, "instantiate", prefabPath);
			_popupDictionary.Add(typeFromHandle, value);
		}
		return (MOTDPopup)_popupDictionary[typeFromHandle];
	}

	public ContentControllerRewards GetRewardsContentController()
	{
		if (_rewardsContentControllerInstance == null)
		{
			Stopwatch watch = StartWatch();
			string prefabPath = _assetLookupSystem.GetPrefabPath<RewardsPrefab, ContentControllerRewards>();
			LogWatch(watch, "load", prefabPath);
			Stopwatch watch2 = StartWatch();
			_rewardsContentControllerInstance = AssetLoader.Instantiate<ContentControllerRewards>(prefabPath, _popupsParent);
			_rewardsContentControllerInstance.Init(_masteryPassProvider, _assetLookupSystem, GetCardZoomView(), _keyboardManager, _actionSystem, _cardDatabase, _cardViewBuilder);
			LogWatch(watch2, "instantiate", prefabPath);
		}
		return _rewardsContentControllerInstance;
	}

	public ContentControllerPlayerInbox GetPlayerInboxContentController()
	{
		if (_playerInboxContentControllerInstance == null)
		{
			Stopwatch watch = StartWatch();
			string prefabPath = _assetLookupSystem.GetPrefabPath<PlayerInboxContentControllerPrefab, ContentControllerPlayerInbox>();
			LogWatch(watch, "load", prefabPath);
			Stopwatch watch2 = StartWatch();
			_playerInboxContentControllerInstance = AssetLoader.Instantiate<ContentControllerPlayerInbox>(prefabPath, _popupsParent);
			_playerInboxContentControllerInstance.Init(Pantry.Get<PlayerInboxDataProvider>(), _artTextureLoader, _assetLookupSystem, BILogger);
			LogWatch(watch2, "instantiate", prefabPath);
		}
		return _playerInboxContentControllerInstance;
	}

	public ContentControllerObjectives GetObjectivesController()
	{
		return UnityUtilities.FindObjectOfType<ContentControllerObjectives>(includeInactive: true);
	}

	private void GetAchievementToastsController()
	{
		Stopwatch watch = StartWatch();
		string prefabPath = _assetLookupSystem.GetPrefabPath<AchievementsToastManagerPrefab, AchievementsToastManager>();
		LogWatch(watch, "load", prefabPath);
		Stopwatch watch2 = StartWatch();
		AssetLoader.Instantiate<AchievementsToastManager>(prefabPath, _overlayParent);
		LogWatch(watch2, "instantiate", prefabPath);
	}

	public EventPageContentController TryGetEventPageContentController()
	{
		if (!_navContentLoader.HasNavContentController(NavContentType.EventLanding))
		{
			return null;
		}
		return _navContentLoader.GetController<EventPageContentController>(NavContentType.EventLanding);
	}

	public bool HasSeenNewSetAnnouncement()
	{
		return SetAnnouncementController.HasSeenNewSet(GetSetAnnouncementController().NewSetId);
	}

	public SetAnnouncementController GetSetAnnouncementController()
	{
		Type typeFromHandle = typeof(SetAnnouncementController);
		if (!_popupDictionary.ContainsKey(typeFromHandle))
		{
			Stopwatch watch = StartWatch();
			string prefabPath = _assetLookupSystem.GetPrefabPath<SetAnnouncementPrefab, SetAnnouncementController>();
			LogWatch(watch, "load", prefabPath);
			Stopwatch watch2 = StartWatch();
			SetAnnouncementController setAnnouncementController = AssetLoader.Instantiate<SetAnnouncementController>(prefabPath, _popupsParent);
			setAnnouncementController.gameObject.SetActive(value: true);
			setAnnouncementController.gameObject.SetActive(value: false);
			LogWatch(watch2, "instantiate", prefabPath);
			_popupDictionary.Add(typeFromHandle, setAnnouncementController);
		}
		return (SetAnnouncementController)_popupDictionary[typeFromHandle];
	}

	public RuleChangePopup GetRuleChangePopUp()
	{
		Type typeFromHandle = typeof(RuleChangePopup);
		if (!_popupDictionary.ContainsKey(typeFromHandle))
		{
			Stopwatch watch = StartWatch();
			string prefabPath = _assetLookupSystem.GetPrefabPath<RuleChangeSplashPrefab, RuleChangePopup>();
			LogWatch(watch, "load", prefabPath);
			Stopwatch watch2 = StartWatch();
			RuleChangePopup value = AssetLoader.Instantiate<RuleChangePopup>(prefabPath, _popupsParent);
			LogWatch(watch2, "instantiate", prefabPath);
			_popupDictionary.Add(typeFromHandle, value);
		}
		return (RuleChangePopup)_popupDictionary[typeFromHandle];
	}

	public bool HasSeenMOZTutorialPopup()
	{
		return MOZTutorialPopup.HasSeenMOZTutorial();
	}

	public MOZTutorialPopup GetMOZTutorialPopup()
	{
		Type typeFromHandle = typeof(MOZTutorialPopup);
		if (!_popupDictionary.ContainsKey(typeFromHandle))
		{
			Stopwatch watch = StartWatch();
			string prefabPath = _assetLookupSystem.GetPrefabPath<MOZTutorialPopupPrefab, MOZTutorialPopup>();
			LogWatch(watch, "load", prefabPath);
			if (prefabPath == null)
			{
				_unityCrossThreadLogger.Debug("No MOZ Tutorial Popup found in the ALT for the current Mode (this is a Handheld only prefab)");
				return null;
			}
			Stopwatch watch2 = StartWatch();
			MOZTutorialPopup value = AssetLoader.Instantiate<MOZTutorialPopup>(prefabPath, _popupsParent);
			LogWatch(watch2, "instantiate", prefabPath);
			_popupDictionary.Add(typeFromHandle, value);
		}
		return (MOZTutorialPopup)_popupDictionary[typeFromHandle];
	}

	public BannedCardPopup GetBannedCardPopup()
	{
		Type typeFromHandle = typeof(BannedCardPopup);
		if (!_popupDictionary.ContainsKey(typeFromHandle))
		{
			Stopwatch watch = StartWatch();
			string prefabPath = _assetLookupSystem.GetPrefabPath<BannedCardPopupPrefab, BannedCardPopup>();
			LogWatch(watch, "load", prefabPath);
			Stopwatch watch2 = StartWatch();
			BannedCardPopup value = AssetLoader.Instantiate<BannedCardPopup>(prefabPath, _popupsParent);
			LogWatch(watch2, "instantiate", prefabPath);
			_popupDictionary.Add(typeFromHandle, value);
		}
		return (BannedCardPopup)_popupDictionary[typeFromHandle];
	}

	public TablesPopupUI GetTablesPopup(TablesPopupUI prefab)
	{
		Type typeFromHandle = typeof(TablesPopupUI);
		if (!_popupDictionary.ContainsKey(typeFromHandle))
		{
			TablesPopupUI value = UnityEngine.Object.Instantiate(prefab, _popupsParent);
			_popupDictionary.Add(typeFromHandle, value);
		}
		return (TablesPopupUI)_popupDictionary[typeFromHandle];
	}

	public RenewalPopup GetRenewalPopup()
	{
		Type typeFromHandle = typeof(RenewalPopup);
		if (!_popupDictionary.ContainsKey(typeFromHandle))
		{
			Stopwatch watch = StartWatch();
			string prefabPath = _assetLookupSystem.GetPrefabPath<RenewalPopupPrefab, RenewalPopup>();
			LogWatch(watch, "load", prefabPath);
			Stopwatch watch2 = StartWatch();
			RenewalPopup value = AssetLoader.Instantiate<RenewalPopup>(prefabPath, _popupsParent);
			LogWatch(watch2, "instantiate", prefabPath);
			_popupDictionary.Add(typeFromHandle, value);
		}
		return (RenewalPopup)_popupDictionary[typeFromHandle];
	}

	public RotationPopup GetRotationPopup()
	{
		Type typeFromHandle = typeof(RotationPopup);
		if (!_popupDictionary.ContainsKey(typeFromHandle))
		{
			Stopwatch watch = StartWatch();
			string prefabPath = _assetLookupSystem.GetPrefabPath<RotationPopupPrefab, RotationPopup>();
			LogWatch(watch, "load", prefabPath);
			Stopwatch watch2 = StartWatch();
			RotationPopup value = AssetLoader.Instantiate<RotationPopup>(prefabPath, _popupsParent);
			LogWatch(watch2, "instantiate", prefabPath);
			_popupDictionary.Add(typeFromHandle, value);
		}
		return (RotationPopup)_popupDictionary[typeFromHandle];
	}

	public WildcardPopup GetWildcardPopup()
	{
		Type typeFromHandle = typeof(WildcardPopup);
		if (!_popupDictionary.ContainsKey(typeFromHandle))
		{
			Stopwatch watch = StartWatch();
			string prefabPath = _assetLookupSystem.GetPrefabPath<WildcardPopupPrefab, WildcardPopup>();
			LogWatch(watch, "load", prefabPath);
			Stopwatch watch2 = StartWatch();
			WildcardPopup wildcardPopup = AssetLoader.Instantiate<WildcardPopup>(prefabPath, _popupsParent);
			wildcardPopup.Inject(_keyboardManager);
			LogWatch(watch2, "instantiate", prefabPath);
			_popupDictionary.Add(typeFromHandle, wildcardPopup);
		}
		return (WildcardPopup)_popupDictionary[typeFromHandle];
	}

	public CinematicVideo PlayVideo(VideoClip clip, GameObject closerScreen)
	{
		Type typeFromHandle = typeof(CinematicVideo);
		if (clip == null)
		{
			return null;
		}
		if (_popupDictionary.ContainsKey(typeFromHandle) && _popupDictionary[typeFromHandle].gameObject.activeSelf)
		{
			return null;
		}
		if (!_popupDictionary.ContainsKey(typeFromHandle))
		{
			Stopwatch watch = StartWatch();
			string prefabPath = _assetLookupSystem.GetPrefabPath<CinematicVideoPrefab, CinematicVideo>();
			LogWatch(watch, "load", prefabPath);
			Stopwatch watch2 = StartWatch();
			CinematicVideo value = AssetLoader.Instantiate<CinematicVideo>(prefabPath, _overlayParent);
			LogWatch(watch2, "instantiate", prefabPath);
			_popupDictionary.Add(typeFromHandle, value);
		}
		NavContentType contentType = CurrentContentType;
		LoadContentInternal(NavContentType.None, SceneChangeInitiator.System, "PlayVideo Initiated");
		((CinematicVideo)_popupDictionary[typeFromHandle]).Activate(clip);
		CinematicVideo obj = (CinematicVideo)_popupDictionary[typeFromHandle];
		obj.OnComplete = (Action)Delegate.Combine(obj.OnComplete, (Action)delegate
		{
			closerScreen.SetActive(value: true);
			LoadContentInternal(contentType, SceneChangeInitiator.System, "PlayVideo Complete");
		});
		return (CinematicVideo)_popupDictionary[typeFromHandle];
	}

	public RotationPreviewPopup GetRotationPreviewPopup()
	{
		Type typeFromHandle = typeof(RotationPreviewPopup);
		if (!_popupDictionary.ContainsKey(typeFromHandle))
		{
			Stopwatch watch = StartWatch();
			string prefabPath = _assetLookupSystem.GetPrefabPath<RotationPreviewPopupPrefab, RotationPreviewPopup>();
			LogWatch(watch, "load", prefabPath);
			Stopwatch watch2 = StartWatch();
			RotationPreviewPopup value = AssetLoader.Instantiate<RotationPreviewPopup>(prefabPath, _popupsParent);
			LogWatch(watch2, "instantiate", prefabPath);
			_popupDictionary.Add(typeFromHandle, value);
		}
		return (RotationPreviewPopup)_popupDictionary[typeFromHandle];
	}

	public RenewalPreviewPopup GetRenewalPreviewPopup()
	{
		Type typeFromHandle = typeof(RenewalPreviewPopup);
		if (!_popupDictionary.ContainsKey(typeFromHandle))
		{
			Stopwatch watch = StartWatch();
			string prefabPath = _assetLookupSystem.GetPrefabPath<RenewalPreviewPopupPrefab, RenewalPreviewPopup>();
			LogWatch(watch, "load", prefabPath);
			Stopwatch watch2 = StartWatch();
			RenewalPreviewPopup value = AssetLoader.Instantiate<RenewalPreviewPopup>(prefabPath, _popupsParent);
			LogWatch(watch2, "instantiate", prefabPath);
			_popupDictionary.Add(typeFromHandle, value);
		}
		return (RenewalPreviewPopup)_popupDictionary[typeFromHandle];
	}

	public void EnableCardViewerPopup(bool craftingMode, uint grpid, string craftSkin, int quantityToCraft, Action<string> onSelect = null, Action<Action> onNav = null, uint artId = 0u)
	{
		Type typeFromHandle = typeof(CardViewerController);
		CardViewerController cardViewerController;
		if (_popupDictionary.TryGetValue(typeFromHandle, out var value))
		{
			cardViewerController = value as CardViewerController;
			if ((object)cardViewerController != null)
			{
				goto IL_0083;
			}
		}
		Stopwatch watch = StartWatch();
		string prefabPath = _assetLookupSystem.GetPrefabPath<CardViewerPrefab, CardViewerController>();
		LogWatch(watch, "load", prefabPath);
		Stopwatch watch2 = StartWatch();
		cardViewerController = AssetLoader.Instantiate<CardViewerController>(prefabPath, _popupsParent);
		LogWatch(watch2, "instantiate", prefabPath);
		_popupDictionary.Add(typeFromHandle, cardViewerController);
		goto IL_0083;
		IL_0083:
		cardViewerController.gameObject.SetActive(value: true);
		cardViewerController.Setup(craftingMode, grpid, craftSkin, quantityToCraft, onSelect, onNav, GetCardZoomView(), _cosmetics, _cardDatabase, _cardViewBuilder, _locManager, _setMetadataProvider, _titleCountManager, artId);
		cardViewerController.Activate(activate: true);
	}

	public void DisableCardViewerPopup()
	{
		if (_popupDictionary.ContainsKey(typeof(CardViewerController)) && _popupDictionary[typeof(CardViewerController)].IsShowing)
		{
			_popupDictionary[typeof(CardViewerController)].Activate(activate: false);
		}
	}

	public void EnableBonusPackProgressMeter()
	{
		Type typeFromHandle = typeof(PackProgressMeter);
		PackProgressMeter packProgressMeter;
		if (_popupDictionary.TryGetValue(typeFromHandle, out var value))
		{
			packProgressMeter = value as PackProgressMeter;
			if ((object)packProgressMeter != null)
			{
				goto IL_00c7;
			}
		}
		Stopwatch watch = StartWatch();
		string prefabPath = _assetLookupSystem.GetPrefabPath<PackProgressMeterPrefab, PackProgressMeter>();
		if (prefabPath == null)
		{
			_logger.LogError("Since the BonusPackProgressMeter prefab path could not be loaded, skipping EnableBonusPackProgressMeter");
			return;
		}
		LogWatch(watch, "load", prefabPath);
		watch = StartWatch();
		packProgressMeter = AssetLoader.Instantiate<PackProgressMeter>(prefabPath, _popupsParent);
		if (packProgressMeter == null)
		{
			_logger.LogError("Since the BonusPackProgressMeter prefab could not be loaded from path " + prefabPath + ", skipping EnableBonusPackProgressMeter");
			return;
		}
		LogWatch(watch, "instantiate", prefabPath);
		_popupDictionary.Add(typeFromHandle, packProgressMeter);
		goto IL_00c7;
		IL_00c7:
		packProgressMeter.Activate(activate: true);
	}

	public void DisableBonusPackProgressMeter()
	{
		if (_popupDictionary.TryGetValue(typeof(PackProgressMeter), out var value) && value.IsShowing)
		{
			value.Activate(activate: false);
		}
	}

	public void InPurchaseConfirmation(bool inPurchaseConfirmation)
	{
		if (_popupDictionary.TryGetValue(typeof(PackProgressMeter), out var value) && value.IsShowing)
		{
			(value as PackProgressMeter).InStorePurchaseConfirmation(inPurchaseConfirmation);
		}
	}

	public bool HasPopups()
	{
		if (!SystemMessageManager.Instance.ShowingMessage && (!(_rewardsContentControllerInstance != null) || !_rewardsContentControllerInstance.Visible) && (!_popupDictionary.ContainsKey(typeof(CinematicVideo)) || !_popupDictionary[typeof(CinematicVideo)].IsShowing) && (!_popupDictionary.ContainsKey(typeof(SetAnnouncementController)) || !_popupDictionary[typeof(SetAnnouncementController)].IsShowing) && (!_popupDictionary.ContainsKey(typeof(MOTDPopup)) || !_popupDictionary[typeof(MOTDPopup)].IsShowing) && (!_popupDictionary.ContainsKey(typeof(RenewalPopup)) || !_popupDictionary[typeof(RenewalPopup)].IsShowing) && (!_popupDictionary.ContainsKey(typeof(RotationPopup)) || !_popupDictionary[typeof(RotationPopup)].IsShowing) && (!_popupDictionary.ContainsKey(typeof(RenewalPreviewPopup)) || !_popupDictionary[typeof(RenewalPreviewPopup)].IsShowing))
		{
			if (_popupDictionary.ContainsKey(typeof(RotationPreviewPopup)))
			{
				return _popupDictionary[typeof(RotationPreviewPopup)].IsShowing;
			}
			return false;
		}
		return true;
	}

	public void DestroyPopup<T>() where T : PopupBase
	{
		PopupBase value = null;
		_popupDictionary.TryGetValue(typeof(T), out value);
		if ((bool)value)
		{
			UnityEngine.Object.Destroy(value.gameObject);
			_popupDictionary.Remove(typeof(T));
		}
	}

	public bool GetIsHomeScreenReady()
	{
		if (CurrentContentType == NavContentType.Home && !_isLoading && _loadingCount == 0 && !HasPopups())
		{
			return _navContentLoader.GetController<HomePageContentController>(NavContentType.Home).HomePageState == HomePageState.Normal;
		}
		return false;
	}

	public void ShowConnectionFailedMessage(string title, string message, bool allowRetry = true, bool exitInsteadOfLogout = false)
	{
		ForceDisableLoadingIndicator();
		Pantry.Get<FrontDoorConnectionManager>().ShowConnectionFailedMessage(title, message, allowRetry, exitInsteadOfLogout);
	}

	private void InitNavContentData()
	{
		if (_navContentLoader == null)
		{
			return;
		}
		_navContentLoader.AddNavContentData(NavContentType.Home, canDynamicLoad: false, InitHome, (NavContentType currentType, NavContentType nextType) => true);
		_navContentLoader.AddNavContentData(NavContentType.Profile, canDynamicLoad: false, InitProfile, (NavContentType currentType, NavContentType nextType) => true);
		_navContentLoader.AddNavContentData(NavContentType.BoosterChamber, canDynamicLoad: false, InitBoosterChamber, (NavContentType currentType, NavContentType nextType) => true);
		_navContentLoader.AddNavContentData(NavContentType.Store, canDynamicLoad: false, InitStore, (NavContentType currentType, NavContentType nextType) => true);
		_navContentLoader.AddNavContentData(NavContentType.Achievements, canDynamicLoad: false, InitAchievements, (NavContentType currentType, NavContentType nextType) => true);
		_navContentLoader.AddNavContentData(NavContentType.DeckListViewer, canDynamicLoad: false, InitDeckManager, (NavContentType currentType, NavContentType nextType) => nextType != NavContentType.DeckBuilder);
		_navContentLoader.AddNavContentData(NavContentType.DeckBuilder, canDynamicLoad: false, InitDeckBuilder, delegate(NavContentType currentType, NavContentType nextType)
		{
			if (nextType != NavContentType.DeckListViewer && _navContentLoader.HasNavContentController(NavContentType.DeckListViewer))
			{
				_navContentLoader.UnloadNavContent(NavContentType.None, NavContentType.DeckListViewer);
			}
			DestroyPopup<CardViewerController>();
			return true;
		});
		_navContentLoader.AddNavContentData(NavContentType.RewardTrack, canDynamicLoad: false, InitRewardTrack, (NavContentType currentType, NavContentType nextType) => nextType != NavContentType.RewardTree);
		_navContentLoader.AddNavContentData(NavContentType.RewardTree, canDynamicLoad: false, InitRewardTree, delegate(NavContentType currentType, NavContentType nextType)
		{
			if (nextType != NavContentType.RewardTrack && _navContentLoader.HasNavContentController(NavContentType.RewardTrack))
			{
				_navContentLoader.UnloadNavContent(NavContentType.None, NavContentType.RewardTrack);
			}
			return true;
		});
		_navContentLoader.AddNavContentData(NavContentType.LearnToPlay, canDynamicLoad: false, InitLearnToPlay, (NavContentType currentType, NavContentType nextType) => true);
		_navContentLoader.AddNavContentData(NavContentType.SealedBoosterOpen, canDynamicLoad: false, InitSealedOpen, (NavContentType currentType, NavContentType nextType) => true);
		_navContentLoader.AddNavContentData(NavContentType.Draft, canDynamicLoad: false, InitDraft, (NavContentType currentType, NavContentType nextType) => true);
		_navContentLoader.AddNavContentData(NavContentType.PrizeWall, canDynamicLoad: false, InitPrizeWall, (NavContentType currentType, NavContentType nextType) => true);
		_navContentLoader.AddNavContentData(NavContentType.ConstructedDeckSelect, canDynamicLoad: false, InitConstructedDeckSelect);
		_navContentLoader.AddNavContentData(NavContentType.TableDraftQueue, canDynamicLoad: false, InitTableDraftQueue);
		_navContentLoader.AddNavContentData(NavContentType.EventLanding, canDynamicLoad: false, InitEventPage);
		_navContentLoader.AddNavContentData(NavContentType.ChallengeEventLanding, canDynamicLoad: false, InitChallengeEventPage);
		_navContentLoader.AddNavContentData(NavContentType.PacketSelect, canDynamicLoad: false, LoadPacketSelect);
		_navContentLoader.AddNavContentData(NavContentType.FactionalizedEvent, canDynamicLoad: false, InitFactionalizedEventPage);
	}
}
