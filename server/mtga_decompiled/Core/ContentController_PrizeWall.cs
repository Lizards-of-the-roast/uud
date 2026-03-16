using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using AssetLookupTree.Payloads.Store;
using Assets.Core.Shared.Code;
using Core.Code.PrizeWall;
using Core.MainNavigation.RewardTrack;
using Core.Meta.MainNavigation.Store;
using Core.Meta.MainNavigation.Store.Utils;
using MovementSystem;
using Pooling;
using SharedClientCore.SharedClientCore.Code.Providers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Wizards.Arena.Client.Logging;
using Wizards.GeneralUtilities.ObjectCommunication;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Logging;
using Wizards.Mtga.Platforms;
using Wizards.Unification.Models.Mercantile;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.CustomInput;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;

public class ContentController_PrizeWall : NavContentController
{
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

	[Header("Store Item Prefabs")]
	[SerializeField]
	private StoreItemBase _storeItemBasePrefab;

	[SerializeField]
	private StoreItemBase _storeItemBaseWidePrefab;

	[Header("Text")]
	[SerializeField]
	private Localize _title;

	[SerializeField]
	private Localize _noItemsAvailableBanner;

	[Header("Filters")]
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
	private GameObject _handheldLegalText;

	[SerializeField]
	private GameObject _prizeWallLockedCurrencyPosition;

	[SerializeField]
	private GameObject _prizeWallDefaultCurrencyPosition;

	[SerializeField]
	private CustomButton _prizeWallBackButton;

	private bool _titleDisabledForRewards;

	private Wizards.Arena.Client.Logging.Logger _logger;

	private CardSkinDatabase _cardSkinDB;

	private ICardRolloverZoom _zoomHandler;

	private IBILogger _biLogger;

	private string _highlightItemId;

	[SerializeField]
	private UnityEvent _packsTabSelected;

	private bool _readyToShow;

	private AssetLookupSystem _assetLookupSystem;

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	private Coroutine _refreshAllCoroutine;

	private SettingsMenuHost _settingsMenuHost;

	private ConnectionManager _connectionManager;

	private ISetMetadataProvider _setMetadataProvider;

	private Client_PrizeWall _prizeWall;

	private PrizeWallContext _prizeWallContext;

	private PrizeWallDataProvider _prizeWallDataProvider;

	private AssetLoader.AssetTracker<Sprite> _prizeWallBackgroundImageSpriteTracker;

	private Dictionary<string, StoreItemBase> _pooledItems = new Dictionary<string, StoreItemBase>();

	private List<StoreItemBase> spawnedPackStoreItems = new List<StoreItemBase>();

	private readonly AssetTracker _assetTracker = new AssetTracker();

	private IClientLocProvider _clientLocProvider;

	public override NavContentType NavContentType => NavContentType.PrizeWall;

	private StoreManager Store => WrapperController.Instance.Store;

	public override bool IsReadyToShow => _readyToShow;

	private IUnityObjectPool _storeObjectPool { get; set; }

	public void SetPrizeWallData(string prizeWallId, PrizeWallContext prizeWallContext)
	{
		_prizeWallDataProvider = Pantry.Get<PrizeWallDataProvider>();
		Client_PrizeWall prizeWallById = _prizeWallDataProvider.GetPrizeWallById(prizeWallId);
		if (prizeWallById.AppearsAsStoreTab)
		{
			SimpleLog.LogError("Store prize walls can not be displayed by standalone prize wall content controller");
			return;
		}
		_prizeWall = prizeWallById;
		TimeSpan timeSpan = _prizeWall.EarnStopDate - ServerGameTime.GameTime;
		TimeSpan timeSpan2 = _prizeWall.SpendStopDate - ServerGameTime.GameTime;
		TimeSpan timeSpan3;
		string key;
		if (_prizeWall.CurrencyCustomTokenId == null)
		{
			timeSpan3 = timeSpan2;
			key = "MainNav/Store/PrizeWall/TabTimer";
		}
		else if (timeSpan > TimeSpan.Zero)
		{
			timeSpan3 = timeSpan;
			key = "MainNav/Store/PrizeWall/EarnSubHeader";
		}
		else
		{
			timeSpan3 = timeSpan2;
			key = "MainNav/Store/PrizeWall/SpendSubHeader";
		}
		_textPrizeWallHeader.SetText(_prizeWall.NameLocKey);
		_textPrizeWallSubHeader.SetText(key, new Dictionary<string, string>
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
		_prizeWallContext = prizeWallContext;
	}

	public bool IsMasteryPrizeWall(SetMasteryDataProvider masteryProvider)
	{
		if (!(masteryProvider.CurrentPrizeWallId == _prizeWall.Id))
		{
			return masteryProvider.PreviousPrizeWallId == _prizeWall.Id;
		}
		return true;
	}

	public void SetContext(StoreTabType context)
	{
		if (IsReadyToShow && base.gameObject.activeInHierarchy)
		{
			ShowTab();
		}
	}

	public void GoToItem(string id)
	{
		StoreItem storeItem = (from kvp in Store.StoreListings
			where string.Equals(kvp.Key, id, StringComparison.OrdinalIgnoreCase)
			select kvp.Value).FirstOrDefault();
		if (storeItem != null)
		{
			_highlightItemId = storeItem.Id;
		}
	}

	private void Awake()
	{
		_prizeWallDataProvider = Pantry.Get<PrizeWallDataProvider>();
		if (WrapperController.Instance != null)
		{
			IAccountClient accountClient = WrapperController.Instance.AccountClient;
			if (accountClient != null && accountClient.AccountInformation?.HasRole_Debugging() == true)
			{
				_logger = WrapperController.Instance.UnityCrossThreadLogger;
				goto IL_008c;
			}
		}
		_logger = new UnityLogger("Store", LoggerLevel.Emergency);
		LoggerManager.Register(_logger);
		goto IL_008c;
		IL_008c:
		_connectionManager = Pantry.Get<ConnectionManager>();
		ConnectionManager connectionManager = _connectionManager;
		connectionManager.OnFdReconnected = (Action)Delegate.Combine(connectionManager.OnFdReconnected, new Action(OnFdReconnected));
		_prizeWallBackButton.OnClick.AddListener(OnPrizeWallBackButton);
	}

	public void Init(AssetLookupSystem assetLookupSystem, ICardRolloverZoom zoomHandler, IBILogger biLogger, IGreLocProvider greLocalizationManager, IClientLocProvider clientLocalizationManager, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, SettingsMenuHost settingsMenuHost, ISetMetadataProvider setMetadataProvider, Transform contentParent)
	{
		_biLogger = biLogger;
		_assetLookupSystem = assetLookupSystem;
		_cardDatabase = cardDatabase;
		_cardViewBuilder = cardViewBuilder;
		_settingsMenuHost = settingsMenuHost;
		_setMetadataProvider = setMetadataProvider;
		_clientLocProvider = Pantry.Get<IClientLocProvider>();
		string prefabPath = _assetLookupSystem.GetPrefabPath<StoreConfirmationModalPrefab, StoreConfirmationModal>();
		_confirmationModal = AssetLoader.Instantiate<StoreConfirmationModal>(prefabPath, contentParent);
		_confirmationModal.Initialize(cardDatabase, greLocalizationManager, clientLocalizationManager, _cardViewBuilder, assetLookupSystem);
		_confirmationModal.OnOpened += OnConfirmationOpened;
		_confirmationModal.OnClosed += OnConfirmationClosed;
		_zoomHandler = zoomHandler;
		_storeObjectPool = PlatformContext.CreateUnityPool("Store", keepAlive: false, base.transform, new SplineMovementSystem());
	}

	private void OnDestroy()
	{
		if (_confirmationModal != null)
		{
			_confirmationModal.OnOpened -= OnConfirmationOpened;
			_confirmationModal.OnClosed -= OnConfirmationClosed;
		}
		_assetTracker.Cleanup();
		_cardSkinDB?.Dispose();
		ConnectionManager connectionManager = _connectionManager;
		connectionManager.OnFdReconnected = (Action)Delegate.Remove(connectionManager.OnFdReconnected, new Action(OnFdReconnected));
		_prizeWallBackButton.OnClick.RemoveListener(OnPrizeWallBackButton);
		_storeObjectPool.Destroy();
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
		foreach (StoreItemBase value in _pooledItems.Values)
		{
			value.gameObject.UpdateActive(active: false);
		}
		_noItemsAvailableBanner.gameObject.UpdateActive(active: false);
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
			ShowTab();
		}
		_readyToShow = true;
	}

	public override void OnBeginClose()
	{
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
	}

	public override void OnHandheldBackButton()
	{
		if (_confirmationModal != null && _confirmationModal.gameObject.activeSelf)
		{
			_confirmationModal.Close();
		}
		else
		{
			base.OnHandheldBackButton();
		}
	}

	private void UpdateLocOnCurrentTab()
	{
		ShowTab();
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
		ShowTab();
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

	private void OnInventoryUpdated(ClientInventoryUpdateReportItem update)
	{
		RefreshAll();
	}

	public void OnPrizeWallBackButton()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_back, base.gameObject);
		switch (_prizeWallContext.BackContentType)
		{
		case NavContentType.EventLanding:
			SceneLoader.GetSceneLoader().GoToEventScreen(_prizeWallContext.EventContext);
			break;
		case NavContentType.RewardTrack:
			SceneLoader.GetSceneLoader().GoToProgressionTrackScene(_prizeWallContext.MasteryPassContext, "From prize wall back button");
			break;
		default:
			SceneLoader.GetSceneLoader().GoToLanding(new HomePageContext());
			break;
		}
	}

	private void OnPurchaseSuccessful(StoreItem item)
	{
		SetMasteryDataProvider setMasteryDataProvider = Pantry.Get<SetMasteryDataProvider>();
		if (IsMasteryPrizeWall(setMasteryDataProvider))
		{
			StartCoroutine(setMasteryDataProvider.Refresh());
		}
		if (!item.HasRemainingPurchases && _pooledItems.TryGetValue(item.Id, out var value))
		{
			if (StoreDisplayUtils.IsPreorder(item))
			{
				StoreDisplayUtils.UpdatePreOrderItem(item, value, _assetLookupSystem);
			}
			else
			{
				value.gameObject.UpdateActive(active: false);
			}
		}
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
		WrapperController.EnableUniqueLoadingIndicator("PrizeWall.RefreshInventory");
		bool success = false;
		yield return Store.RefreshStoreDataYield(delegate(bool b)
		{
			success = b;
		});
		WrapperController.DisableUniqueLoadingIndicator("PrizeWall.RefreshInventory");
		if (!success)
		{
			SceneLoader.GetSceneLoader().SystemMessages.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Store_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Store_Products_Get_Error_Text"), delegate
			{
				SceneLoader.GetSceneLoader().GoToLanding(new HomePageContext());
			});
		}
		else
		{
			ShowTab();
		}
	}

	private void ExecutePurchase(StoreItem item, Client_PurchaseCurrencyType currencyType)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_purchase_pack, base.gameObject);
		StartCoroutine(Store.PurchaseItemYield(item, currencyType, OnPurchaseSuccessful, 1, (currencyType == Client_PurchaseCurrencyType.CustomToken) ? item.PurchaseOptions.First((Client_PurchaseOption x) => x.CurrencyType == Client_PurchaseCurrencyType.CustomToken).CurrencyId : null));
	}

	private void OnStoreItemPurchaseOptionClicked(StoreItem item, Client_PurchaseCurrencyType currencyType, Action<StoreItem, Client_PurchaseCurrencyType> executePurchase)
	{
		StoreItemBase itemWidget = GetOrCreateItemWidget(item);
		_confirmationModal.SetStoreItem(item, itemWidget, currencyType, delegate(StoreItem i, Client_PurchaseCurrencyType c)
		{
			OnStoreItemConfirmationClicked(i, c, executePurchase);
		});
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
		if (currencyType == Client_PurchaseCurrencyType.CustomToken)
		{
			Client_PurchaseOption client_PurchaseOption = item.PurchaseOptions.First((Client_PurchaseOption x) => x.CurrencyType == Client_PurchaseCurrencyType.CustomToken);
			int price = client_PurchaseOption.Price;
			int value = 0;
			inventoryManager.Inventory?.CustomTokens.TryGetValue(client_PurchaseOption.CurrencyId, out value);
			if (value >= price)
			{
				executePurchase(item, currencyType);
			}
		}
		else
		{
			SimpleLog.LogError($"Attempting to buy prize wall items with currency: {currencyType}");
		}
	}

	public void Update()
	{
		float y = CustomInputModule.GetMouseScroll().y;
		if (y != 0f && !_settingsMenuHost.IsGameObjectActive())
		{
			Scrollbar.value += ((y < 0f) ? 0.1f : (-0.1f));
		}
	}

	private void ShowTab()
	{
		Scrollbar.value = 0f;
		_noItemsAvailableBanner.gameObject.UpdateActive(active: false);
		SceneLoader sceneLoader = SceneLoader.GetSceneLoader();
		if (_prizeWallBackgroundImageSpriteTracker == null)
		{
			_prizeWallBackgroundImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("PrizeWallBackgroundImageSprite");
		}
		AssetLoaderUtils.TrySetSprite(_prizeWallBackgroundImage, _prizeWallBackgroundImageSpriteTracker, PrizeWallUtils.GetBackgroundImagePath(_assetLookupSystem, _prizeWall.Id));
		_prizeWallBackgroundImage.gameObject.UpdateActive(active: true);
		if (sceneLoader != null)
		{
			sceneLoader.DisableBonusPackProgressMeter();
		}
		List<StoreItem> itemsToDisplay = GetItemsToDisplay(Store);
		LayoutItemsToDisplay(itemsToDisplay, reIntro: false);
		_currencyPrizeWall.SetCurrency(_prizeWallDataProvider, _prizeWall, _assetLookupSystem);
		if (_prizeWall.CurrencyCustomTokenId != null && Pantry.Get<ICustomTokenProvider>().TokenDefinitions.TryGetValue(_prizeWall.CurrencyCustomTokenId, out var value))
		{
			_currencyPrizeWall.UpdateCurrencyCountTooltip(value.HeaderLocKey);
		}
	}

	public void LayoutItemsToDisplay(List<StoreItem> itemsToDisplay, bool reIntro)
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
		IEnumerator enumerator3 = null;
		foreach (StoreItem item in itemsToDisplay)
		{
			StoreItemBase orCreateItemWidget = GetOrCreateItemWidget(item);
			if (orCreateItemWidget != null)
			{
				orCreateItemWidget.transform.SetAsLastSibling();
				if (item.StoreSection == EStoreSection.CardSkins)
				{
					orCreateItemWidget.ShowBrowseButton("MainNav/Store/BrowseCardStyles");
				}
				else
				{
					orCreateItemWidget.HideBrowseButton();
				}
				if (_zoomHandler != null)
				{
					orCreateItemWidget.SetZoomHandler(_zoomHandler, _cardDatabase, _cardViewBuilder);
				}
				if (!string.IsNullOrEmpty(_highlightItemId) && item.Id == _highlightItemId)
				{
					enumerator3 = StoreDisplayUtils.Coroutine_HighlightItem(orCreateItemWidget, _storeButtonLayoutGroup, _highlightItemId);
				}
			}
			else
			{
				_logger.Warn("[Store] Could not create widget for " + item.Id);
			}
		}
		_noItemsAvailableBanner.gameObject.UpdateActive(itemsToDisplay.Count == 0);
		if (_confirmationModal != null && _confirmationModal.gameObject.activeSelf)
		{
			_confirmationModal.Close();
		}
		if (enumerator3 != null)
		{
			StartCoroutine(enumerator3);
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
			else if (bundlePayload != null)
			{
				StoreDisplayUtils.UpdateItemLoc(item, storeItemBase, _assetLookupSystem, _logger);
				storeItemBase.SetTimer(item.ExpireTime);
				StoreDisplayUtils.RefreshPooledPack(item, storeItemBase, _storeButtonLayoutGroup.transform, _assetLookupSystem, _logger);
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
			storeItemBase2.transform.SetParent(_storeButtonLayoutGroup.transform, worldPositionStays: false);
		}
		return storeItemBase2;
	}

	private List<StoreItem> GetItemsToDisplay(StoreManager store)
	{
		if (_prizeWall == null)
		{
			return new List<StoreItem>();
		}
		return (from x in store.PrizeWall
			where x?.PrizeWallData?.AssociatedPrizeWall == _prizeWall.Id && (x == null || x.ListingType != EListingType.PrizeWall)
			orderby x.SortIndex descending
			select x).ToList().Where(delegate(StoreItem item)
		{
			bool flag = StoreDisplayUtils.IsPreorder(item);
			SetMasteryDataProvider setMasteryDataProvider = Pantry.Get<SetMasteryDataProvider>();
			if (item.SubType == "RewardTierUpgrade" && setMasteryDataProvider.PlayerHitPremiumRewardTier(setMasteryDataProvider.CurrentBpName))
			{
				return false;
			}
			if (!item.HasRemainingPurchases && !flag)
			{
				return false;
			}
			return item.Enabled ? true : false;
		}).ToList();
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
		Action<StoreItem, Client_PurchaseCurrencyType> executePurchase = ExecutePurchase;
		Action<StoreItem, Client_PurchaseCurrencyType, Action<StoreItem, Client_PurchaseCurrencyType>> onStoreItemPurchaseOptionClicked = OnStoreItemPurchaseOptionClicked;
		StoreItemBase storeItemBase = StoreWidgetUtils.CreateBoosterPackWidget(item, executePurchase, onStoreItemPurchaseOptionClicked, _assetLookupSystem, _assetTracker, _storeObjectPool, _storeItemBaseWidePrefab, _storeItemBasePrefab, _setMetadataProvider, spawnedPackStoreItems);
		storeItemBase.BrowseButtonClicked += delegate
		{
			ShowTab();
		};
		return storeItemBase;
	}

	private StoreItemBase CreateBundleWidget(StoreItem item)
	{
		return StoreWidgetUtils.CreateBundleWidget(item, ExecutePurchase, OnStoreItemPurchaseOptionClicked, _assetLookupSystem, _assetTracker, _storeItemBaseWidePrefab, _storeItemBasePrefab, _storeButtonLayoutGroup, _logger);
	}

	private StoreItemBase CreateCardWidget(StoreItem item)
	{
		return StoreWidgetUtils.CreateCardWidget(item, ExecutePurchase, OnStoreItemPurchaseOptionClicked, _assetLookupSystem, _cardStoreItemModel, _storeItemBaseWidePrefab, _storeItemBasePrefab, _cardDatabase, _cardViewBuilder, _logger, allowStoreTags: true);
	}

	private StoreItemBase CreateCardStyleWidget(StoreItem item)
	{
		StoreItemBase storeItemBase = StoreWidgetUtils.CreateCardStyleWidget(item, ExecutePurchase, OnStoreItemPurchaseOptionClicked, _assetLookupSystem, _cardStyleStoreItemModel, _storeItemBaseWidePrefab, _storeItemBasePrefab, _cardDatabase, _cardViewBuilder, _logger, allowStoreTags: true);
		storeItemBase.BrowseButtonClicked += delegate
		{
			NavigateToCollectionStyles();
		};
		return storeItemBase;
	}

	private StoreItemBase CreateSleeveWidget(StoreItem item)
	{
		return StoreWidgetUtils.CreateSleeveWidget(item, ExecutePurchase, OnStoreItemPurchaseOptionClicked, _assetLookupSystem, _sleeveStoreItemModel, _storeItemBaseWidePrefab, _storeItemBasePrefab, _cardDatabase, _cardViewBuilder, allowStoreTags: true);
	}

	private StoreItemBase CreatePrizeWallWidget(StoreItem item)
	{
		return StoreWidgetUtils.CreatePrizeWallWidget(item, ExecutePurchase, OnStoreItemPurchaseOptionClicked, _assetLookupSystem, _prizeWallStoreItemModel, _storeItemBasePrefab, _prizeWall, allowStoreTags: true, _logger);
	}

	private StoreItemBase CreateAvatarWidget(StoreItem item)
	{
		return StoreWidgetUtils.CreateAvatarWidget(item, ExecutePurchase, OnStoreItemPurchaseOptionClicked, _assetLookupSystem, _avatarStoreItemModel, _storeItemBaseWidePrefab, _storeItemBasePrefab, allowStoreTags: true);
	}

	private StoreItemBase CreateGemWidget(StoreItem item)
	{
		StoreItemBase storeItemBase = StoreWidgetUtils.CreateGemWidget(item, ExecutePurchase, OnStoreItemPurchaseOptionClicked, _assetLookupSystem, _assetTracker, _storeItemBaseWidePrefab, _storeItemBasePrefab);
		storeItemBase.BrowseButtonClicked += delegate
		{
			ShowTab();
		};
		return storeItemBase;
	}

	private static float TICKS_TO_MS(long ticks)
	{
		return (float)ticks / (float)Stopwatch.Frequency * 1000f;
	}
}
