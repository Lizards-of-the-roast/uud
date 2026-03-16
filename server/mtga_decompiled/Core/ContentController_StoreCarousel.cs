using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using AssetLookupTree.Payloads.Store;
using Assets.Core.Shared.Code;
using Core.Code.Decks;
using Core.Code.PrizeWall;
using Core.Code.Promises;
using Core.MainNavigation.RewardTrack;
using Core.Meta.MainNavigation;
using Core.Meta.MainNavigation.Store;
using Core.Meta.MainNavigation.Store.Data;
using Core.Meta.MainNavigation.Store.Tabs;
using Core.Meta.MainNavigation.Store.Utils;
using Core.Shared.Code.CardFilters;
using MovementSystem;
using Pooling;
using SharedClientCore.SharedClientCore.Code.Providers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Wizards.Arena.Client.Logging;
using Wizards.GeneralUtilities.ObjectCommunication;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Logging;
using Wizards.Mtga.Platforms;
using Wizards.Mtga.Store;
using Wizards.Unification.Models.Mercantile;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.CustomInput;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;

public class ContentController_StoreCarousel : NavContentController
{
	private class StoreItemDisplayQueueEntry
	{
		public StoreTabType StoreTabType;

		public StoreItem StoreItem;
	}

	private InventoryManager Inventory;

	[Header("Containers & References")]
	[SerializeField]
	private Tab _tabPrefab;

	[SerializeField]
	private Transform _tabContainer;

	[SerializeField]
	private HorizontalLayoutGroup _storeButtonLayoutGroup;

	[SerializeField]
	private GameObject _contentsContainer;

	[SerializeField]
	private Scrollbar Scrollbar;

	[SerializeField]
	private Button _button_DeletePaymentInfo;

	[SerializeField]
	private BeaconIdentifier _packsButtonBeaconID;

	[Header("Item Models")]
	[SerializeField]
	private StoreItemDisplay _avatarStoreItemModel;

	[SerializeField]
	private StoreItemDisplay _sleeveStoreItemModel;

	[SerializeField]
	private StoreItemDisplay _cardStyleStoreItemModel;

	[SerializeField]
	private StoreItemDisplay _cardStoreItemModel;

	[SerializeField]
	private StoreDisplayPet _petItemModel;

	[SerializeField]
	private StoreItemDisplay _prizeWallStoreItemModel;

	[SerializeField]
	private StoreDisplayPreconDeck _deckStoreItemModel;

	[Header("Store Item Prefabs")]
	[SerializeField]
	private StoreItemBase _storeItemBasePrefab;

	[SerializeField]
	private StoreItemBase _storeItemBaseWidePrefab;

	[Header("Text")]
	[SerializeField]
	private Localize _title;

	[SerializeField]
	private RawImage _packTitle;

	[SerializeField]
	private TMP_Text _packWarningText;

	[SerializeField]
	private Localize _noItemsAvailableBanner;

	[SerializeField]
	private GameObject _dailySalesSubHeader;

	[SerializeField]
	private TMP_Text _dailySalesSubHeader_Text;

	[SerializeField]
	private GameObject _dailySalesSubHeader_NoSales;

	[SerializeField]
	private GameObject _universesBeyondDisplay;

	private AssetLoader.AssetTracker<Texture> _textureTracker = new AssetLoader.AssetTracker<Texture>("PackTitleTextureTracker");

	[Header("Filters")]
	[SerializeField]
	private StoreSetFilterToggles _setFilters;

	[SerializeField]
	private StoreSetFilterDropdown _setFilterDropdown;

	[SerializeField]
	private TMP_Dropdown _storeItemFilterDropdown;

	[Header("Currency & Codes")]
	[SerializeField]
	private GameObject _dropRatesLink;

	[SerializeField]
	private GameObject _taxDisclaimerUSD;

	[SerializeField]
	private GameObject _taxDisclaimerEURO;

	[SerializeField]
	private GameObject _paymentInfoButton;

	[SerializeField]
	private RedeemCodeInputField _redeemCodeInput;

	[SerializeField]
	private bool _canRedeemCode = true;

	private StoreConfirmationModal _confirmationModal;

	[Header("Prize Wall")]
	[SerializeField]
	private Image _prizeWallBackgroundImage;

	[SerializeField]
	private Localize _textPrizeWallHeader;

	[SerializeField]
	private Localize _textPrizeWallSubHeader;

	[SerializeField]
	private GameObject _locatorPrizeWallCurrency;

	[SerializeField]
	private PrizeWallCurrency _currencyPrizeWall;

	[SerializeField]
	private PrizeWallUnlocker _prizeWallLockedState;

	[SerializeField]
	private GameObject _handheldLegalText;

	[Header("Loading")]
	[SerializeField]
	private bool _enableAsyncLoading = true;

	[SerializeField]
	private int _maxDisplayItemsToLoadPerFrame = 1;

	private Queue<StoreItemDisplayQueueEntry> _itemDisplayQueue = new Queue<StoreItemDisplayQueueEntry>();

	private bool _titleDisabledForRewards;

	private StoreTabType _context;

	private Tab _currentTab;

	private Tab _featuredTab;

	private Tab _gemsTab;

	private Tab _packsTab;

	private Tab _dailyDealsTab;

	private Tab _bundlesTab;

	private Tab _cosmeticsTab;

	private Tab _decksTab;

	private Tab _prizeWallTab;

	private Wizards.Arena.Client.Logging.Logger _logger;

	private CardSkinDatabase _cardSkinDB;

	private ICardRolloverZoom _zoomHandler;

	private IBILogger _biLogger;

	private StoreScreenWrapperCompassGuide _compassGuide;

	private int _purchasableSaleItems;

	private string _highlightItemId;

	[SerializeField]
	private UnityEvent _packsTabSelected;

	private readonly Dictionary<Tab, StoreTabType> _storeTabTypeLookup = new Dictionary<Tab, StoreTabType>();

	private bool _readyToShow;

	private AssetLookupSystem _assetLookupSystem;

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	private Coroutine _refreshAllCoroutine;

	private SettingsMenuHost _settingsMenuHost;

	private ConnectionManager _connectionManager;

	private ISetMetadataProvider _setMetadataProvider;

	private PrizeWallDataProvider _prizeWallDataProvider;

	private bool _storePrizeWallEnabled;

	private bool _storePrizeWallUnlocked;

	private Client_PrizeWall _storePrizeWall;

	private Animator _animator;

	private static readonly int PrizeWall_LockedAlert = Animator.StringToHash("PrizeWall_LockedAlert");

	private static readonly int PrizeWall_CurrencyPosition = Animator.StringToHash("PrizeWall_CurrencyPosition");

	private static readonly HashSet<(StoreTabType, string)> _seenFlag = new HashSet<(StoreTabType, string)>();

	private AssetLoader.AssetTracker<Sprite> _prizeWallBackgroundImageSpriteTracker;

	private bool _poolModified;

	private Dictionary<string, StoreItemBase> _pooledItems = new Dictionary<string, StoreItemBase>();

	private List<StoreItemBase> spawnedPackStoreItems = new List<StoreItemBase>();

	private List<StoreItem> _storeItems = new List<StoreItem>();

	private readonly AssetTracker _assetTracker = new AssetTracker();

	private IClientLocProvider _clientLocProvider;

	private IAccountClient _accountClient;

	public override NavContentType NavContentType => NavContentType.Store;

	private StoreManager Store => WrapperController.Instance.Store;

	public StoreTabType CurrentTab
	{
		get
		{
			if (!(_currentTab == null))
			{
				return _storeTabTypeLookup[_currentTab];
			}
			return StoreTabType.None;
		}
	}

	public override bool IsReadyToShow => _readyToShow;

	private IUnityObjectPool _storeObjectPool { get; set; }

	public void SetContext(StoreTabType context)
	{
		_context = context;
		if (IsReadyToShow && base.gameObject.activeInHierarchy)
		{
			StartCoroutine(Coroutine_TrySwitchTab(GetTabFromContext(_context)));
		}
	}

	public void GoToSetPacks(string expansionCode)
	{
		_context = StoreTabType.Packs;
		_setFilters.SelectForGivenSet(expansionCode);
		if (IsReadyToShow && base.gameObject.activeInHierarchy)
		{
			StartCoroutine(Coroutine_TrySwitchTab(GetTabFromContext(_context)));
		}
	}

	public void GoToItem(string id, StoreTabType fallbackContext)
	{
		StoreItem storeItem = (from kvp in Store.StoreListings
			where string.Equals(kvp.Key, id, StringComparison.OrdinalIgnoreCase)
			select kvp.Value).FirstOrDefault();
		if (storeItem != null)
		{
			StoreTabType storeTabType = StoreTabType.None;
			storeTabType = ((storeItem.FeaturedIndex == 1) ? StoreTabType.Featured : GetTabTypeForStoreSection(storeItem.StoreSection));
			_context = storeTabType;
			_highlightItemId = storeItem.Id;
			if (IsReadyToShow && base.gameObject.activeInHierarchy)
			{
				StartCoroutine(Coroutine_TrySwitchTab(GetTabFromContext(_context)));
			}
		}
		else
		{
			SetContext(fallbackContext);
		}
	}

	private void UpdateTabNotificationPip(Tab tab)
	{
		StoreTabLogic.LogicForTab(_storeTabTypeLookup[tab]).GetIsHot().ThenOnMainThreadIfSuccess(tab.SetPipVisible);
	}

	protected override void Start()
	{
		base.Start();
		if (_compassGuide != null)
		{
			if (_compassGuide.ItemId != null)
			{
				GoToItem(_compassGuide.ItemId, _compassGuide.Context);
			}
			else if (_compassGuide.ExpansionCode != null)
			{
				GoToSetPacks(_compassGuide.ExpansionCode);
			}
			else
			{
				SetContext(_compassGuide.Context);
			}
		}
	}

	private void Awake()
	{
		_featuredTab = CreateTab(StoreTabType.Featured, "MainNav/Store/Featured");
		_gemsTab = CreateTab(StoreTabType.Gems, "MainNav/Store/Store_2ndCol_Gems_Bottom_Center");
		_packsTab = CreateTab(StoreTabType.Packs, "MainNav/Store/Store_2ndCol_Packs_Bottom_Center");
		_dailyDealsTab = CreateTab(StoreTabType.DailyDeals, "MainNav/Store/Sales_Tab");
		_bundlesTab = CreateTab(StoreTabType.Bundles, "MainNav/Store/Store_Bundles");
		_cosmeticsTab = CreateTab(StoreTabType.Cosmetics, "MainNav/Store/Store_Cosmetics");
		_decksTab = CreateTab(StoreTabType.Decks, "MainNav/Store/Store_Decks");
		UpdateTabNotificationPip(_featuredTab);
		UpdateTabNotificationPip(_gemsTab);
		UpdateTabNotificationPip(_packsTab);
		UpdateTabNotificationPip(_dailyDealsTab);
		UpdateTabNotificationPip(_bundlesTab);
		UpdateTabNotificationPip(_cosmeticsTab);
		UpdateTabNotificationPip(_decksTab);
		_prizeWallDataProvider = Pantry.Get<PrizeWallDataProvider>();
		_storePrizeWall = _prizeWallDataProvider.GetStoreTabPrizeWall();
		if (_storePrizeWall != null)
		{
			PrizeWallTabLogic obj = StoreTabLogic.LogicForTab(StoreTabType.PrizeWall) as PrizeWallTabLogic;
			_storePrizeWallUnlocked = _prizeWallDataProvider.IsPrizeWallUnlocked(_storePrizeWall.Id);
			if (obj != null && _prizeWallDataProvider.IsPrizeWallBeyondEarnStopDate(_storePrizeWall.Id) && (!_storePrizeWallUnlocked || !PrizeWallDataProvider.PrizeWallHasAffordableItems(Store, _storePrizeWall.Id, _prizeWallDataProvider.GetPrizeWallCurrencyQuantity(_storePrizeWall.Id))))
			{
				_storePrizeWallEnabled = false;
			}
			else
			{
				_storePrizeWallEnabled = true;
				_prizeWallTab = CreateTab(StoreTabType.PrizeWall, _storePrizeWall.NameLocKey, ETabTypeForAnimator.PrizeWall);
				TimeSpan timeSpan = _storePrizeWall.EarnStopDate - ServerGameTime.GameTime;
				TimeSpan timeSpan2 = _storePrizeWall.SpendStopDate - ServerGameTime.GameTime;
				TimeSpan timeSpan3;
				string key;
				string key2;
				if (_storePrizeWall.CurrencyCustomTokenId == null)
				{
					timeSpan3 = timeSpan2;
					key = "MainNav/Store/PrizeWall/TabTimer";
					key2 = "MainNav/Store/PrizeWall/TabTimer";
				}
				else if (timeSpan > TimeSpan.Zero)
				{
					timeSpan3 = timeSpan;
					key = "MainNav/Store/PrizeWall/EarnTabTimer";
					key2 = "MainNav/Store/PrizeWall/EarnSubHeader";
				}
				else
				{
					timeSpan3 = timeSpan2;
					key = "MainNav/Store/PrizeWall/SpendTabTimer";
					key2 = "MainNav/Store/PrizeWall/SpendSubHeader";
				}
				_prizeWallTab.SetSubLabel(key, new Dictionary<string, string>
				{
					{
						"day",
						timeSpan3.Days.ToString()
					},
					{
						"hour",
						timeSpan3.Hours.ToString()
					}
				});
				_textPrizeWallHeader.SetText(_storePrizeWall.NameLocKey);
				_textPrizeWallSubHeader.SetText(key2, new Dictionary<string, string>
				{
					{
						"day",
						timeSpan3.Days.ToString()
					},
					{
						"hour",
						timeSpan3.Hours.ToString()
					}
				});
			}
		}
		else
		{
			_storePrizeWallEnabled = false;
		}
		_packsTab.gameObject.AddComponent<Beacon>().AssignIdentifier(_packsButtonBeaconID, _packsTab.transform);
		_packsTab.gameObject.AddComponent<Beacon>().AssignIdentifier(_packsButtonBeaconID, null, _packsTab.SparkyHighlight);
		if (WrapperController.Instance != null)
		{
			IAccountClient accountClient = WrapperController.Instance.AccountClient;
			if (accountClient != null && accountClient.AccountInformation?.HasRole_Debugging() == true)
			{
				_logger = WrapperController.Instance.UnityCrossThreadLogger;
				goto IL_03a8;
			}
		}
		_logger = new UnityLogger("Store", LoggerLevel.Error);
		LoggerManager.Register(_logger);
		goto IL_03a8;
		IL_03a8:
		_button_DeletePaymentInfo.gameObject.UpdateActive(active: false);
		_connectionManager = Pantry.Get<ConnectionManager>();
		ConnectionManager connectionManager = _connectionManager;
		connectionManager.OnFdReconnected = (Action)Delegate.Combine(connectionManager.OnFdReconnected, new Action(OnFdReconnected));
		_accountClient = Pantry.Get<IAccountClient>();
		if (_animator == null)
		{
			_animator = GetComponent<Animator>();
		}
		Tab CreateTab(StoreTabType type, string text, ETabTypeForAnimator tabType = ETabTypeForAnimator.Default, Action<Tab> onClicked = null)
		{
			Tab tab = UnityEngine.Object.Instantiate(_tabPrefab, _tabContainer);
			_storeTabTypeLookup[tab] = type;
			tab.SetTabType(tabType);
			tab.SetLabel(text);
			tab.SetSparkyBeacons(type.ToString());
			tab.Clicked += onClicked ?? new Action<Tab>(OnTabClicked);
			return tab;
		}
	}

	public void Init(AssetLookupSystem assetLookupSystem, ICardRolloverZoom zoomHandler, IBILogger biLogger, IGreLocProvider greLocalizationManager, IClientLocProvider clientLocalizationProvider, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, SettingsMenuHost settingsMenuHost, Transform contentParent, ISetMetadataProvider setMetadataProvider)
	{
		_biLogger = biLogger;
		_assetLookupSystem = assetLookupSystem;
		_cardDatabase = cardDatabase;
		_cardViewBuilder = cardViewBuilder;
		_settingsMenuHost = settingsMenuHost;
		_setMetadataProvider = setMetadataProvider;
		_clientLocProvider = Pantry.Get<IClientLocProvider>();
		_compassGuide = Pantry.Get<WrapperCompass>().GetGuide<StoreScreenWrapperCompassGuide>();
		string prefabPath = _assetLookupSystem.GetPrefabPath<StoreConfirmationModalPrefab, StoreConfirmationModal>();
		_confirmationModal = AssetLoader.Instantiate<StoreConfirmationModal>(prefabPath, contentParent);
		_confirmationModal.Initialize(cardDatabase, greLocalizationManager, clientLocalizationProvider, _cardViewBuilder, assetLookupSystem);
		_confirmationModal.OnOpened += OnConfirmationOpened;
		_confirmationModal.OnClosed += OnConfirmationClosed;
		_zoomHandler = zoomHandler;
		SetMetadataProvider setMetadataProvider2 = setMetadataProvider as SetMetadataProvider;
		if (PlatformUtils.IsHandheld())
		{
			_setFilterDropdown.Initialize(_assetLookupSystem, _setFilters, setMetadataProvider2.StoreSets);
		}
		_setFilters.Init(_assetLookupSystem, OnSetRefreshed);
		_setFilters.SetSetFilters(setMetadataProvider2.StoreSets);
		_storeObjectPool = PlatformContext.CreateUnityPool("Store", keepAlive: false, base.transform, new SplineMovementSystem());
	}

	private void OnDestroy()
	{
		if (_confirmationModal != null)
		{
			_confirmationModal.OnOpened -= OnConfirmationOpened;
			_confirmationModal.OnClosed -= OnConfirmationClosed;
		}
		_setFilters.CleanUp();
		_assetTracker.Cleanup();
		_cardSkinDB?.Dispose();
		ConnectionManager connectionManager = _connectionManager;
		connectionManager.OnFdReconnected = (Action)Delegate.Remove(connectionManager.OnFdReconnected, new Action(OnFdReconnected));
		_storeObjectPool.Destroy();
		AssetLoaderUtils.CleanupImage(_prizeWallBackgroundImage, _prizeWallBackgroundImageSpriteTracker);
		OnBeginClose();
		OnFinishClose();
	}

	private void OnFdReconnected()
	{
		RefreshAll();
	}

	private void NavigateToCollectionStyles()
	{
		SceneLoader.GetSceneLoader().GoToDeckBuilder(new DeckBuilderContext());
	}

	private void SetActiveTabVisuals(Tab tab)
	{
		if (_storePrizeWallEnabled)
		{
			_prizeWallTab.gameObject.UpdateActive(active: true);
			_prizeWallTab.SetTabActiveVisuals(tab == _prizeWallTab);
		}
		_featuredTab.SetTabActiveVisuals(tab == _featuredTab);
		_gemsTab.SetTabActiveVisuals(tab == _gemsTab);
		_packsTab.SetTabActiveVisuals(tab == _packsTab);
		_bundlesTab.SetTabActiveVisuals(tab == _bundlesTab);
		_dailyDealsTab.SetTabActiveVisuals(tab == _dailyDealsTab);
		_cosmeticsTab.SetTabActiveVisuals(tab == _cosmeticsTab);
		_decksTab.SetTabActiveVisuals(tab == _decksTab);
	}

	public override void OnBeginOpen()
	{
		_readyToShow = false;
		WrapperController instance = WrapperController.Instance;
		Inventory = instance.InventoryManager;
		SceneLoader sceneLoader = SceneLoader.GetSceneLoader();
		ContentControllerRewards rewardsContentController = sceneLoader.GetRewardsContentController();
		rewardsContentController.RegisterRewardClosedCallback(OnRewardsClosed);
		rewardsContentController.OnRewardsDisplayed += OnRewardsDisplayed;
		_settingsMenuHost.CurrencyChangedHandlers += OnCurrencyChanged;
		Inventory.SubscribeToAll(OnInventoryUpdated);
		Languages.LanguageChangedSignal.Listeners += UpdateLocOnCurrentTab;
		if (_setFilters.SelectedModel == null)
		{
			_setFilters.Select(0);
		}
		SetActiveTabVisuals(null);
		foreach (StoreItemBase value in _pooledItems.Values)
		{
			value.gameObject.UpdateActive(active: false);
		}
		_title.gameObject.UpdateActive(active: false);
		_noItemsAvailableBanner.gameObject.UpdateActive(active: false);
		_packWarningText.gameObject.UpdateActive(active: false);
		if (PlatformUtils.IsHandheld())
		{
			_setFilterDropdown.gameObject.UpdateActive(active: false);
		}
		else
		{
			_packTitle.transform.parent.gameObject.UpdateActive(active: false);
		}
		sceneLoader.EnableBonusPackProgressMeter();
		StartCoroutine(Coroutine_GetInitialData());
	}

	private IEnumerator Coroutine_GetInitialData()
	{
		WrapperController.EnableLoadingIndicator(enabled: true);
		bool success = false;
		yield return Store.RefreshStoreDataYield(delegate(bool b)
		{
			success = b;
		});
		if (_redeemCodeInput != null)
		{
			_redeemCodeInput.gameObject.SetActive(_canRedeemCode && Store.StoreStatus.CodeRedemptionEnabled && Store.StoreStatus.CodeRedemptionEnabled);
		}
		yield return Store.GetEntitlements().AsCoroutine();
		WrapperController.EnableLoadingIndicator(enabled: false);
		if (!success)
		{
			SceneLoader.GetSceneLoader().SystemMessages.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Store_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Store_Products_Get_Error_Text"), delegate
			{
				SceneLoader.GetSceneLoader().GoToLanding(new HomePageContext());
			});
		}
		else
		{
			Tab tabFromContext = GetTabFromContext(_context);
			Tab tab = ((!tabFromContext.Locked) ? tabFromContext : _featuredTab);
			ShowTab(tab);
		}
		_readyToShow = true;
	}

	public override void OnBeginClose()
	{
		BI_TrackStoreTabChanged(StoreTabType.None.ToString(), NavContentType.Store.ToString(), "Player left store");
		_currentTab = null;
		if (_confirmationModal != null && _confirmationModal.gameObject.activeSelf)
		{
			_confirmationModal.Close();
		}
		Languages.LanguageChangedSignal.Listeners -= UpdateLocOnCurrentTab;
		Inventory.UnsubscribeFromAll(OnInventoryUpdated);
		_settingsMenuHost.CurrencyChangedHandlers -= OnCurrencyChanged;
		if (SceneLoader.GetSceneLoader() != null)
		{
			SceneLoader.GetSceneLoader().GetRewardsContentController().UnregisterRewardsClosedCallback(OnRewardsClosed);
			SceneLoader.GetSceneLoader().GetRewardsContentController().OnRewardsDisplayed -= OnRewardsDisplayed;
		}
	}

	public override void OnFinishClose()
	{
		base.OnFinishClose();
		foreach (StoreItemBase spawnedPackStoreItem in spawnedPackStoreItems)
		{
			StoreDisplayUtils.DespawnPackWidget(_storeObjectPool, spawnedPackStoreItem);
		}
		SceneLoader sceneLoader = SceneLoader.GetSceneLoader();
		if (sceneLoader != null)
		{
			sceneLoader.DisableBonusPackProgressMeter();
		}
		spawnedPackStoreItems.Clear();
		_storeObjectPool.Clear();
		if (_packTitle != null)
		{
			_packTitle.texture = null;
		}
		_textureTracker.Cleanup();
		_seenFlag.Clear();
	}

	public override void OnHandheldBackButton()
	{
		if (_confirmationModal != null && _confirmationModal.gameObject.activeSelf)
		{
			_confirmationModal.Close();
		}
		else if (CurrentTab != StoreTabType.Featured)
		{
			ShowTab(_featuredTab);
		}
		else
		{
			base.OnHandheldBackButton();
		}
	}

	private void UpdateLocOnCurrentTab()
	{
		ShowTab(_currentTab);
	}

	public void OnCurrencyChanged()
	{
		StartCoroutine(UpdateCurrencyLocYield());
	}

	private IEnumerator UpdateCurrencyLocYield()
	{
		WrapperController.EnableLoadingIndicator(enabled: true);
		yield return Store.ProcessRMTListingsYield();
		WrapperController.EnableLoadingIndicator(enabled: false);
		ShowTab(_currentTab);
	}

	private void OnRewardsDisplayed()
	{
		if (_title.gameObject.activeSelf)
		{
			_titleDisabledForRewards = true;
			_title.gameObject.UpdateActive(active: false);
		}
	}

	private void OnRewardsClosed()
	{
		if (_titleDisabledForRewards)
		{
			_titleDisabledForRewards = false;
			_title.gameObject.UpdateActive(active: true);
		}
	}

	private void OnConfirmationOpened()
	{
		_contentsContainer.UpdateActive(active: false);
		UpdateInPurchaseConfirmation(inPurchaseConfirmation: true);
	}

	private void OnConfirmationClosed()
	{
		_contentsContainer.UpdateActive(active: true);
		foreach (KeyValuePair<string, StoreItemBase> pooledItem in _pooledItems)
		{
			pooledItem.Deconstruct(out var _, out var value);
			value.ResetTags();
		}
		UpdateInPurchaseConfirmation(inPurchaseConfirmation: false);
	}

	private void UpdateInPurchaseConfirmation(bool inPurchaseConfirmation)
	{
		SceneLoader sceneLoader = SceneLoader.GetSceneLoader();
		if (sceneLoader != null)
		{
			sceneLoader.InPurchaseConfirmation(inPurchaseConfirmation);
		}
	}

	public void OnSetRefreshed(StoreSetFilterModel model)
	{
		ShowTab(_packsTab);
	}

	private void OnInventoryUpdated(ClientInventoryUpdateReportItem update)
	{
		RefreshAll();
	}

	private void OnBuyGemsForPurchase()
	{
		_biLogger.Send(ClientBusinessEventType.PurchaseFunnel, PurchaseFunnel.Create(ClientPurchaseFunnelContext.PlayerRedirectedToGemPurchase));
		ShowTab(_gemsTab);
	}

	public void OnButton_PaymentSetup()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
		Store.OpenPaymentSetup();
	}

	public void OnBackButton()
	{
		SceneLoader.GetSceneLoader().GoToStore(StoreTabType.Featured, "Store back button");
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_back, base.gameObject);
	}

	private void OnPurchaseSuccessful(StoreItem item)
	{
		if (!item.HasRemainingPurchases && _pooledItems.TryGetValue(item.Id, out var value))
		{
			if (StoreDisplayUtils.IsPreorder(item))
			{
				StoreDisplayUtils.UpdatePreOrderItem(item, value, _assetLookupSystem);
			}
			else if (CurrentTab == StoreTabType.DailyDeals)
			{
				StoreDisplayUtils.UpdatePurchaseButton(item, value, _assetLookupSystem);
			}
			else
			{
				value.gameObject.UpdateActive(active: false);
			}
		}
		if (item.ListingType == EListingType.PrizeWall)
		{
			OnTabClicked(_prizeWallTab);
		}
	}

	public static StoreTabType GetTabTypeForStoreSection(EStoreSection section)
	{
		return section switch
		{
			EStoreSection.Gems => StoreTabType.Gems, 
			EStoreSection.Packs => StoreTabType.Packs, 
			EStoreSection.CardSkins => StoreTabType.Cosmetics, 
			EStoreSection.CardSleeves => StoreTabType.Cosmetics, 
			EStoreSection.Bundles => StoreTabType.Bundles, 
			EStoreSection.Pets => StoreTabType.Cosmetics, 
			EStoreSection.Sale => StoreTabType.DailyDeals, 
			EStoreSection.Avatars => StoreTabType.Cosmetics, 
			EStoreSection.Cosmetics => StoreTabType.Cosmetics, 
			EStoreSection.Decks => StoreTabType.Decks, 
			EStoreSection.PrizeWall => StoreTabType.PrizeWall, 
			_ => StoreTabType.None, 
		};
	}

	private void RefreshAll()
	{
		if (base.gameObject.activeInHierarchy)
		{
			if (_refreshAllCoroutine != null)
			{
				StopCoroutine(_refreshAllCoroutine);
			}
			_refreshAllCoroutine = StartCoroutine(Coroutine_RefreshAll());
		}
	}

	private IEnumerator Coroutine_RefreshAll()
	{
		WrapperController.EnableUniqueLoadingIndicator("StoreCarousel.RefreshInventory");
		bool success = false;
		yield return Store.RefreshStoreDataYield(delegate(bool b)
		{
			success = b;
		});
		WrapperController.DisableUniqueLoadingIndicator("StoreCarousel.RefreshInventory");
		if (!success)
		{
			SceneLoader.GetSceneLoader().SystemMessages.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Store_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Store_Products_Get_Error_Text"), delegate
			{
				SceneLoader.GetSceneLoader().GoToLanding(new HomePageContext());
			});
		}
		else
		{
			ShowTab(_currentTab);
		}
	}

	private void ExecutePurchase(StoreItem item, Client_PurchaseCurrencyType currencyType)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_purchase_pack, base.gameObject);
		StartCoroutine(Store.PurchaseItemYield(item, currencyType, OnPurchaseSuccessful, 1, (currencyType == Client_PurchaseCurrencyType.CustomToken) ? item.PurchaseOptions.First((Client_PurchaseOption x) => x.CurrencyType == Client_PurchaseCurrencyType.CustomToken).CurrencyId : null));
	}

	private void OnStoreItemPurchaseOptionClicked(StoreItem item, Client_PurchaseCurrencyType currencyType, Action<StoreItem, Client_PurchaseCurrencyType> executePurchase)
	{
		if (item.PrizeWallData != null && !_storePrizeWallUnlocked && item.ListingType != EListingType.PrizeWall)
		{
			if (_animator == null)
			{
				_animator = GetComponent<Animator>();
			}
			_animator.SetTrigger(PrizeWall_LockedAlert);
			return;
		}
		StoreItemBase itemWidget = GetOrCreateItemWidget(item);
		_confirmationModal.SetStoreItem(item, itemWidget, currencyType, delegate(StoreItem i, Client_PurchaseCurrencyType c)
		{
			OnStoreItemConfirmationClicked(i, c, executePurchase);
		}, CurrentTab);
		if (item.ListingType == EListingType.Booster || item.IsSalesPack)
		{
			itemWidget.InvokeUponRestore(delegate
			{
				StoreDisplayUtils.DespawnPackWidget(_storeObjectPool, itemWidget, spawnedPackStoreItems);
			});
		}
	}

	private void OnStoreItemConfirmationClicked(StoreItem item, Client_PurchaseCurrencyType currencyType, Action<StoreItem, Client_PurchaseCurrencyType> executePurchase)
	{
		InventoryManager inventoryManager = WrapperController.Instance.InventoryManager;
		if (item.SubType == "RewardTierUpgrade")
		{
			SetMasteryDataProvider setMasteryDataProvider = Pantry.Get<SetMasteryDataProvider>();
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_gems_payment, base.gameObject);
			setMasteryDataProvider.PurchaseTrackUpgrade();
			return;
		}
		switch (currencyType)
		{
		case Client_PurchaseCurrencyType.Gem:
		{
			int price2 = item.PurchaseOptions.First((Client_PurchaseOption x) => x.CurrencyType == Client_PurchaseCurrencyType.Gem).Price;
			if ((inventoryManager.Inventory?.gems ?? 0) < price2)
			{
				_biLogger.Send(ClientBusinessEventType.PurchaseFunnel, PurchaseFunnel.Create(ClientPurchaseFunnelContext.PlayerSuggestedForGemPurchaseRedirect));
				SystemMessageManager.Instance.ShowOkCancel(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Buy_Gems_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Insufficient_Gems_Text"), OnBuyGemsForPurchase, null);
			}
			else
			{
				executePurchase(item, currencyType);
			}
			break;
		}
		case Client_PurchaseCurrencyType.Gold:
		{
			int price3 = item.PurchaseOptions.First((Client_PurchaseOption x) => x.CurrencyType == Client_PurchaseCurrencyType.Gold).Price;
			if ((inventoryManager.Inventory?.gold ?? 0) >= price3)
			{
				executePurchase(item, currencyType);
			}
			break;
		}
		case Client_PurchaseCurrencyType.RMT:
			executePurchase(item, currencyType);
			break;
		case Client_PurchaseCurrencyType.CustomToken:
		{
			Client_PurchaseOption client_PurchaseOption = item.PurchaseOptions.First((Client_PurchaseOption x) => x.CurrencyType == Client_PurchaseCurrencyType.CustomToken);
			int price = client_PurchaseOption.Price;
			int value = 0;
			inventoryManager.Inventory?.CustomTokens.TryGetValue(client_PurchaseOption.CurrencyId, out value);
			if (value >= price)
			{
				executePurchase(item, currencyType);
			}
			break;
		}
		}
	}

	public void Update()
	{
		if (_itemDisplayQueue.Count > 0)
		{
			DoLayoutItemsToDisplay();
		}
		float y = CustomInputModule.GetMouseScroll().y;
		if (y != 0f && !_settingsMenuHost.IsGameObjectActive())
		{
			Scrollbar.value += ((y < 0f) ? 0.1f : (-0.1f));
		}
		if (CurrentTab != StoreTabType.DailyDeals)
		{
			return;
		}
		if (!Store.SaleInfo.nextSaleRefreshTime.HasValue)
		{
			_dailySalesSubHeader_NoSales.UpdateActive(_purchasableSaleItems == 0);
			_dailySalesSubHeader.UpdateActive(active: false);
			return;
		}
		if (Store.SaleInfo.nextSaleRefreshTime <= ServerGameTime.GameTime)
		{
			ShowTab(_featuredTab);
			Store.ClearSaleTimer();
			RefreshAll();
			return;
		}
		_dailySalesSubHeader_NoSales.UpdateActive(active: false);
		_dailySalesSubHeader.UpdateActive(active: true);
		TimeSpan timeSpan = Store.SaleInfo.nextSaleRefreshTime.Value - ServerGameTime.GameTime;
		MTGALocalizedString mTGALocalizedString = new MTGALocalizedString();
		if ((int)timeSpan.TotalHours > 1)
		{
			mTGALocalizedString.Key = ((timeSpan.Minutes > 1) ? "MainNav/General/Timers/HH_MM_plural_plural" : "MainNav/General/Timers/HH_MM_plural_singular");
		}
		else if ((int)timeSpan.TotalHours == 0)
		{
			mTGALocalizedString.Key = ((timeSpan.Minutes > 1) ? "MainNav/General/Timers/MM_plural" : "MainNav/General/Timers/MM_singular");
		}
		else
		{
			mTGALocalizedString.Key = ((timeSpan.Minutes > 1) ? "MainNav/General/Timers/HH_MM_singular_plural" : "MainNav/General/Timers/HH_MM_singular_singular");
		}
		mTGALocalizedString.Parameters = new Dictionary<string, string>
		{
			{
				"hours",
				((int)timeSpan.TotalHours).ToString()
			},
			{
				"minutes",
				timeSpan.Minutes.ToString()
			}
		};
		_dailySalesSubHeader_Text.text = mTGALocalizedString;
	}

	private Tab GetTabFromContext(StoreTabType context)
	{
		Tab tab = context switch
		{
			StoreTabType.Bundles => _bundlesTab, 
			StoreTabType.Gems => _gemsTab, 
			StoreTabType.Packs => _packsTab, 
			StoreTabType.DailyDeals => _dailyDealsTab, 
			StoreTabType.Cosmetics => _cosmeticsTab, 
			StoreTabType.Decks => _decksTab, 
			StoreTabType.PrizeWall => _prizeWallTab, 
			_ => _featuredTab, 
		};
		if (tab == null)
		{
			tab = _featuredTab;
		}
		return tab;
	}

	private void OnTabClicked(Tab tab)
	{
		if (_currentTab != tab && !tab.Locked)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_whoosh_01, base.gameObject);
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
			StoreTabLogic storeTabLogic = StoreTabLogic.LogicForTab(_storeTabTypeLookup[tab]);
			storeTabLogic.UpdateSeenToCurrent();
			storeTabLogic.GetIsHot().ThenOnMainThreadIfSuccess(tab.SetPipVisible);
			StartCoroutine(Coroutine_TrySwitchTab(tab));
		}
	}

	private IEnumerator Coroutine_TrySwitchTab(Tab tab)
	{
		if (!tab.Locked)
		{
			ShowTab(tab);
		}
		yield break;
	}

	private Tab DefaultTab(Tab tab)
	{
		if (tab != null)
		{
			return tab;
		}
		StoreTabType value;
		foreach (KeyValuePair<Tab, StoreTabType> item in _storeTabTypeLookup)
		{
			item.Deconstruct(out var key, out value);
			Tab result = key;
			if (value == _context)
			{
				return result;
			}
		}
		using (Dictionary<Tab, StoreTabType>.Enumerator enumerator = _storeTabTypeLookup.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				enumerator.Current.Deconstruct(out var key2, out value);
				return key2;
			}
		}
		return null;
	}

	private void ShowTab(Tab tab)
	{
		tab = DefaultTab(tab);
		bool flag = _currentTab != tab;
		if (flag)
		{
			StoreTabType storeTabType = _storeTabTypeLookup[tab];
			bool flag2 = CurrentTab == StoreTabType.None;
			BI_TrackStoreTabChanged((storeTabType == StoreTabType.PrizeWall) ? _storePrizeWall.Id : storeTabType.ToString(), flag2 ? SceneLoader.GetSceneLoader().ActivatingContent.ToString() : NavContentType.Store.ToString(), flag2 ? "Player entered store" : "Player changed tabs");
		}
		_currentTab = tab;
		Scrollbar.value = 0f;
		SetActiveTabVisuals(tab);
		_noItemsAvailableBanner.gameObject.UpdateActive(active: false);
		_dailySalesSubHeader.UpdateActive(active: false);
		_dailySalesSubHeader_NoSales.UpdateActive(active: false);
		StoreTabType tabType = _storeTabTypeLookup[tab];
		StoreTabLogic storeTabLogic = StoreTabLogic.LogicForTab(tabType);
		TryActivateStoreItemFilterDropdown(tabType, storeTabLogic);
		bool showPackTitle = storeTabLogic.ShowPackTitle;
		bool flag3 = PlatformUtils.IsHandheld();
		if (flag3)
		{
			_setFilterDropdown.gameObject.UpdateActive(showPackTitle);
			_setFilterDropdown.ToggleScrollRect(enabled: false);
			_handheldLegalText.gameObject.SetActive(tab.TabType == ETabTypeForAnimator.PrizeWall);
		}
		else
		{
			_packTitle.transform.parent.gameObject.UpdateActive(showPackTitle);
			_redeemCodeInput.gameObject.UpdateActive(tab.TabType != ETabTypeForAnimator.PrizeWall);
		}
		_packWarningText.gameObject.UpdateActive(showPackTitle);
		if (showPackTitle)
		{
			_packWarningText.text = DeckFormatUtils.GetWarningText(_setFilters.SelectedModel.Availability);
			if (flag3)
			{
				_setFilterDropdown.UpdateButtonDisplay(_setFilters.SelectedModel);
			}
			else
			{
				_setFilters.RefreshCurrentlySelected();
				StoreSetUtils.LogoForSetName(_assetLookupSystem, ref _textureTracker, ref _packTitle, _setFilters.SelectedModel.SetSymbolAsCollationMapping);
			}
			_packsTabSelected.Invoke();
		}
		_universesBeyondDisplay.UpdateActive(storeTabLogic.ShowUniversesBeyondLogo(_setFilters));
		SceneLoader sceneLoader = SceneLoader.GetSceneLoader();
		if (_prizeWallBackgroundImageSpriteTracker == null)
		{
			_prizeWallBackgroundImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("PrizeWallBackgroundImageSprite");
		}
		_prizeWallBackgroundImage.gameObject.UpdateActive(tab.TabType == ETabTypeForAnimator.PrizeWall);
		_textPrizeWallHeader.gameObject.UpdateActive(tab.TabType == ETabTypeForAnimator.PrizeWall);
		_locatorPrizeWallCurrency.gameObject.UpdateActive(tab.TabType == ETabTypeForAnimator.PrizeWall);
		if (tab.TabType == ETabTypeForAnimator.PrizeWall)
		{
			AssetLoaderUtils.TrySetSprite(_prizeWallBackgroundImage, _prizeWallBackgroundImageSpriteTracker, PrizeWallUtils.GetBackgroundImagePath(_assetLookupSystem, _storePrizeWall.Id));
			_storePrizeWallUnlocked = _prizeWallDataProvider.IsPrizeWallUnlocked(_storePrizeWall.Id);
			_prizeWallLockedState.gameObject.SetActive(!_storePrizeWallUnlocked);
			if (!_storePrizeWallUnlocked)
			{
				ShowPrizeWallUnlock();
			}
			if (_animator == null)
			{
				_animator = GetComponent<Animator>();
			}
			_animator.SetInteger(PrizeWall_CurrencyPosition, (!_storePrizeWallUnlocked) ? 1 : 3);
			_currencyPrizeWall.SetCurrency(_prizeWallDataProvider, _storePrizeWall, _assetLookupSystem);
			if (_storePrizeWall.CurrencyCustomTokenId != null && Pantry.Get<ICustomTokenProvider>().TokenDefinitions.TryGetValue(_storePrizeWall.CurrencyCustomTokenId, out var value))
			{
				_currencyPrizeWall.UpdateCurrencyCountTooltip(value.HeaderLocKey);
			}
		}
		else
		{
			_animator.SetInteger(PrizeWall_CurrencyPosition, _storePrizeWallUnlocked ? 2 : 0);
			_prizeWallLockedState.gameObject.SetActive(value: false);
		}
		if (sceneLoader != null)
		{
			if (tab.TabType == ETabTypeForAnimator.PrizeWall)
			{
				sceneLoader.DisableBonusPackProgressMeter();
			}
			else
			{
				sceneLoader.EnableBonusPackProgressMeter();
			}
		}
		string titleText = storeTabLogic.TitleText;
		_title.gameObject.UpdateActive(titleText != null && tab.TabType != ETabTypeForAnimator.PrizeWall);
		if (titleText != null)
		{
			_title.SetText(titleText);
		}
		bool showTaxDisclaimer = storeTabLogic.ShowTaxDisclaimer;
		bool flag4 = _accountClient.GetStoreCurrencySelection() == "EUR";
		_taxDisclaimerUSD.UpdateActive(!flag4 && showTaxDisclaimer);
		_taxDisclaimerEURO.UpdateActive(flag4 && showTaxDisclaimer);
		_dropRatesLink.UpdateActive(storeTabLogic.ShowDropRatesLink);
		_paymentInfoButton.UpdateActive(storeTabLogic.ShowPaymentInfoButton);
		_storeItems = GetItemsToDisplay(Store, tabType, _setFilters, _storeItemFilterDropdown);
		LayoutItemsToDisplay(tabType, _storeItems, flag);
		List<StoreItemSeen> list = new List<StoreItemSeen>();
		foreach (StoreItem storeItem in _storeItems)
		{
			if (_seenFlag.Add((CurrentTab, storeItem.Id)))
			{
				StoreItemSeen item = new StoreItemSeen
				{
					StoreItemId = storeItem.Id,
					PlayersPriceInGems = (storeItem.PurchaseOptions.FirstOrDefault((Client_PurchaseOption x) => x.CurrencyType == Client_PurchaseCurrencyType.Gem)?.Price ?? 0),
					PlayersPriceInGold = (storeItem.PurchaseOptions.FirstOrDefault((Client_PurchaseOption x) => x.CurrencyType == Client_PurchaseCurrencyType.Gold)?.Price ?? 0),
					PlayersPriceInRMT = storeItem.LocalizedPrice
				};
				list.Add(item);
			}
		}
		_biLogger.Send(ClientBusinessEventType.StoreItemsSeen, new StoreItemsSeen
		{
			EventTime = DateTime.UtcNow,
			StoreItems = list,
			StoreTab = CurrentTab.ToString()
		});
	}

	private void ShowPrizeWallUnlock()
	{
		StoreItem prizeWallUnlockListing = _prizeWallDataProvider.GetPrizeWallUnlockListing(_storePrizeWall.Id);
		if (prizeWallUnlockListing != null)
		{
			_prizeWallLockedState.SetPrizeWallToUnlock(prizeWallUnlockListing);
			_prizeWallLockedState.PurchaseOptionClicked += delegate(StoreItem i, Client_PurchaseCurrencyType c)
			{
				OnStoreItemPurchaseOptionClicked(i, c, ExecutePurchase);
			};
		}
		else
		{
			_prizeWallLockedState.UpdateActiveUnlockWidget(active: false);
		}
	}

	public void LayoutItemsToDisplay(StoreTabType tabType, List<StoreItem> itemsToDisplay, bool reIntro)
	{
		foreach (var (data, storeItemBase2) in _pooledItems)
		{
			if (reIntro || !itemsToDisplay.Exists(data, (StoreItem x, string k) => x.Id == k))
			{
				storeItemBase2.gameObject.UpdateActive(active: false);
			}
		}
		foreach (StoreItemBase spawnedPackStoreItem in spawnedPackStoreItems)
		{
			StoreDisplayUtils.DespawnPackWidget(_storeObjectPool, spawnedPackStoreItem);
		}
		spawnedPackStoreItems.Clear();
		_poolModified = false;
		_itemDisplayQueue.Clear();
		foreach (StoreItem item2 in itemsToDisplay)
		{
			StoreItemDisplayQueueEntry item = new StoreItemDisplayQueueEntry
			{
				StoreTabType = tabType,
				StoreItem = item2
			};
			_itemDisplayQueue.Enqueue(item);
		}
		if (_confirmationModal != null && _confirmationModal.gameObject.activeSelf)
		{
			_confirmationModal.Close();
		}
		_noItemsAvailableBanner.gameObject.UpdateActive(itemsToDisplay.Count == 0);
	}

	private void DoLayoutItemsToDisplay()
	{
		StoreItem storeItem = null;
		if (CurrentTab == StoreTabType.DailyDeals)
		{
			storeItem = (from x in Store.Sales
				where x.HasRemainingPurchases
				orderby x.SaleIndex descending
				select x).FirstOrDefault();
		}
		IEnumerator enumerator = null;
		_purchasableSaleItems = 0;
		int num = (_enableAsyncLoading ? _maxDisplayItemsToLoadPerFrame : _itemDisplayQueue.Count);
		for (int num2 = 0; num2 < num; num2++)
		{
			if (!_itemDisplayQueue.TryDequeue(out var result))
			{
				break;
			}
			StoreItem storeItem2 = result.StoreItem;
			StoreTabType storeTabType = result.StoreTabType;
			StoreItemBase orCreateItemWidget = GetOrCreateItemWidget(storeItem2);
			if (orCreateItemWidget != null)
			{
				if (_poolModified)
				{
					orCreateItemWidget.transform.SetAsLastSibling();
				}
				else
				{
					StoreSortUtils.ResyncSiblingOrder(orCreateItemWidget, _storeItems);
				}
				switch (storeTabType)
				{
				case StoreTabType.Featured:
					switch (storeItem2.StoreSection)
					{
					case EStoreSection.Gems:
						orCreateItemWidget.ShowBrowseButton("MainNav/Store/BrowseGems");
						break;
					case EStoreSection.Packs:
						orCreateItemWidget.ShowBrowseButton("MainNav/Store/BrowsePacks");
						break;
					default:
						orCreateItemWidget.HideBrowseButton();
						break;
					}
					if (storeItem2.SaleIndex != -1)
					{
						orCreateItemWidget.SetFeatureCalloutText("MainNav/Store/FeaturedSale_Callout");
						orCreateItemWidget.SetDealsButton("MainNav/Store/Browse_Deals", _assetLookupSystem, delegate
						{
							ShowTab(_dailyDealsTab);
						});
						orCreateItemWidget.SetItem(storeItem2, allowStoreTags: false);
						orCreateItemWidget.SetDailyDealOverride(useDailyDealOverride: true);
					}
					break;
				case StoreTabType.DailyDeals:
					if (storeItem2 == storeItem)
					{
						orCreateItemWidget.SetItem(storeItem2);
						orCreateItemWidget.SetDailyDealOverride(useDailyDealOverride: true);
					}
					orCreateItemWidget.SetFeatureCalloutText(null);
					break;
				case StoreTabType.PrizeWall:
					orCreateItemWidget.SetAllowInput(_storePrizeWallUnlocked);
					break;
				default:
					if (storeItem2.StoreSection == EStoreSection.CardSkins)
					{
						if (CurrentTab != StoreTabType.DailyDeals)
						{
							orCreateItemWidget.ShowBrowseButton("MainNav/Store/BrowseCardStyles");
						}
					}
					else
					{
						orCreateItemWidget.HideBrowseButton();
					}
					break;
				}
				if (_zoomHandler != null)
				{
					orCreateItemWidget.SetZoomHandler(_zoomHandler, _cardDatabase, _cardViewBuilder);
				}
				if (storeItem2.HasRemainingPurchases)
				{
					_purchasableSaleItems++;
				}
				if (!string.IsNullOrEmpty(_highlightItemId) && storeItem2.Id == _highlightItemId)
				{
					enumerator = StoreDisplayUtils.Coroutine_HighlightItem(orCreateItemWidget, _storeButtonLayoutGroup, _highlightItemId);
				}
			}
			else
			{
				_logger.Warn("[Store] Could not create widget for " + storeItem2.Id);
			}
		}
		if (enumerator != null)
		{
			StartCoroutine(enumerator);
		}
	}

	private StoreItemBase GetOrCreateItemWidget(StoreItem item)
	{
		StoreItemBase storeItemBase;
		if (item.ListingType == EListingType.Booster || item.IsSalesPack)
		{
			storeItemBase = CreateItemWidget(item);
			StoreDisplayUtils.RefreshPooledPack(item, storeItemBase, _storeButtonLayoutGroup.transform, _assetLookupSystem, _logger);
			return storeItemBase;
		}
		if (item.ListingType != EListingType.Booster && _pooledItems.TryGetValue(item.Id, out storeItemBase))
		{
			storeItemBase.gameObject.UpdateActive(active: true);
			storeItemBase.SetPurchaseButtons(item, _assetLookupSystem);
			storeItemBase.SetItem(item);
			BundlePayload bundlePayload = StoreDisplayUtils.StorePayloadForFlavor<BundlePayload>(item.PrefabIdentifier, _assetLookupSystem);
			StoreDisplayUtils.SetBackgroundColor(bundlePayload, storeItemBase);
			StoreDisplayUtils.UpdateCardViews(storeItemBase.ItemDisplay, item);
			if (StoreDisplayUtils.IsPreorder(item))
			{
				StoreDisplayUtils.UpdatePreOrderItem(item, storeItemBase, _assetLookupSystem);
			}
			else
			{
				if (bundlePayload != null)
				{
					StoreDisplayUtils.UpdateItemLoc(item, storeItemBase, _assetLookupSystem, _logger);
					storeItemBase.SetTimer(item.ExpireTime);
					StoreDisplayUtils.RefreshPooledPack(item, storeItemBase, _storeButtonLayoutGroup.transform, _assetLookupSystem, _logger);
				}
				if (!item.HasRemainingPurchases && CurrentTab == StoreTabType.DailyDeals)
				{
					StoreDisplayUtils.UpdatePurchaseButton(item, storeItemBase, _assetLookupSystem);
				}
			}
			if (item.ListingType == EListingType.PrizeWall && storeItemBase.ItemDisplay == null)
			{
				StoreItemDisplay storeItemDisplay = UnityEngine.Object.Instantiate(_prizeWallStoreItemModel);
				if (_storePrizeWall.CurrencyCustomTokenId != null)
				{
					storeItemDisplay.SetBackgroundSprite(PrizeWallUtils.GetTokenImagePath(_assetLookupSystem, _storePrizeWall.CurrencyCustomTokenId));
				}
				storeItemBase.AttachItemDisplay(storeItemDisplay);
			}
			return storeItemBase;
		}
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		StoreItemBase storeItemBase2 = CreateItemWidget(item);
		stopwatch.Stop();
		_logger.Info($"Creating item for {item} took {TICKS_TO_MS(stopwatch.ElapsedTicks):.00}ms");
		if (storeItemBase2 != null)
		{
			_pooledItems[item.Id] = storeItemBase2;
			_poolModified = true;
			storeItemBase2.transform.SetParent(_storeButtonLayoutGroup.transform, worldPositionStays: false);
			if (!item.HasRemainingPurchases && CurrentTab == StoreTabType.DailyDeals)
			{
				StoreDisplayUtils.UpdatePurchaseButton(item, storeItemBase2, _assetLookupSystem);
			}
		}
		return storeItemBase2;
	}

	private static List<StoreItem> GetItemsToDisplay(StoreManager store, StoreTabType tabType, StoreSetFilterToggles setFilters, TMP_Dropdown storeFilterDropdown)
	{
		return StoreTabLogic.LogicForTab(tabType).GetItemsToDisplay(setFilters, storeFilterDropdown).Where(delegate(StoreItem item)
		{
			bool flag = StoreDisplayUtils.IsPreorder(item);
			SetMasteryDataProvider setMasteryDataProvider = Pantry.Get<SetMasteryDataProvider>();
			if (item.SubType == "RewardTierUpgrade" && setMasteryDataProvider.PlayerHitPremiumRewardTier(setMasteryDataProvider.CurrentBpName))
			{
				return false;
			}
			if (!item.HasRemainingPurchases && !flag && tabType != StoreTabType.DailyDeals)
			{
				return false;
			}
			return item.Enabled ? true : false;
		})
			.ToList();
	}

	private void TryActivateStoreItemFilterDropdown(StoreTabType tabType, StoreTabLogic logicForTab)
	{
		logicForTab.ActivateStoreFilterDropdown(_storeItemFilterDropdown, delegate
		{
			List<StoreItem> itemsToDisplay = logicForTab.GetItemsToDisplay(_setFilters, _storeItemFilterDropdown);
			LayoutItemsToDisplay(tabType, itemsToDisplay, reIntro: false);
		});
	}

	private StoreItemBase CreateItemWidget(StoreItem item)
	{
		switch (item.ListingType)
		{
		case EListingType.Avatar:
			return CreateAvatarWidget(item);
		case EListingType.Booster:
			return CreateBoosterPackWidget(item);
		case EListingType.SetMastery:
		case EListingType.Bundle:
			return CreateBundleWidget(item);
		case EListingType.Card:
			return CreateCardWidget(item);
		case EListingType.Economic:
			return CreateGemWidget(item);
		case EListingType.Pet:
			return CreatePetWidget(item);
		case EListingType.Sleeve:
			return CreateSleeveWidget(item);
		case EListingType.ArtStyle:
			return CreateCardStyleWidget(item);
		case EListingType.PreconDeck:
			return CreateDeckWidget(item);
		case EListingType.PrizeWall:
			return CreatePrizeWallWidget(item);
		default:
			return null;
		}
	}

	private StoreItemBase CreatePetWidget(StoreItem item)
	{
		return StoreWidgetUtils.CreatePetWidget(item, ExecutePurchase, OnStoreItemPurchaseOptionClicked, _assetLookupSystem, _storeItemBasePrefab, _petItemModel, _clientLocProvider);
	}

	private StoreItemBase CreateBoosterPackWidget(StoreItem item)
	{
		StoreItemBase storeItemBase = StoreWidgetUtils.CreateBoosterPackWidget(item, ExecutePurchase, OnStoreItemPurchaseOptionClicked, _assetLookupSystem, _assetTracker, _storeObjectPool, _storeItemBaseWidePrefab, _storeItemBasePrefab, _setMetadataProvider, spawnedPackStoreItems);
		storeItemBase.BrowseButtonClicked += delegate(StoreItem i)
		{
			_setFilters.SelectForGivenSet(i.SubType);
			ShowTab(_packsTab);
		};
		return storeItemBase;
	}

	private StoreItemBase CreateBundleWidget(StoreItem item)
	{
		return StoreWidgetUtils.CreateBundleWidget(item, ExecutePurchase, OnStoreItemPurchaseOptionClicked, _assetLookupSystem, _assetTracker, _storeItemBaseWidePrefab, _storeItemBasePrefab, _storeButtonLayoutGroup, _logger);
	}

	private StoreItemBase CreateCardWidget(StoreItem item)
	{
		return StoreWidgetUtils.CreateCardWidget(item, ExecutePurchase, OnStoreItemPurchaseOptionClicked, _assetLookupSystem, _cardStoreItemModel, _storeItemBaseWidePrefab, _storeItemBasePrefab, _cardDatabase, _cardViewBuilder, _logger, allowStoreTags: false);
	}

	private StoreItemBase CreateCardStyleWidget(StoreItem item)
	{
		bool allowStoreTags = CurrentTab != StoreTabType.Featured || item.SaleIndex == -1;
		StoreItemBase storeItemBase = StoreWidgetUtils.CreateCardStyleWidget(item, ExecutePurchase, OnStoreItemPurchaseOptionClicked, _assetLookupSystem, _cardStyleStoreItemModel, _storeItemBaseWidePrefab, _storeItemBasePrefab, _cardDatabase, _cardViewBuilder, _logger, allowStoreTags);
		storeItemBase.BrowseButtonClicked += delegate
		{
			NavigateToCollectionStyles();
		};
		return storeItemBase;
	}

	private StoreItemBase CreateSleeveWidget(StoreItem item)
	{
		bool allowStoreTags = CurrentTab != StoreTabType.Featured || item.SaleIndex == -1;
		return StoreWidgetUtils.CreateSleeveWidget(item, ExecutePurchase, OnStoreItemPurchaseOptionClicked, _assetLookupSystem, _sleeveStoreItemModel, _storeItemBaseWidePrefab, _storeItemBasePrefab, _cardDatabase, _cardViewBuilder, allowStoreTags);
	}

	private StoreItemBase CreatePrizeWallWidget(StoreItem item)
	{
		if (_storePrizeWall == null)
		{
			return null;
		}
		bool allowStoreTags = CurrentTab != StoreTabType.Featured || item.SaleIndex == -1;
		return StoreWidgetUtils.CreatePrizeWallWidget(item, ExecutePurchase, OnStoreItemPurchaseOptionClicked, _assetLookupSystem, _prizeWallStoreItemModel, _storeItemBasePrefab, _storePrizeWall, allowStoreTags, _logger);
	}

	private StoreItemBase CreateDeckWidget(StoreItem item)
	{
		StoreDisplayPreconDeck itemDisplay = UnityEngine.Object.Instantiate(_deckStoreItemModel);
		Action<StoreItem, Client_PurchaseCurrencyType> action = delegate(StoreItem i, Client_PurchaseCurrencyType c)
		{
			OnStoreItemPurchaseOptionClicked(i, c, ExecutePurchase);
		};
		itemDisplay.Init(item, _cardDatabase, _cardViewBuilder, action);
		StoreItemBase newButton = UnityEngine.Object.Instantiate(itemDisplay.WideBase ? _storeItemBaseWidePrefab : _storeItemBasePrefab);
		newButton.SetPurchaseButtons(item, _assetLookupSystem);
		newButton.SetItem(item);
		newButton.SetLabelText(item.LocData.LocKey);
		string description = Pantry.Get<IPreconDeckServiceWrapper>().GetPreconDeck(new Guid(item.Skus[0].TreasureItem.ReferenceId)).Summary.Description;
		if (!string.IsNullOrWhiteSpace(description))
		{
			newButton.SetDescriptionText(description);
		}
		newButton.SetDescriptionActiveIfNotEmpty(isActive: false);
		_confirmationModal.OnOpened += OnModalOpened;
		_confirmationModal.OnClosed += OnModalClosed;
		newButton.WhenDestroyed += CleanupModalCallbacks;
		if (item.LocData?.TooltipLocKey != null)
		{
			newButton.SetTooltipText(item.LocData.TooltipLocKey);
		}
		newButton.AttachItemDisplay(itemDisplay);
		newButton.PurchaseOptionClicked += action;
		return newButton;
		void CleanupModalCallbacks()
		{
			_confirmationModal.OnOpened -= OnModalOpened;
			_confirmationModal.OnClosed -= OnModalClosed;
			newButton.WhenDestroyed -= CleanupModalCallbacks;
		}
		void OnModalClosed()
		{
			newButton.SetDescriptionActiveIfNotEmpty(isActive: false);
			itemDisplay.PurchaseConfirmationVisibilityChanged(confirmationShowing: false);
		}
		void OnModalOpened()
		{
			newButton.SetDescriptionActiveIfNotEmpty(isActive: true);
			itemDisplay.PurchaseConfirmationVisibilityChanged(confirmationShowing: true);
		}
	}

	private StoreItemBase CreateAvatarWidget(StoreItem item)
	{
		bool allowStoreTags = CurrentTab != StoreTabType.Featured || item.SaleIndex == -1;
		return StoreWidgetUtils.CreateAvatarWidget(item, ExecutePurchase, OnStoreItemPurchaseOptionClicked, _assetLookupSystem, _avatarStoreItemModel, _storeItemBaseWidePrefab, _storeItemBasePrefab, allowStoreTags);
	}

	private StoreItemBase CreateGemWidget(StoreItem item)
	{
		StoreItemBase storeItemBase = StoreWidgetUtils.CreateGemWidget(item, ExecutePurchase, OnStoreItemPurchaseOptionClicked, _assetLookupSystem, _assetTracker, _storeItemBaseWidePrefab, _storeItemBasePrefab);
		storeItemBase.BrowseButtonClicked += delegate
		{
			ShowTab(_gemsTab);
		};
		return storeItemBase;
	}

	private static float TICKS_TO_MS(long ticks)
	{
		return (float)ticks / (float)Stopwatch.Frequency * 1000f;
	}

	private void BI_TrackStoreTabChanged(string toTab, string fromScene, string humanContext = null)
	{
		_biLogger.Send(ClientBusinessEventType.StoreTabChange, new StoreTabChange
		{
			EventTime = DateTime.UtcNow,
			FromTab = CurrentTab.ToString(),
			FromScene = fromScene,
			ToTab = toTab
		});
	}
}
