using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AssetLookupTree;
using AssetLookupTree.Blackboard;
using Assets.Core.Meta.Utilities;
using Core.BI;
using Core.Code.ClientFeatureToggle;
using Core.Code.PlayerInbox;
using Core.Code.PrizeWall;
using Core.MainNavigation.RewardTrack;
using Core.Meta.MainNavigation;
using Core.Meta.MainNavigation.Challenge;
using Core.Meta.MainNavigation.Notifications;
using Core.Meta.MainNavigation.Notifications.PopupNotifications;
using Core.Meta.NewPlayerExperience.Graph;
using Core.Shared.Code.ClientModels;
using Core.Shared.Code.Connection;
using Core.Shared.Code.Network;
using Core.Shared.Code.Providers;
using GreClient.CardData;
using MovementSystem;
using Pooling;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wizards.Arena.Promises;
using Wizards.MDN;
using Wizards.MDN.Services.Models.PlayerInventory.CampaignGraph;
using Wizards.Models;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Models.Renewal;
using Wizards.Mtga;
using Wizards.Mtga.Assets;
using Wizards.Mtga.Decks;
using Wizards.Mtga.Deeplink;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Platforms;
using Wizards.Mtga.PlayBlade;
using Wizards.Mtga.PreferredPrinting;
using Wizards.Unification.Models.Event;
using Wizards.Unification.Models.NetDeck;
using Wizards.Unification.Models.PlayBlade;
using Wotc.Mtga;
using Wotc.Mtga.Cards.ArtCrops;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Login;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;
using Wotc.Mtga.Wrapper.BonusPack;

public class WrapperController : MonoBehaviour
{
	public struct DebugFlags
	{
		public bool VouchersPopup;

		public bool SeasonPayoutPopup;

		public bool EventPayoutPopup;

		public bool MythicQualifyPopup;

		public bool BannedPopup;

		public bool LoginGrantPopup;

		public bool SetAnnouncePopup;

		public bool MOZTutorialPopup;

		public bool RotationPopup;
	}

	public const uint COMMON_WILDCARD_GRPID = 9u;

	public const uint UNCOMMON_WILDCARD_GRPID = 8u;

	public const uint RARE_WILDCARD_GRPID = 7u;

	public const uint MYTHICRARE_WILDCARD_GRPID = 6u;

	private NavBarController _navBarController;

	public RewardScheduleIntermediate RewardSchedule;

	public PrizeWallDataProvider PrizeWallDataProvider;

	private IEmoteDataProvider _emoteDataProvider;

	private QuestDataProvider _questDataProvider;

	private NetDeckFolderDataProvider _netDeckFolderDataProvider;

	private PopupNotificationManager _popupNotificationManager;

	private VoucherDataProvider _voucherDataProvider;

	private DeckFolderStatesDataProvider _deckFolderStatesDataProvider;

	private PlayerInboxDataProvider _playerInboxDataProvider;

	private IBILogger _biLogger;

	public SparkyTourState SparkyTourState;

	public DecksManager DecksManager;

	public FormatManager FormatManager;

	public UnityCrossThreadLogger UnityCrossThreadLogger;

	public IPreconDeckServiceWrapper PreconDeckManager;

	[NonSerialized]
	public PostMatchClientUpdate PostMatchClientUpdate;

	[NonSerialized]
	public DebugFlags DebugFlag;

	private static bool wrapperSceneEverLoaded = false;

	private NewPlayerExperienceStrategy _npeStrategy;

	private MatchManager _matchManager;

	private bool _firstTime = true;

	private IStartHookServiceWrapper _startHookServiceWrapper;

	public float SecondsSinceBoot;

	private static readonly Dictionary<GameObject, bool> _temporarilyHiddenGameObjects = new Dictionary<GameObject, bool>();

	public static WrapperController Instance { get; private set; }

	public IFrontDoorConnectionServiceWrapper FrontDoorConnectionServiceWrapper { get; private set; }

	public InventoryManager InventoryManager { get; private set; }

	public IAccountClient AccountClient { get; private set; }

	public SceneLoader SceneLoader { get; private set; }

	public CardDatabase CardDatabase { get; private set; }

	public CardViewBuilder CardViewBuilder { get; private set; }

	public CardMaterialBuilder CardMaterialBuilder { get; private set; }

	public Matchmaking Matchmaking { get; internal set; }

	public EventManager EventManager { get; private set; }

	public NavBarController NavBarController
	{
		get
		{
			if (_navBarController != null)
			{
				return _navBarController;
			}
			_navBarController = SceneLoader.GetNavBar();
			return _navBarController;
		}
	}

	public StoreManager Store { get; private set; }

	public BonusPackManager BonusPackManager { get; private set; }

	public RenewalManager RenewalManager { get; private set; }

	public AssetLookupSystem AssetLookupSystem { get; private set; }

	public SettingsMenuHost SettingsMenuHost { get; private set; }

	public NPEState NPEState { get; private set; }

	private IPlayerRankServiceWrapper PlayerRankServiceWrapper { get; set; }

	public IEmoteDataProvider EmoteDataProvider => InjectionDependencyBypassWarning(_emoteDataProvider);

	public NavContentType CurrentContentType => SceneLoader.CurrentContentType;

	public IUnityObjectPool UnityObjectPool { get; private set; }

	private static T InjectionDependencyBypassWarning<T>(T member)
	{
		if (member == null)
		{
			if (Application.isEditor)
			{
				UnityEngine.Debug.LogError($"Failed getting {typeof(T)} through static instance of WrapperController. " + "Consider injecting dependency rather than doing this.");
			}
			return default(T);
		}
		UnityEngine.Debug.LogWarning($"Attempting to get {typeof(T)} through static instance of WrapperController. " + "Consider injecting dependency rather than doing this.");
		return member;
	}

	public static IEnumerator Coroutine_Load(PAPA papa, CardArtTextureLoader cardArtTextureLoader, IArtCropProvider cardCropDatabase, UnityCrossThreadLogger crossThreadLogger, MOTDSession motdSession, IEmoteDataProvider emoteDataProvider, IBILogger biLogger, object panelContext, CosmeticsProvider cosmetics)
	{
		AsyncOperation unloadOp = Resources.UnloadUnusedAssets();
		while (!unloadOp.isDone)
		{
			yield return null;
		}
		GC.Collect();
		WrapperSceneManagement wrapperSceneManagement = Pantry.Get<WrapperSceneManagement>();
		Stopwatch sw = new Stopwatch();
		sw.Start();
		Scene sceneByName = SceneManager.GetSceneByName("AssetPrep");
		if (sceneByName.isLoaded)
		{
			yield return SceneManager.UnloadSceneAsync(sceneByName);
		}
		yield return Scenes.LoadSceneAsync("MainNavigation", LoadSceneMode.Additive);
		sw.Stop();
		UnityEngine.Debug.Log($"MainNav load in {sw.ElapsedMilliseconds}ms");
		Scene sceneByName2 = SceneManager.GetSceneByName("MainNavigation");
		SceneManager.SetActiveScene(sceneByName2);
		WrapperController wrapper = sceneByName2.GetSceneComponent<WrapperController>();
		wrapper.FrontDoorConnectionServiceWrapper = papa.FrontDoorConnection;
		wrapper.AccountClient = Pantry.Get<IAccountClient>();
		wrapper.DecksManager = Pantry.Get<DecksManager>();
		wrapper.CardDatabase = papa.CardDatabase;
		wrapper.Matchmaking = papa.Matchmaking;
		wrapper.EventManager = papa.EventManager;
		wrapper.InventoryManager = Pantry.Get<InventoryManager>();
		wrapper.CardViewBuilder = papa.CardViewBuilder;
		wrapper.CardMaterialBuilder = papa.CardMaterialBuilder;
		wrapper.AssetLookupSystem = papa.AssetLookupSystem;
		wrapper.PlayerRankServiceWrapper = Pantry.Get<IPlayerRankServiceWrapper>();
		wrapper._popupNotificationManager = Pantry.Get<PopupNotificationManager>();
		wrapper._matchManager = papa.MatchManager;
		wrapper._voucherDataProvider = Pantry.Get<VoucherDataProvider>();
		wrapper._deckFolderStatesDataProvider = Pantry.Get<DeckFolderStatesDataProvider>();
		ICustomTokenProvider customTokenProvider = Pantry.Get<ICustomTokenProvider>();
		wrapper.SceneLoader = UnityUtilities.FindObjectOfType<SceneLoader>(includeInactive: true);
		wrapper.SceneLoader.Init(papa, motdSession, crossThreadLogger, cardCropDatabase, cardArtTextureLoader, emoteDataProvider, cosmetics, papa.CardDatabase.GreLocProvider, customTokenProvider, wrapper._voucherDataProvider);
		yield return wrapperSceneManagement.LoadScene_Coroutine(new WrapperSceneInformation("NavBar", WrapperSceneLifeCycle.Wrapper));
		wrapper._navBarController = wrapper.SceneLoader.GetNavBar();
		wrapper.PreconDeckManager = papa.PreconDeckManager;
		wrapper._questDataProvider = Pantry.Get<QuestDataProvider>();
		wrapper._netDeckFolderDataProvider = Pantry.Get<NetDeckFolderDataProvider>();
		wrapper.UnityCrossThreadLogger = crossThreadLogger;
		wrapper.FormatManager = papa.FormatManager;
		wrapper.Store = Pantry.Get<StoreManager>();
		wrapper.Store.OnLoad();
		wrapper.BonusPackManager = new BonusPackManager(biLogger, wrapper.InventoryManager, wrapper.Store);
		wrapper.RenewalManager = new RenewalManager();
		wrapper._emoteDataProvider = emoteDataProvider;
		wrapper._biLogger = biLogger;
		wrapper.SettingsMenuHost = papa.SettingsMenuHost;
		wrapper.NPEState = papa.NpeState;
		wrapper._playerInboxDataProvider = Pantry.Get<PlayerInboxDataProvider>();
		wrapper.PrizeWallDataProvider = Pantry.Get<PrizeWallDataProvider>();
		wrapper.AssetLookupSystem.Blackboard.AddFillerDelegate(wrapper.FillBlackboard);
		yield return wrapper.Init(panelContext);
		if (!wrapperSceneEverLoaded)
		{
			wrapperSceneEverLoaded = true;
			BIEventType.WrapperSceneLoadedForFirstTime.SendWithDefaults();
		}
	}

	public static IEnumerator Coroutine_Reload(object panelContext)
	{
		AsyncOperation unloadOp = Resources.UnloadUnusedAssets();
		while (!unloadOp.isDone)
		{
			yield return null;
		}
		GC.Collect();
		UnityEngine.Debug.Log($"{DateTime.Now} Coroutine_Reload");
		Scene sceneByName = SceneManager.GetSceneByName("MainNavigation");
		SceneManager.SetActiveScene(sceneByName);
		WrapperController sceneComponent = sceneByName.GetSceneComponent<WrapperController>();
		yield return sceneComponent.Init(panelContext);
	}

	public IEnumerator Coroutine_TransitionOut()
	{
		yield return SceneLoader.Coroutine_TransitionOut();
	}

	private void FillBlackboard(IBlackboard bb)
	{
		bb.NavContentType = CurrentContentType;
	}

	public void OnDestroy()
	{
		AssetLookupSystem.Blackboard?.RemoveFillerDelegate(FillBlackboard);
		if (InventoryManager != null)
		{
			InventoryManager.UnSubscribe(InventoryUpdateSource.MercantilePurchase, OnMercantilePurchase);
			InventoryManager.UnSubscribe(InventoryUpdateSource.MercantileChestPurchase, OnMercantileChestPurchase);
			InventoryManager.UnSubscribe(InventoryUpdateSource.CatalogPurchase, OnCatalogPurchase);
			InventoryManager.UnSubscribe(InventoryUpdateSource.MercantileBoosterPurchase, OnInventoryUpdateFromPurchase);
			InventoryManager.UnSubscribe(InventoryUpdateSource.CosmeticPurchase, OnInventoryUpdateFromPurchase);
			InventoryManager.UnSubscribe(InventoryUpdateSource.ModifyPlayerInventory, OnModifyPlayerInventory);
			InventoryManager.UnSubscribe(InventoryUpdateSource.CustomerSupportGrant, OnInventoryUpdate);
			InventoryManager.UnSubscribe(InventoryUpdateSource.OpenChest, OnRedeemInventory);
			InventoryManager.UnSubscribe(InventoryUpdateSource.CampaignGraphPayoutNode, OnRedeemInventory);
			InventoryManager.UnSubscribe(InventoryUpdateSource.CampaignGraphAutomaticPayoutNode, OnRedeemInventory);
			InventoryManager.UnSubscribe(InventoryUpdateSource.AccumulativePayoutNode, OnRedeemInventory);
			InventoryManager.UnSubscribe(InventoryUpdateSource.EventRefundEntry, OnEventRefunded);
			InventoryManager.UnSubscribe(InventoryUpdateSource.CrossPlatformReward, OnCrossPlatformReward);
			InventoryManager.InventoryUpdated -= InventoryManager_InventoryUpdated;
		}
		DecksManager?.OnDestroy();
		Store?.OnDestroy();
		BonusPackManager?.OnDestroy();
		Application.deepLinkActivated -= OnDeepLink;
		Shutdown();
		Instance = null;
	}

	private void Awake()
	{
		if (Instance != null)
		{
			UnityEngine.Debug.LogError("!!! We already have a Wrapper instance. Something has gone very wrong.");
			UnityEngine.Object.Destroy(Instance.gameObject);
		}
		Instance = this;
	}

	public IEnumerator Init(object panelContext)
	{
		InventoryManager.Subscribe(InventoryUpdateSource.MercantilePurchase, OnMercantilePurchase, null, publish: false);
		InventoryManager.Subscribe(InventoryUpdateSource.MercantileChestPurchase, OnMercantileChestPurchase, null, publish: false);
		InventoryManager.Subscribe(InventoryUpdateSource.CatalogPurchase, OnCatalogPurchase, null, publish: false);
		InventoryManager.Subscribe(InventoryUpdateSource.MercantileBoosterPurchase, OnInventoryUpdateFromPurchase, null, publish: false);
		InventoryManager.Subscribe(InventoryUpdateSource.CosmeticPurchase, OnInventoryUpdateFromPurchase, null, publish: false);
		InventoryManager.Subscribe(InventoryUpdateSource.ModifyPlayerInventory, OnModifyPlayerInventory, null, publish: false);
		InventoryManager.Subscribe(InventoryUpdateSource.CustomerSupportGrant, OnInventoryUpdate, null, publish: false);
		InventoryManager.Subscribe(InventoryUpdateSource.OpenChest, OnRedeemInventory, null, publish: false);
		InventoryManager.Subscribe(InventoryUpdateSource.CampaignGraphPayoutNode, OnRedeemInventory, null, publish: false);
		InventoryManager.Subscribe(InventoryUpdateSource.CampaignGraphAutomaticPayoutNode, OnRedeemInventory, null, publish: false);
		InventoryManager.Subscribe(InventoryUpdateSource.AccumulativePayoutNode, OnRedeemInventory, null, publish: false);
		InventoryManager.Subscribe(InventoryUpdateSource.CrossPlatformReward, OnCrossPlatformReward, null, publish: false);
		InventoryManager.Subscribe(InventoryUpdateSource.EventRefundEntry, OnEventRefunded);
		InventoryManager.InventoryUpdated += InventoryManager_InventoryUpdated;
		Application.deepLinkActivated += OnDeepLink;
		yield return StartCoroutine(Coroutine_StartupSequence(panelContext));
	}

	public void OnDeepLink(string url)
	{
		DeepLinking.TryNavigateViaUrl(url, this, _biLogger);
	}

	private void OnEventRefunded(ClientInventoryUpdateReportItem reportItem)
	{
		_popupNotificationManager.Enqueue(new SystemMessageNotification
		{
			TitleLocKey = "Draft/EventEnded_Title",
			MessageLocKey = "Draft/EventEnded_Description"
		});
	}

	public void Shutdown()
	{
		SceneLoader?.Cleanup();
	}

	public static void ForceDisableLoadingIndicator()
	{
		Instance.SceneLoader.ForceDisableLoadingIndicator();
	}

	public static void EnableLoadingIndicator(bool enabled)
	{
		Instance.SceneLoader.EnableLoadingIndicator(enabled);
	}

	public static void EnableUniqueLoadingIndicator(string id, bool warnOnDuplicate = true)
	{
		Instance.SceneLoader.EnableUniqueLoadingIndicator(id, warnOnDuplicate);
	}

	public static void DisableUniqueLoadingIndicator(string id, bool warnOnEmpty = true)
	{
		Instance.SceneLoader.DisableUniqueLoadingIndicator(id, warnOnEmpty);
	}

	public List<DeckDisplayInfo> SortDecks(IEnumerable<DeckDisplayInfo> decks, DeckFormat eventFormat = null)
	{
		string formatName = ((eventFormat == null) ? "" : eventFormat.FormatName);
		return (from di in decks
			orderby di.IsValid descending, di.Deck.Summary.Format == formatName descending, FormatManager.GetSafeFormat(di.Deck.Summary.Format).SortOrder, di.Deck.Summary.LastUpdated descending, di.Deck.Summary.Name
			select di).ToList();
	}

	private IEnumerator Coroutine_StartupSequence(object panelContext)
	{
		EnableLoadingIndicator(enabled: true);
		_startHookServiceWrapper = Pantry.Get<IStartHookServiceWrapper>();
		UnityEngine.Debug.Log($"{DateTime.Now} Coroutine_StartupSequence");
		yield return new WaitUntil(() => FrontDoorConnectionServiceWrapper.Connected);
		SparkyTourState = new SparkyTourState();
		SparkyTourState.Init(NPEState);
		if (_firstTime && MDNPlayerPrefs.PLAYERPREFS_Experience == PlayerExperience.Advanced.ToString() && ((panelContext as HomePageContext)?.CameFromNPE ?? false))
		{
			Task skipTour = SparkyTourState.SkipTour();
			yield return new WaitUntil(() => skipTour.IsCompleted);
			MDNPlayerPrefs.SetSparkRankRewardShown(AccountClient.AccountInformation.AccountID, newValue: true);
		}
		Promise<StartData> startHookPromise = _startHookServiceWrapper.StartHook().Then(CampaignGraphManager.UpdateCampaignGraphStates, propagateSourceError: true);
		List<EStaticContent> requestedStaticContent = new List<EStaticContent>
		{
			EStaticContent.AvailableCosmetics,
			EStaticContent.SurveyConfigs,
			EStaticContent.CardNicknames,
			EStaticContent.EmergencyCardBans,
			EStaticContent.AchievementsMetadata,
			EStaticContent.QueueTips
		};
		StaticContentProviders staticContentProviders = new StaticContentProviders
		{
			CosmeticsProvider = Pantry.Get<CosmeticsProvider>(),
			SurveyConfigProvider = Pantry.Get<ISurveyConfigProvider>(),
			CardNicknamesProvider = Pantry.Get<ICardNicknamesProvider>(),
			EmergencyBansProvider = Pantry.Get<IEmergencyCardBansProvider>(),
			AchievementsDataProvider = Pantry.Get<IAchievementDataProvider>(),
			QueueTipProvider = Pantry.Get<IQueueTipProvider>()
		};
		Promise<StaticContentResponse> staticContentPromise = StaticContentController.GetStaticContent(requestedStaticContent, Pantry.Get<StaticContentController>(), Pantry.Get<IFrontDoorConnectionServiceWrapper>().FDCAWS, staticContentProviders);
		yield return new WaitUntil(() => startHookPromise.IsDone && staticContentPromise.IsDone);
		if (!startHookPromise.Successful)
		{
			UnityEngine.Debug.LogError($"Starthook Response failed! Error: {startHookPromise.Error.Message}, Exception: {startHookPromise.Error.Exception}");
			showErrorLoadingDataMessage();
			EnableLoadingIndicator(enabled: false);
			yield break;
		}
		ClientFeatureToggleDataProvider featureToggleController = Pantry.Get<ClientFeatureToggleDataProvider>();
		featureToggleController.InjectFrontDoor(FrontDoorConnectionServiceWrapper);
		if (startHookPromise.Result != null)
		{
			SystemMessageDataProvider systemMessageDataProvider = Pantry.Get<SystemMessageDataProvider>();
			if (startHookPromise.Result.SystemMessages.Length != 0)
			{
				systemMessageDataProvider.MessageOfTheDay = new MessageOfTheDay
				{
					Title = startHookPromise.Result.SystemMessages[0].Title,
					Message = startHookPromise.Result.SystemMessages[0].Message
				};
			}
			DecksManager.SetDeckLimit(startHookPromise.Result.DeckLimit);
			if (startHookPromise.Result.DesignerCardMetaData != null)
			{
				CardUtilities.NonCraftableCardList = startHookPromise.Result.DesignerCardMetaData.NonCraftableCardList.Select((int x) => (uint)x).ToList();
				CardUtilities.NonCollectibleCardList = startHookPromise.Result.DesignerCardMetaData.NonCollectibleCardList.Select((int x) => (uint)x).ToList();
				if (startHookPromise.Result.DesignerCardMetaData.UnreleasedSets != null)
				{
					CardUtilities.UnreleasedSets = startHookPromise.Result.DesignerCardMetaData.UnreleasedSets;
				}
			}
		}
		IPlayerRankServiceWrapper playerRankServiceWrapper = Pantry.Get<IPlayerRankServiceWrapper>();
		SeasonAndRankDataProvider seasonAndRankDataProvider = Pantry.Get<SeasonAndRankDataProvider>();
		Promise<CardsAndCacheVersion> cardsServiceHandle = Instance.InventoryManager.RefreshCards();
		Promise<CombinedRankInfo> rankHandle = playerRankServiceWrapper.GetPlayerRankInfo();
		Promise<Client_SeasonAndRankInfo> seasonDetailsHandle = seasonAndRankDataProvider.Refresh();
		IRenewalServiceWrapper renewalService = Pantry.Get<IRenewalServiceWrapper>();
		Promise<DTO_CurrentRenewalResponse> currentRenewalPromise = renewalService.GetCurrentRenewal();
		IMercantileServiceWrapper mercantileServiceWrapper = Pantry.Get<IMercantileServiceWrapper>();
		Promise<Client_EntitlementsResponse> entitlementsPromise = mercantileServiceWrapper.CheckEntitlements(shouldRetry: false);
		IPreconDeckServiceWrapper preconServiceWrapper = Pantry.Get<IPreconDeckServiceWrapper>();
		DeckDataProvider deckDataProvider = Pantry.Get<DeckDataProvider>();
		PlayerPrefsDataProvider playerPrefsProvider = Pantry.Get<PlayerPrefsDataProvider>();
		PlayBladeConfigDataProvider playBladeConfigProvider = Pantry.Get<PlayBladeConfigDataProvider>();
		Promise<List<Client_Deck>> deckPromise = deckDataProvider.GetAllDecks();
		yield return deckPromise.AsCoroutine();
		if (!deckPromise.Successful || deckPromise.Result == null)
		{
			UnityEngine.Debug.LogError($"deckPromise: {deckPromise.Successful} : {deckPromise.State.ToString()}");
			SceneLoader.GetSceneLoader().ShowConnectionFailedMessage(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_ClientMessageParseError"));
			yield break;
		}
		Promise<Dictionary<Guid, Client_Deck>> preconPromise = preconServiceWrapper.EnsurePreconDecks();
		Promise<DTO_PlayerPreferences> playerPrefsPromise = playerPrefsProvider.Initialize();
		Promise<List<PlayBladeQueueEntry>> playBladeConfigPromise = playBladeConfigProvider.Initialize();
		Promise<List<DTO_NetDeckFolder>> netDeckFolderPromise = _netDeckFolderDataProvider.Initialize();
		Promise<List<Client_VoucherDefinition>> voucherDefinitionsPromise = _voucherDataProvider.Initialize();
		Promise<List<Client_Letter>> playerInboxPromise = _playerInboxDataProvider.Initialize(featureToggleController.GetToggleValueById("ClientPlayerInbox"));
		Promise<List<Client_PrizeWall>> prizeWallPromise = PrizeWallDataProvider.Initialize(featureToggleController.GetToggleValueById("ClientPrizeWall"));
		Promise<Dictionary<int, PreferredPrintingWithStyle>> preferredPrintingsPromise = Pantry.Get<IPreferredPrintingDataProvider>().ForceRefreshPreferredPrintings();
		Promise<bool> challengesPromise = Pantry.Get<PVPChallengeController>().ReconnectAndCleanupOldChallenges();
		_deckFolderStatesDataProvider.Initialize();
		yield return new WaitUntil(() => cardsServiceHandle.IsDone && rankHandle.IsDone && seasonDetailsHandle.IsDone && currentRenewalPromise.IsDone && entitlementsPromise.IsDone && preconPromise.IsDone && playerPrefsPromise.IsDone && playBladeConfigPromise.IsDone && netDeckFolderPromise.IsDone && voucherDefinitionsPromise.IsDone && playerInboxPromise.IsDone && prizeWallPromise.IsDone && preferredPrintingsPromise.IsDone && challengesPromise.IsDone);
		if (!cardsServiceHandle.Successful)
		{
			UnityEngine.Debug.LogError($"cardsServiceHandle: {cardsServiceHandle.Successful} : {cardsServiceHandle.State.ToString()}");
			SceneLoader.GetSceneLoader().ShowConnectionFailedMessage(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_ClientMessageParseError"));
			yield break;
		}
		_npeStrategy = Pantry.Get<NewPlayerExperienceStrategy>();
		if (InventoryManager.Inventory == null)
		{
			SimpleLog.LogError("Could not resolve player inventory, potentially fatal error");
			ForceDisableLoadingIndicator();
			SceneLoader.GetSceneLoader().ShowConnectionFailedMessage(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Login_Unable_Title"), "Could not resolve player inventory", allowRetry: false);
			yield break;
		}
		RewardSchedule = startHookPromise.Result.RewardSchedule;
		if (rankHandle.Successful && rankHandle.Result != null)
		{
			Pantry.Get<IPlayerRankServiceWrapper>().CombinedRank = rankHandle.Result;
		}
		else
		{
			UnityEngine.Debug.Log("GetCombinedRank unsuccessful in GetInitialData, showing dummy rank.");
			Pantry.Get<IPlayerRankServiceWrapper>().CombinedRank = new CombinedRankInfo();
		}
		Client_Survey surveyConfig = Pantry.Get<ISurveyConfigProvider>().GetSurveyConfig(SurveyConfigProvider.POST_MATCH_SURVEY);
		if (surveyConfig != null)
		{
			_matchManager.SetPostMatchSurveyConfig(surveyConfig);
		}
		IInventoryServiceWrapper inventoryServiceWrapper = Pantry.Get<IInventoryServiceWrapper>();
		RenewalManager.Init(renewalService, inventoryServiceWrapper, currentRenewalPromise.Successful ? currentRenewalPromise.Result : null);
		SetMasteryDataProvider setMasteryDataProvider = Pantry.Get<SetMasteryDataProvider>();
		yield return setMasteryDataProvider.Refresh();
		yield return CampaignGraphManager.SendPendingManualCompleteNodes();
		yield return Store.RefreshStoreDataYield(null);
		StartCoroutine(_npeStrategy.UpdateDataAsync(refresh: true).AsCoroutine());
		SceneLoader.ResetTransitionBlocker();
		UnityEngine.Debug.Log($"{DateTime.Now} Coroutine_StartupSequence - Data retrieved");
		EventContext activeDraft = null;
		if (_firstTime)
		{
			yield return EventManager.Coroutine_GetEventsAndCourses();
			foreach (EventContext ec in EventManager.EventContexts)
			{
				if (ec.PlayerEvent.CourseData.CurrentModule != PlayerEventModule.HumanDraft)
				{
					continue;
				}
				IPlayerEvent playerEvent = ec.PlayerEvent;
				if (playerEvent is LimitedPlayerEvent limitedEvent && !string.IsNullOrWhiteSpace(limitedEvent.CourseData.DraftId))
				{
					if (limitedEvent.DraftPod == null)
					{
						yield return limitedEvent.CreateHumanDraft(rejoinEvent: true, UnityCrossThreadLogger, _biLogger).AsCoroutine();
					}
					if (limitedEvent.DraftPod != null)
					{
						activeDraft = ec;
					}
					break;
				}
			}
		}
		EnableLoadingIndicator(enabled: false);
		if (!_firstTime || !DeepLinking.NavigateViaDeepLink(this, _biLogger))
		{
			if (activeDraft != null)
			{
				SceneLoader.GoToDraftScene(activeDraft);
			}
			else if (panelContext is RewardTreePageContext rewardTreePageContext)
			{
				processPostMatchContext(rewardTreePageContext.PostMatchContext);
				SceneLoader.GoToRewardTreeScene(rewardTreePageContext);
				SceneLoader.SpawnNPEOnboarding();
			}
			else if (panelContext is EventContext eventContext)
			{
				processPostMatchContext(eventContext.PostMatchContext);
				SceneLoader.GoToEventScreen(eventContext);
			}
			else if (panelContext is HomePageContext homePageContext)
			{
				processPostMatchContext(homePageContext.PostMatchContext);
				SceneLoader.GoToLanding(homePageContext, homePageContext.ForceReload);
			}
			else if (panelContext is DeckBuilderContext context)
			{
				SceneLoader.GoToDeckBuilder(context);
			}
			else
			{
				SceneLoader.GoToLanding(new HomePageContext());
			}
		}
		if (_firstTime)
		{
			UnityObjectPool = PlatformContext.CreateUnityPool("Wrapper", keepAlive: false, base.transform, new SplineMovementSystem());
		}
		if (_firstTime)
		{
			CleanupPlayerPrefs();
		}
		_firstTime = false;
		SecondsSinceBoot = Time.realtimeSinceStartup;
		void processPostMatchContext(PostMatchContext postMatchContext)
		{
			if (postMatchContext != null)
			{
				PostMatchClientUpdate = postMatchContext.PostMatchClientUpdate;
				if (PostMatchClientUpdate?.campaignGraphUpdates != null)
				{
					CampaignGraphUpdate[] campaignGraphUpdates = PostMatchClientUpdate.campaignGraphUpdates;
					foreach (CampaignGraphUpdate update in campaignGraphUpdates)
					{
						UpgradePacket upgradePacket = update.upgradePacket;
						if (upgradePacket != null && upgradePacket.cardsAdded?.Count > 0)
						{
							EventManager.EventContexts.FirstOrDefault((EventContext e) => e.PlayerEvent.EventUXInfo.PublicEventName == update.graphName)?.PlayerEvent.UpgradeDeck(update.upgradePacket);
						}
						else if (update.inventoryUpdateReportItem != null)
						{
							DecksManager.OnInventoryUpdate_General(update.inventoryUpdateReportItem);
						}
					}
				}
				if (PostMatchClientUpdate?.questUpdate != null)
				{
					_questDataProvider.UpdateQuestsFromPostMatch(PostMatchClientUpdate);
				}
			}
		}
	}

	private void CleanupPlayerPrefs()
	{
		PlayerPrefsExt.DeleteKey("Birthday");
	}

	private static void showErrorLoadingDataMessage()
	{
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
			Text = Languages.ActiveLocProvider.GetLocalizedText("Boot/BootScene_Button_Retry"),
			Callback = delegate
			{
				Pantry.Get<FrontDoorConnectionManager>().RestartGame("Doorbell Error");
			}
		};
		SystemMessageManager.SystemMessageButtonData item3 = new SystemMessageManager.SystemMessageButtonData
		{
			Text = Languages.ActiveLocProvider.GetLocalizedText("DuelScene/EscapeMenu/Exit_Button_Text"),
			Callback = SceneLoader.ApplicationQuit
		};
		List<SystemMessageManager.SystemMessageButtonData> list = new List<SystemMessageManager.SystemMessageButtonData>();
		list.Add(item);
		list.Add(item2);
		if (!PlatformUtils.IsHandheld())
		{
			list.Add(item3);
		}
		SystemMessageManager.Instance.ShowMessage(Languages.ActiveLocProvider.GetLocalizedText("MainNav/General/ErrorTitle"), Languages.ActiveLocProvider.GetLocalizedText("Boot/BootScene_Error"), list);
	}

	private void OnRedeemInventory(ClientInventoryUpdateReportItem update)
	{
		SceneLoader.GetSceneLoader().GetRewardsContentController().AddAndDisplayRewardsCoroutine(update, Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/Rewards_Title_StoreAcquired"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/EventRewards/ClaimRedeemedButton"));
	}

	private void OnCatalogPurchase(ClientInventoryUpdateReportItem update)
	{
		if (update.delta.artSkinsAdded.Length == 0 || !SceneLoader.GetSceneLoader().IsCardViewerEnabled)
		{
			OnInventoryUpdate(update);
		}
	}

	private void OnModifyPlayerInventory(ClientInventoryUpdateReportItem update)
	{
		if (update.parentcontext != "Event.GrantCardPool")
		{
			OnInventoryUpdate(update);
		}
	}

	private void OnCrossPlatformReward(ClientInventoryUpdateReportItem update)
	{
		OnInventoryUpdate(update);
	}

	private void OnMercantilePurchase(ClientInventoryUpdateReportItem update)
	{
		LogPurchaseComplete();
		OnInventoryUpdate(update);
	}

	private void OnMercantileChestPurchase(ClientInventoryUpdateReportItem update)
	{
		update.xpGained = 0;
		OnInventoryUpdate(update);
	}

	private void OnInventoryUpdateFromPurchase(ClientInventoryUpdateReportItem update)
	{
		OnInventoryUpdate(update);
	}

	private void LogPurchaseComplete()
	{
		_biLogger.Send(ClientBusinessEventType.PurchaseFunnel, PurchaseFunnel.Create(ClientPurchaseFunnelContext.PlayerPurchaseSettled));
	}

	private void OnInventoryUpdate(ClientInventoryUpdateReportItem update)
	{
		SceneLoader.GetSceneLoader().GetRewardsContentController().AddAndDisplayRewardsCoroutine(update, Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/Rewards_Title_StoreAcquired"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/EventRewards/ClaimRedeemedButton"));
		string[] vanityItemsAdded = update.delta.vanityItemsAdded;
		for (int i = 0; i < vanityItemsAdded.Length; i++)
		{
			if (vanityItemsAdded[i].ToLower().StartsWith("cardbacks.") && !MDNPlayerPrefs.HasSeenSleeveNotify)
			{
				MDNPlayerPrefs.HasSeenSleeveNotify = true;
				MDNPlayerPrefs.FirstTimeSleeveNotify = true;
				break;
			}
		}
	}

	private void InventoryManager_InventoryUpdated()
	{
		if (!(NavBarController == null))
		{
			NavBarController.RefreshCurrencyDisplay();
			NavBarController.RefreshVaultProgress(InventoryManager.Inventory.vaultProgress);
		}
	}

	public void TemporarilyHideAllUI(string nextSceneNameToLoad)
	{
		if (_temporarilyHiddenGameObjects.Count > 0)
		{
			RestoreAllTemporarilyHiddenUI();
		}
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			if (sceneAt.name == nextSceneNameToLoad)
			{
				continue;
			}
			GameObject[] rootGameObjects = sceneAt.GetRootGameObjects();
			foreach (GameObject gameObject in rootGameObjects)
			{
				if (gameObject != null)
				{
					bool activeSelf = gameObject.activeSelf;
					gameObject.UpdateActive(active: false);
					_temporarilyHiddenGameObjects.Add(gameObject, activeSelf);
				}
			}
		}
	}

	public void RestoreAllTemporarilyHiddenUI()
	{
		foreach (var (gameObject2, active) in _temporarilyHiddenGameObjects)
		{
			if (gameObject2 != null)
			{
				gameObject2.UpdateActive(active);
			}
		}
		_temporarilyHiddenGameObjects.Clear();
	}
}
