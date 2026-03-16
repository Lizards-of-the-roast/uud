using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.Decks;
using Core.Code.Input;
using Core.Shared.Code.CardFilters;
using GreClient.CardData;
using Pooling;
using SharedClientCore.SharedClientCore.Code.Providers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Platforms;
using Wizards.Mtga.PreferredPrinting;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;

public class DeckBuilderWidget : MonoBehaviour, IFindActionHandler
{
	private bool _isActive;

	private bool _suppressModeChange;

	[SerializeField]
	private Transform _popupRoot;

	[SerializeField]
	private CustomButton _doneButton;

	[SerializeField]
	private CustomButton _backButton;

	[SerializeField]
	private CustomButton _batchCraftingButton;

	[SerializeField]
	private CustomButton _applySkinsButton;

	[SerializeField]
	private Transform _deckListParent;

	[SerializeField]
	private DeckColumnView _deckColumnViewPrefab;

	[SerializeField]
	private Transform _deckColumnParent;

	[SerializeField]
	private CardPoolHolder _poolHolderPrefab;

	[SerializeField]
	private Transform _poolParent;

	[SerializeField]
	private AutoLandsToggle _autoLandsTogglePrefab;

	[SerializeField]
	private AdvancedFiltersView _advancedFiltersPrefab;

	[SerializeField]
	private SearchAndFilterBar _headerPrefab;

	[SerializeField]
	private Transform _headerParent;

	[SerializeField]
	private bool _hideSideboardBeforeFormatSelected;

	[Header("Handheld Feature Toggles")]
	[SerializeField]
	private bool _hideHeaderWhenExpanded;

	[SerializeField]
	private bool _isSideboardExpandedOnly;

	[SerializeField]
	private bool _canUseLargeCardsInColumnView = true;

	[SerializeField]
	private bool _limitSubButtonsInHalfView;

	private AdvancedFiltersView _advancedFiltersView;

	private SearchAndFilterBar _header;

	private DeckListView _deckListView;

	private DeckColumnView _deckColumnView;

	private CardPoolHolder _poolCardHolder;

	private DeckDetailsPopup _detailsPopup;

	private SpecializePopup _specializePopup;

	private AutoLandsToggle _autoLandsToggle;

	private ICardRolloverZoom _zoomHandler;

	private InventoryManager _inventoryManager;

	private CosmeticsProvider _cosmeticsProvider;

	private StoreManager _storeManager;

	private CardBackCatalog _cardBackCatalog;

	private DecksManager _deckManager;

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	private IActionSystem _actionSystem;

	private FormatManager _formatManager;

	private EventManager _eventManager;

	private IBILogger _logger;

	private IAccountClient _accountClient;

	private ISetMetadataProvider _setMetadataProvider;

	private AssetLookupSystem _assetLookupSystem;

	private DeckBuilderModelProvider ModelProvider => Pantry.Get<DeckBuilderModelProvider>();

	private DeckBuilderModel Model => ModelProvider.Model;

	private DeckBuilderVisualsUpdater VisualsUpdater => Pantry.Get<DeckBuilderVisualsUpdater>();

	private CompanionUtil CompanionUtil => Pantry.Get<CompanionUtil>();

	private DeckBuilderCardFilterProvider CardFilterProvider => Pantry.Get<DeckBuilderCardFilterProvider>();

	private bool ListViewShowingSideboard
	{
		get
		{
			return LayoutState.IsListViewSideboarding;
		}
		set
		{
			LayoutState.IsListViewSideboarding = value;
		}
	}

	private bool LargeCardsInPool
	{
		get
		{
			return LayoutState.LargeCardsInPool;
		}
		set
		{
			LayoutState.LargeCardsInPool = value;
		}
	}

	private DeckBuilderMode Mode
	{
		get
		{
			return Context.Mode;
		}
		set
		{
			Context.Mode = value;
		}
	}

	private IReadOnlyCardFilter Filter => CardFilterProvider.Filter;

	private CardFilter PreCraftingFilter => CardFilterProvider.PreCraftingFilter;

	private DeckBuilderContextProvider ContextProvider => Pantry.Get<DeckBuilderContextProvider>();

	private DeckBuilderContext Context => ContextProvider.Context;

	private DeckBuilderLayoutState LayoutState => Pantry.Get<DeckBuilderLayoutState>();

	private bool UseColumnView => LayoutState.LayoutInUse == DeckBuilderLayout.Column;

	private DeckBuilderActionsHandler ActionsHandler => Pantry.Get<DeckBuilderActionsHandler>();

	public event Action DoneClicked;

	public event Action NewDeckButtonClicked;

	private void OnEnable()
	{
		Languages.LanguageChangedSignal.Listeners += ResetLanguage;
		LayoutState.OnLayoutChanged += UseLayoutChanged;
		ContextProvider.OnContextSet += OnContextSet;
		VisualsUpdater.PreOnLayoutUpdated += OnPreLayoutUpdated;
		if ((bool)_specializePopup)
		{
			ModelProvider.CardModifiedInPile += _specializePopup.OnCommanderAdded;
		}
	}

	private void OnDisable()
	{
		Languages.LanguageChangedSignal.Listeners -= ResetLanguage;
		LayoutState.OnLayoutChanged -= UseLayoutChanged;
		ContextProvider.OnContextSet -= OnContextSet;
		VisualsUpdater.PreOnLayoutUpdated -= OnPreLayoutUpdated;
		if ((bool)_specializePopup)
		{
			ModelProvider.CardModifiedInPile -= _specializePopup.OnCommanderAdded;
		}
	}

	public void OnBackButton()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_back, base.gameObject);
	}

	public bool OnHandheldBackButton()
	{
		if (_advancedFiltersView.gameObject.activeSelf)
		{
			_advancedFiltersView.Hide(applyFilters: true);
			return true;
		}
		if (_detailsPopup.IsShowing)
		{
			_detailsPopup.Activate(activate: false);
			return true;
		}
		if (_specializePopup.IsShowing)
		{
			_specializePopup.Activate(activate: false);
			return true;
		}
		return false;
	}

	public void OnNewDeck()
	{
		this.NewDeckButtonClicked?.Invoke();
	}

	public void ResetLanguage()
	{
		Model.ReSort();
		CardFilterProvider.ApplyFilters(FilterValueChangeSource.Miscellaneous);
		if (Context.IsEditingDeck)
		{
			VisualsUpdater.UpdateAllDeckVisuals();
		}
		WrapperDeckBuilder.CacheDeck(Model, Context);
	}

	public void UseLayoutChanged(DeckBuilderLayout layoutToUse)
	{
		if (Context != null && Context.IsEditingDeck)
		{
			if (layoutToUse == DeckBuilderLayout.Column)
			{
				ActionsHandler.HideQuantityAdjust();
			}
			VisualsUpdater.UpdateView();
			UpdateDeckBladeParticles();
		}
	}

	public void SetOnStoreCallback(Action<Action> storeSelected)
	{
		_detailsPopup.OnStoreSelected = storeSelected;
	}

	private void LargeCardsToggle_OnValueChanged(bool value)
	{
		if (LargeCardsInPool != value)
		{
			LargeCardsInPool = value;
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
			CardFilterProvider.ApplyFilters(FilterValueChangeSource.Miscellaneous);
			_poolCardHolder.ScrollToTop();
			MDNPlayerPrefs.SetLargeCardsInPool(_accountClient?.AccountInformation?.PersonaID, value);
		}
	}

	private void CraftingModeToggle_OnValueChanged(bool shouldEnableCraftingMode)
	{
		if (!_suppressModeChange)
		{
			DeckBuilderMode deckBuilderMode = (shouldEnableCraftingMode ? DeckBuilderMode.Crafting : (Context.IsReadOnly ? DeckBuilderMode.ReadOnly : DeckBuilderMode.DeckBuilding));
			if (Mode != deckBuilderMode)
			{
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
				DeckBuilderMode mode = Mode;
				Mode = deckBuilderMode;
				OnModeChanged(mode);
			}
		}
	}

	private void CompanionFilterToggle_OnValueChanged(bool value)
	{
		AudioManager.PlayAudio(value ? WwiseEvents.sfx_ui_generic_click : WwiseEvents.sfx_ui_back, base.gameObject);
		CardFilterProvider.IsCompanionToggleOn = value;
		CardFilterProvider.ApplyFilters(FilterValueChangeSource.Miscellaneous);
	}

	private void OnModeChanged(DeckBuilderMode fromMode)
	{
		if (fromMode == DeckBuilderMode.DeckBuilding)
		{
			CardFilterProvider.SetPreCraftingFilter();
			CardFilterProvider.SetFilter(FilterValueChangeSource.Miscellaneous, CardFilterType.Collection_Collected, value: true);
			CardFilterProvider.SetFilter(FilterValueChangeSource.Miscellaneous, CardFilterType.Collection_Uncollected, value: true);
		}
		else if (Mode == DeckBuilderMode.DeckBuilding && PreCraftingFilter != null)
		{
			CardFilterProvider.SetFilter(FilterValueChangeSource.Miscellaneous, CardFilterType.Collection_Collected, PreCraftingFilter.IsSet(CardFilterType.Collection_Collected));
			CardFilterProvider.SetFilter(FilterValueChangeSource.Miscellaneous, CardFilterType.Collection_Uncollected, PreCraftingFilter.IsSet(CardFilterType.Collection_Uncollected));
		}
		UpdateCollectionFilterToggles();
		CardFilterProvider.ApplyFilters(FilterValueChangeSource.Miscellaneous);
		if (Context.IsEditingDeck)
		{
			VisualsUpdater.UpdateAllDeckVisuals();
		}
		UpdateDeckBladeParticles();
		_suppressModeChange = true;
		_header.CraftingModeToggle.isOn = Mode == DeckBuilderMode.Crafting;
		_suppressModeChange = false;
	}

	public void BatchCraftingButton_OnClick()
	{
		DeckBuilderUtilities.CraftAll(_cardDatabase, Model.GetCardsNeededToFinishDeck(), _inventoryManager, _formatManager, _cardDatabase.GreLocProvider, SystemMessageManager.Instance, Model, _setMetadataProvider);
	}

	public void BatchCraftingButton_OnUpdated()
	{
		bool active = Context.ShowCraftingButtons(_deckManager) && Model.GetCardsNeededToFinishDeck().Count > 0;
		_batchCraftingButton.gameObject.UpdateActive(active);
	}

	private void ApplySkinsButton_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
		ModelProvider.AssignUnassignedCardSkinsToDeck();
		VisualsUpdater.UpdateAllDeckVisuals();
		VisualsUpdater.RefreshPoolView(scrollToTop: false, null);
	}

	private void ApplySkinsButton_OnUpdated()
	{
		bool flag = LayoutState.ShowOnlyOneSubButton();
		bool flag2 = DeckBuilderWidgetUtilities.HasUnassignedCardSkinsInDeck(_cosmeticsProvider, Pantry.Get<IPreferredPrintingDataProvider>(), Pantry.Get<ICardDatabaseAdapter>(), Context, ModelProvider);
		_applySkinsButton.gameObject.UpdateActive(flag2 && !Context.IsReadOnly && (!flag || !_batchCraftingButton.gameObject.activeSelf) && !Context.IsLimited);
	}

	private void OnAddCompanionClicked()
	{
		bool flag = Filter.IsSet(CardFilterType.Companions);
		CardFilterProvider.SetFilter(FilterValueChangeSource.Miscellaneous, CardFilterType.Companions, !flag);
		CardFilterProvider.ApplyFilters(FilterValueChangeSource.Miscellaneous);
	}

	private void DoneButton_OnClick()
	{
		this.DoneClicked?.Invoke();
	}

	private void OnPreLayoutUpdated()
	{
		bool isColumnViewExpanded = LayoutState.IsColumnViewExpanded;
		bool flag = LayoutState.LayoutInUse == DeckBuilderLayout.Column;
		bool flag2 = Context.Mode == DeckBuilderMode.ReadOnlyCollection;
		bool flag3 = Context.Mode == DeckBuilderMode.ReadOnly;
		bool flag4 = !flag2 && Context.IsEditingDeck && flag;
		bool active = !flag3 && !flag2 && Context.IsEditingDeck && !flag;
		_deckColumnParent.gameObject.UpdateActive(flag3 || flag4);
		bool flag5 = isColumnViewExpanded || flag3;
		_poolParent.GetChild(0).gameObject.UpdateActive(!flag5);
		_deckListParent.gameObject.UpdateActive(active);
		_deckListParent.GetComponent<LayoutElement>().enabled = true;
	}

	private void UpdateCollectionFilterToggles()
	{
		bool shouldShowCollectedToggles = Context.ShouldShowCollectedToggles;
		_advancedFiltersView.UpdateFilterMode(shouldShowCollectedToggles);
	}

	private void UpdateDeckBladeParticles()
	{
		DeckBuilderWidgetUtilities.UpdateDeckBladeParticles(_deckColumnView.isActiveAndEnabled ? ((MonoBehaviour)_deckColumnView) : ((MonoBehaviour)(_deckListView.isActiveAndEnabled ? _deckListView : null)));
	}

	public void FocusNameInputField()
	{
		if (!UseColumnView && ListViewShowingSideboard)
		{
			ListViewShowingSideboard = false;
			_deckListView.ShowSideboardToggle.isOn = false;
			VisualsUpdater.UpdateAllDeckVisuals();
		}
		TMP_InputField tMP_InputField = (UseColumnView ? _deckColumnView.DeckNameInput : _deckListView.DeckNameInput);
		EventSystem.current.SetSelectedGameObject(tMP_InputField.gameObject, null);
	}

	public bool HasChangesInCurrentDeck()
	{
		return DeckBuilderWidgetUtilities.HasChangesInCurrentDeck(Context, Model);
	}

	public DeckInfo GetDeck()
	{
		return Model.GetServerModel();
	}

	public void OnFind()
	{
		EventSystem.current.SetSelectedGameObject(_header.SearchInput.gameObject, null);
		_header.SearchInput.ActivateInputField();
	}

	private void LoadCards()
	{
		if (Context != null && !Context.IsSideboarding && Context?.Format != null && Model != null)
		{
			Model.SwapRebalancedCards(Context);
		}
		if (Context?.Deck != null && !Context.IsReadOnly && !Context.OnlyShowPoolCards)
		{
			RemoveNonCollectibleCards();
		}
		CompanionUtil.UpdateValidation(Model, Context?.Format);
		CardFilterProvider.ResetAndApplyFilters(FilterValueChangeSource.Miscellaneous);
		if (Context.Deck != null)
		{
			_deckListView.DeckNameInput.textComponent.rectTransform.anchoredPosition = Vector2.zero;
			_deckColumnView.DeckNameInput.textComponent.rectTransform.anchoredPosition = Vector2.zero;
			_deckListView.DeckNameInput.text = Context.Deck.name;
			_deckColumnView.DeckNameInput.text = Context.Deck.name;
			if (Context.SuggestLandAfterDeckLoad)
			{
				BasicLandSuggester.SuggestLand(keepExistingUnlimitedBasicLands: false);
			}
			if (Context.IsLimited)
			{
				ModelProvider.AssignUnassignedCardSkinsToDeck();
				ModelProvider.ReplaceCardPoolWithFavorites();
				VisualsUpdater.RefreshPoolView(scrollToTop: false, null);
			}
			VisualsUpdater.UpdateAllDeckVisuals();
		}
	}

	private void RemoveNonCollectibleCards()
	{
		if (0 + RemoveNonCollectibleCardsFromList(_cardDatabase.CardDataProvider, Context?.Deck?.mainDeck, allowSpecializeFacets: false, delegate(uint id)
		{
			Model?.RemoveCardFromMainDeck(id);
		}) + RemoveNonCollectibleCardsFromList(_cardDatabase.CardDataProvider, Context?.Deck?.sideboard, allowSpecializeFacets: false, delegate(uint id)
		{
			Model?.RemoveCardFromSideboard(id);
		}) + RemoveNonCollectibleCardsFromList(_cardDatabase.CardDataProvider, Context?.Deck?.commandZone, allowSpecializeFacets: true, delegate(uint id)
		{
			Model?.RemoveCardFromCommandZone(id);
		}) > 0)
		{
			string title = (MTGALocalizedString)"MainNav/DeckBuilder/DeckBuilder_InvalidDeckReasons_NotLegal";
			string text = (MTGALocalizedString)"MainNav/DeckBuilder/DeckBuilder_InvalidDeckReasons_Uncollectible";
			SystemMessageManager.Instance.ShowOk(title, text);
		}
	}

	private static long RemoveNonCollectibleCardsFromList(ICardDataProvider db, IEnumerable<CardInDeck> cardList, bool allowSpecializeFacets, Action<uint> removeMethod)
	{
		if (db == null || cardList == null)
		{
			return 0L;
		}
		long num = 0L;
		foreach (CardInDeck card in cardList)
		{
			CardPrintingData cardPrintingData = ((!allowSpecializeFacets) ? db.GetCardPrintingById(card.Id) : SpecializeUtilities.GetBasePrinting(db, card.Id).BasePrinting);
			if (!CardUtilities.IsCardCollectible(cardPrintingData))
			{
				for (int i = 0; i < card.Quantity; i++)
				{
					removeMethod(card.Id);
				}
				num += card.Quantity;
			}
		}
		return num;
	}

	public void OnContextSet(DeckBuilderContext context)
	{
		CompanionUtil?.UpdateValidation(Model, context?.Format);
	}

	public void Initialize(ICardRolloverZoom zoomHandler, InventoryManager inventoryManager, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, StoreManager storeManager, EventManager eventManager, FormatManager formatManager, AssetLookupSystem assetLookupSystem, IBILogger logger, CosmeticsProvider cosmeticsProvider, CardBackCatalog cardBackCatalog, PetCatalog petCatalog, AvatarCatalog avatarCatalog, DecksManager deckManager, IActionSystem actionSystem, IEmoteDataProvider emoteDataProvider, IClientLocProvider locManager, IUnityObjectPool unityObjectPool, ISetMetadataProvider setMetadataProvider)
	{
		_zoomHandler = zoomHandler;
		_inventoryManager = inventoryManager;
		_cardDatabase = cardDatabase;
		_cardViewBuilder = cardViewBuilder;
		_storeManager = storeManager;
		_cardBackCatalog = cardBackCatalog;
		_deckManager = deckManager;
		_actionSystem = actionSystem;
		_eventManager = eventManager;
		_formatManager = formatManager;
		_logger = logger;
		_cosmeticsProvider = cosmeticsProvider;
		_assetLookupSystem = assetLookupSystem;
		_setMetadataProvider = setMetadataProvider;
		_accountClient = Pantry.Get<IAccountClient>();
		LayoutState.HideSideboardBeforeFormatSelected = _hideSideboardBeforeFormatSelected;
		LayoutState.LimitSubButtonsInHalfView = _limitSubButtonsInHalfView;
		LayoutState.CanUseLargeCardsInColumnView = _canUseLargeCardsInColumnView;
		if (_inventoryManager != null)
		{
			_inventoryManager.OnRedeemWildcardResponse += InventoryManager_OnRedeemWildcardsResponse;
			_inventoryManager.OnPurchaseSkinResponse += InventoryManager_OnPurchaseSkinResponse;
			_inventoryManager.NewCardUpdate += NewCardUpdate;
		}
		_poolCardHolder = UnityEngine.Object.Instantiate(_poolHolderPrefab, _poolParent);
		_poolCardHolder.GetComponent<RectTransform>().StretchToParent();
		_poolCardHolder.DirectlySetZoomHandler = _zoomHandler;
		_poolCardHolder.EnsureInit(_cardDatabase, _cardViewBuilder);
		_poolCardHolder.RolloverZoomView = _zoomHandler;
		_deckColumnView = UnityEngine.Object.Instantiate(_deckColumnViewPrefab, _deckColumnParent);
		_deckColumnView.DeckBuilderWidgetInit(_assetLookupSystem, _zoomHandler, _cardDatabase, _cardViewBuilder, _cardBackCatalog);
		string prefabPath = assetLookupSystem.GetPrefabPath<DeckListViewPrefab, DeckListView>();
		_deckListView = AssetLoader.Instantiate<DeckListView>(prefabPath, _deckListParent);
		_deckListView.DeckBuilderWidgetInit(_zoomHandler, _cardDatabase, _cardViewBuilder, _cardBackCatalog);
		_advancedFiltersView = UnityEngine.Object.Instantiate(_advancedFiltersPrefab, _popupRoot);
		_advancedFiltersView.Init(_formatManager, _eventManager);
		_header = UnityEngine.Object.Instantiate(_headerPrefab, _headerParent);
		_header.Init(_zoomHandler, _hideHeaderWhenExpanded);
		_header.GetComponent<RectTransform>().StretchToParent();
		_header.AdvancedFiltersButton.onClick.AddListener(_advancedFiltersView.OpenAdvancedFilters);
		_header.NewDeckButton.OnClick.AddListener(OnNewDeck);
		_header.NewDeckButton.OnMouseover.AddListener(DeckBuilderWidgetUtilities.Generic_OnMouseOver);
		_header.LargeCardsToggle.onValueChanged.AddListener(LargeCardsToggle_OnValueChanged);
		_header.CraftingModeToggle.onValueChanged.AddListener(CraftingModeToggle_OnValueChanged);
		_header.CompanionFilterToggle.onValueChanged.AddListener(CompanionFilterToggle_OnValueChanged);
		string prefabPath2 = _assetLookupSystem.GetPrefabPath<DeckDetailsPrefab, DeckDetailsPopup>();
		DeckBuilderContext context = Context;
		bool isReadOnly = (context != null && context.IsReadOnly) || (Context?.IsSideboarding ?? false);
		_detailsPopup = AssetLoader.Instantiate<DeckDetailsPopup>(prefabPath2, _popupRoot);
		_detailsPopup.Init(locManager, _cosmeticsProvider, avatarCatalog, petCatalog, _assetLookupSystem, _deckManager, _zoomHandler, _logger, _cardDatabase, _cardViewBuilder, emoteDataProvider, unityObjectPool, _storeManager, isReadOnly);
		CardBackCatalog cardBackCatalog2 = _cardBackCatalog;
		if (cardBackCatalog2 != null && cardBackCatalog2.Count < 0)
		{
			_detailsPopup.DisableSleeveSelection();
		}
		string prefabPath3 = _assetLookupSystem.GetPrefabPath<SpecializePopupPrefab, SpecializePopup>();
		_specializePopup = AssetLoader.Instantiate<SpecializePopup>(prefabPath3, _popupRoot);
		IObjectPool objectPool = Pantry.Get<IObjectPool>();
		_specializePopup.Init(Camera.main, unityObjectPool, objectPool, _assetLookupSystem, _cardDatabase.AbilityDataProvider, _zoomHandler);
		_doneButton.OnMouseover.AddListener(DeckBuilderWidgetUtilities.Generic_OnMouseOver);
		_doneButton.OnClick.AddListener(DoneButton_OnClick);
		_backButton.OnMouseover.AddListener(DeckBuilderWidgetUtilities.Generic_OnMouseOver);
		_backButton.OnClick.AddListener(DoneButton_OnClick);
		_batchCraftingButton.OnMouseover.AddListener(DeckBuilderWidgetUtilities.Generic_OnMouseOver);
		_batchCraftingButton.OnClick.AddListener(BatchCraftingButton_OnClick);
		VisualsUpdater.OnSubButtonsUpdated += BatchCraftingButton_OnUpdated;
		_applySkinsButton.OnMouseover.AddListener(DeckBuilderWidgetUtilities.Generic_OnMouseOver);
		_applySkinsButton.OnClick.AddListener(ApplySkinsButton_OnClick);
		VisualsUpdater.OnSubButtonsUpdated += ApplySkinsButton_OnUpdated;
		_autoLandsToggle = UnityEngine.Object.Instantiate(_autoLandsTogglePrefab);
		_autoLandsToggle.Initialize(isAutoLandsToggleOn: true, _header, _poolCardHolder, VisualsUpdater);
	}

	private void OnDestroy()
	{
		Languages.LanguageChangedSignal.Listeners -= ResetLanguage;
		_doneButton.OnMouseover.RemoveListener(DeckBuilderWidgetUtilities.Generic_OnMouseOver);
		_doneButton.OnClick.RemoveListener(DoneButton_OnClick);
		_backButton.OnMouseover.RemoveListener(DeckBuilderWidgetUtilities.Generic_OnMouseOver);
		_backButton.OnClick.RemoveListener(DoneButton_OnClick);
		_header.AdvancedFiltersButton.onClick.RemoveListener(_advancedFiltersView.OpenAdvancedFilters);
		_header.NewDeckButton.OnClick.RemoveListener(OnNewDeck);
		_header.NewDeckButton.OnMouseover.RemoveListener(DeckBuilderWidgetUtilities.Generic_OnMouseOver);
		_header.LargeCardsToggle.onValueChanged.RemoveListener(LargeCardsToggle_OnValueChanged);
		_header.CraftingModeToggle.onValueChanged.RemoveListener(CraftingModeToggle_OnValueChanged);
		_header.CompanionFilterToggle.onValueChanged.RemoveListener(CompanionFilterToggle_OnValueChanged);
		_batchCraftingButton.OnClick.RemoveListener(BatchCraftingButton_OnClick);
		_batchCraftingButton.OnMouseover.RemoveListener(DeckBuilderWidgetUtilities.Generic_OnMouseOver);
		VisualsUpdater.OnSubButtonsUpdated -= BatchCraftingButton_OnUpdated;
		_applySkinsButton.OnMouseover.RemoveListener(DeckBuilderWidgetUtilities.Generic_OnMouseOver);
		_applySkinsButton.OnClick.RemoveListener(ApplySkinsButton_OnClick);
		VisualsUpdater.OnSubButtonsUpdated -= ApplySkinsButton_OnUpdated;
		if (_inventoryManager != null)
		{
			_inventoryManager.OnRedeemWildcardResponse -= InventoryManager_OnRedeemWildcardsResponse;
			_inventoryManager.OnPurchaseSkinResponse -= InventoryManager_OnPurchaseSkinResponse;
			_inventoryManager.NewCardUpdate -= NewCardUpdate;
		}
	}

	private void setActiveDragDropCards(bool isActive)
	{
		_deckColumnView.MainHolder.CanDragCards = (MetaCardView _) => isActive;
		_deckColumnView.MainHolder.CanDropCards = (MetaCardView _) => isActive;
		_deckColumnView.SideboardCardHolder.CanDragCards = (MetaCardView _) => isActive;
		_deckColumnView.SideboardCardHolder.CanDropCards = (MetaCardView _) => isActive;
	}

	private void setActiveSingleClickCards(bool isActive)
	{
		_deckColumnView.MainHolder.CanSingleClickCards = (MetaCardView _) => isActive;
		_deckColumnView.SideboardCardHolder.CanSingleClickCards = (MetaCardView _) => isActive;
	}

	public void ShowOrHide(bool active)
	{
		if (!_isActive && active)
		{
			_actionSystem?.PushFocus(this);
		}
		else if (_isActive && !active)
		{
			_actionSystem?.PopFocus(this);
		}
		_isActive = active;
		_detailsPopup.Activate(activate: false);
		_detailsPopup.SetDeckDetailsRequested(active);
		_specializePopup.Activate(activate: false);
		_advancedFiltersView.Hide(applyFilters: false);
		_deckColumnView.MainHolder.OnHideQuantityAdjust(repositionCards: false);
		if (!active)
		{
			return;
		}
		_deckColumnView.MainHolder.ClearCards();
		_deckColumnView.SideboardCardHolder.ClearCards();
		_deckListView.MainDeckCardHolder.ClearCards();
		_deckListView.SideboardCardHolder.ClearCards();
		LayoutState.IsColumnViewExpanded = false;
		Mode = Context.StartingMode;
		if (Context.IsReadOnly)
		{
			setActiveDragDropCards(isActive: false);
			setActiveSingleClickCards(Context.CanCraft);
			_deckColumnView.ExpandColumnViewButton.gameObject.UpdateActive(active: false);
			_doneButton.gameObject.UpdateActive(active: false);
			_backButton.gameObject.UpdateActive(active: true);
		}
		else if (Context.StartingMode == DeckBuilderMode.ReadOnlyCollection)
		{
			setActiveDragDropCards(isActive: false);
			setActiveSingleClickCards(isActive: false);
			_deckColumnView.ExpandColumnViewButton.gameObject.UpdateActive(active: false);
			_doneButton.gameObject.UpdateActive(active: false);
			_backButton.gameObject.UpdateActive(active: true);
		}
		else
		{
			setActiveDragDropCards(isActive: true);
			setActiveSingleClickCards(isActive: true);
			_deckColumnView.SideboardCardHolder.CanDragCards = (MetaCardView _) => !PlatformUtils.IsHandheld();
			_deckColumnView.ExpandColumnViewButton.gameObject.UpdateActive(active: true);
			if (Context.IsInvalidForEventFormat)
			{
				DeckFormat safeFormat = _formatManager.GetSafeFormat(Context.EventFormat);
				DeckFormat safeFormat2 = _formatManager.GetSafeFormat(Context.Deck.format);
				string button3AlertText = (_deckManager.DeckLimitReached ? Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/ButtonAlert_Deck_Limit_Reached") : "");
				SystemMessageManager.Instance.ShowMessage((MTGALocalizedString)"SystemMessage/InvalidDeckChangeFormat_Header", string.Format(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/InvalidDeckChangeFormat_Body"), _formatManager.GetSafeFormat(safeFormat2.FormatName).GetLocalizedName(), _formatManager.GetSafeFormat(safeFormat.FormatName).GetLocalizedName()), (MTGALocalizedString)"MainNav/DeckBuilder/Cancel_Button", "", button1Disabled: false, null, (MTGALocalizedString)"SystemMessage/InvalidDeckChangeFormat_ButtonChange", "", button2Disabled: false, delegate
				{
					ContextProvider.SelectFormat(_formatManager.GetSafeFormat(Context.EventFormat));
				}, (MTGALocalizedString)"SystemMessage/InvalidDeckChangeFormat_ButtonClone", button3AlertText, _deckManager.DeckLimitReached, CloneDeckWithEventFormat);
			}
			_doneButton.gameObject.UpdateActive(active: true);
			_backButton.gameObject.UpdateActive(active: false);
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(_header.GetComponent<RectTransform>());
		_header.gameObject.SetActive(value: false);
		_header.gameObject.SetActive(value: true);
		string userAccountID = _accountClient?.AccountInformation?.PersonaID;
		if (Context != null && !Context.IsSideboarding)
		{
			List<DeckFormat> availableFormats = DeckBuilderWidgetUtilities.GetAvailableFormats(_formatManager.GetAllFormats(), _eventManager.EventContexts, Context.Format);
			_deckListView.MainDeckTitlePanel.SetDeckFormatSelector(availableFormats, Context.IsAmbiguousFormat, Context.IsConstructed && Context.IsFirstEdit);
			_deckColumnView.MainTitlePanel.SetDeckFormatSelector(availableFormats, Context.IsAmbiguousFormat, Context.IsConstructed && Context.IsFirstEdit);
		}
		ListViewShowingSideboard = false;
		VisualsUpdater.UpdateLayout();
		LayoutState.UpdateSideboardVisibility();
		_deckListView.ShowSideboardToggle.isOn = false;
		_deckListView.ShowMainDeckOrSideboard(value: false);
		if (Mode == DeckBuilderMode.ReadOnlyCollection)
		{
			LargeCardsInPool = false;
		}
		else
		{
			LargeCardsInPool = DeckBuilderWidgetUtilities.CanUseLargeCards(Context, LayoutState) && MDNPlayerPrefs.GetLargeCardsInPool(userAccountID);
			_header.LargeCardsToggle.isOn = LargeCardsInPool;
		}
		_suppressModeChange = true;
		_header.CraftingModeToggle.isOn = Mode == DeckBuilderMode.Crafting;
		_suppressModeChange = false;
		UpdateDeckBladeParticles();
		_header.CraftingModeToggle.gameObject.UpdateActive(Context.ShowCraftingButtons(_deckManager));
		LoadCards();
		if (!Context.IsEditingDeck)
		{
			_batchCraftingButton.gameObject.UpdateActive(active: false);
			_applySkinsButton.gameObject.UpdateActive(active: false);
		}
		_header.LandsToggle.gameObject.UpdateActive(Context.IsEditingDeck && !Context.IsReadOnly);
		if (_header.LandsPreview != null)
		{
			_header.LandsPreview.gameObject.UpdateActive(Context.IsEditingDeck && !Context.IsReadOnly);
		}
		_header.CompanionFilterToggle.isOn = Context != null && DeckUtilities.GetCompanionFilterDefaultState(Context.IsSideboarding, Context.Deck);
		_header.NewDeckButton.gameObject.UpdateActive(!Context.IsEditingDeck && Mode != DeckBuilderMode.ReadOnlyCollection);
		_doneButton.gameObject.SetActive(Context.IsEditingDeck && !Context.IsReadOnly);
		if (!string.IsNullOrEmpty(Context.SetToFilter) && Enum.TryParse<CardFilterType>("Expansion_" + Context.SetToFilter, out var result))
		{
			CardFilterProvider.SetFilter(FilterValueChangeSource.Miscellaneous, result, value: true);
			CardFilterProvider.SetFilter(FilterValueChangeSource.Miscellaneous, CardFilterType.Collection_Uncollected, value: true);
			_header.SearchInput.text = "?booster";
			CardFilterProvider.ApplyFilters(FilterValueChangeSource.Miscellaneous);
		}
	}

	private void CloneDeckWithEventFormat()
	{
		DeckBuilderWidgetUtilities.CloneDeckWithEventFormat(_deckManager, _formatManager, Context, ShowOrHide);
	}

	private void NewCardUpdate()
	{
		_poolCardHolder.SetCardsDirty();
	}

	private void InventoryManager_OnRedeemWildcardsResponse(bool success)
	{
		if (!base.gameObject.activeInHierarchy || !success)
		{
			return;
		}
		if (!Context.OnlyShowPoolCards)
		{
			Dictionary<uint, uint> poolCounts = _inventoryManager.Cards.ToDictionary((KeyValuePair<uint, int> x) => x.Key, (KeyValuePair<uint, int> x) => (uint)x.Value);
			Model.SetPoolCounts(poolCounts);
			VisualsUpdater.RefreshPoolView(scrollToTop: false, null);
		}
		if (Context.IsEditingDeck)
		{
			VisualsUpdater.UpdateAllDeckVisuals();
		}
	}

	private void InventoryManager_OnPurchaseSkinResponse(uint grpId, string variant, bool success)
	{
		if (!base.gameObject.activeInHierarchy || !success)
		{
			return;
		}
		VisualsUpdater.RefreshPoolView(scrollToTop: false, null);
		if (Context.IsEditingDeck)
		{
			Model.SetCardSkin(grpId, variant);
			if (Model.GetQuantityInWholeDeck(grpId) != 0)
			{
				VisualsUpdater.UpdateAllDeckVisuals();
			}
		}
	}
}
