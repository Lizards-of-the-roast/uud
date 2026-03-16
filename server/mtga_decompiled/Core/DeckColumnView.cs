using System;
using System.Collections;
using System.Collections.Generic;
using AssetLookupTree;
using Core.Code.Decks;
using Core.Meta.Cards.Views;
using Core.Shared.Code.CardFilters;
using GreClient.CardData;
using TMPro;
using UnityEngine;
using Wizards.Mtga;
using Wizards.Mtga.Platforms;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Extensions;

public class DeckColumnView : MonoBehaviour
{
	private const float READONLY_VIEW_SIDEBOARD_VIEWPORT_HEIGHT = 850f;

	private const float EDITING_VIEW_SIDEBOARD_VIEWPORT_HEIGHT = 400f;

	[Header("Main Deck Title Panel References")]
	public DeckMainTitlePanel MainTitlePanel;

	public TMP_InputField DeckNameInput;

	public TMP_Text DeckNameDraft;

	public DeckCostsSummary CostWidget;

	public CustomButton SleevesButton;

	public TooltipTrigger DeckImageTrigger;

	public RectTransform SleeveCDCParent;

	public GameObject SleeveDefault;

	[Header("Main Deck References")]
	public CardDropHandler[] DeckBoxDropTargets;

	public StaticColumnManager MainHolder;

	public CommanderSlotCardHolder CommanderCardHolder;

	public CommanderSlotCardHolder PartnerCardHolder;

	public CommanderStackCardHolder CommanderStackCardHolder;

	public CommanderSlotCardHolder CompanionCardHolder;

	[Header("Sideboard Title Panel References")]
	public DeckSideboardTitlePanel SideboardTitlePanel;

	[Header("Sideboard References")]
	public SideboardListCardHolder SideboardCardHolder;

	public Transform SideboardCardsParent;

	public GameObject SideboardIconDraft;

	[SerializeField]
	private RectTransform _sideboardTransform;

	[Header("Other References")]
	public CustomButton ExpandColumnViewButton;

	public Animator DeckBladeAnimator;

	[Header("Column View Expansion")]
	[SerializeField]
	private float _expandColumnPoolUpdateDelay;

	[SerializeField]
	private float _contractColumnPoolUpdateDelay;

	[SerializeField]
	private bool _isSideboardExpandedOnly;

	private Meta_CDC _cdc;

	private Meta_CDC[] _sideboardCDCs;

	private static readonly int Expanded = Animator.StringToHash("Expanded");

	private ICardRolloverZoom _zoomHandler;

	private DeckBuilderModelProvider ModelProvider => Pantry.Get<DeckBuilderModelProvider>();

	private DeckBuilderContextProvider ContextProvider => Pantry.Get<DeckBuilderContextProvider>();

	private DeckBuilderVisualsUpdater VisualsUpdater => Pantry.Get<DeckBuilderVisualsUpdater>();

	private DeckBuilderCardFilterProvider CardFilterProvider => Pantry.Get<DeckBuilderCardFilterProvider>();

	private DeckBuilderLayoutState LayoutState => Pantry.Get<DeckBuilderLayoutState>();

	private DeckBuilderActionsHandler ActionsHandler => Pantry.Get<DeckBuilderActionsHandler>();

	private ICardRolloverZoom ZoomHandler
	{
		get
		{
			if (_zoomHandler == null)
			{
				return Pantry.Get<ICardRolloverZoom>();
			}
			return _zoomHandler;
		}
	}

	public void Awake()
	{
		DeckNameInput.onEndEdit.AddListener(OnEndEditDeckName);
	}

	public void OnDestroy()
	{
		DeckNameInput.onEndEdit.RemoveListener(OnEndEditDeckName);
		MainTitlePanel.Button.OnClick.RemoveListener(ActionsHandler.OpenDeckDetails);
		StaticColumnManager mainHolder = MainHolder;
		mainHolder.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Remove(mainHolder.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(OnCardDroppedImpl));
		StaticColumnManager mainHolder2 = MainHolder;
		mainHolder2.OnCardClicked = (Action<MetaCardView>)Delegate.Remove(mainHolder2.OnCardClicked, new Action<MetaCardView>(DeckMainColumns_OnCardClicked));
		StaticColumnManager mainHolder3 = MainHolder;
		mainHolder3.OnCardUpdated = (Action<MetaCardView>)Delegate.Remove(mainHolder3.OnCardUpdated, new Action<MetaCardView>(DeckMainColumns_OnCardUpdate));
		MainHolder.CanAddCard = null;
		ColumnCardQuantityAdjust quantityAdjust = MainHolder.QuantityAdjust;
		quantityAdjust.CardQuantityIncrease = (Action<CardData>)Delegate.Remove(quantityAdjust.CardQuantityIncrease, new Action<CardData>(DeckMainColumns_OnCardAdded));
		ColumnCardQuantityAdjust quantityAdjust2 = MainHolder.QuantityAdjust;
		quantityAdjust2.CardQuantityDecrease = (Action<CardData>)Delegate.Remove(quantityAdjust2.CardQuantityDecrease, new Action<CardData>(DeckMainColumns_OnCardRemoved));
		MainHolder.QuantityAdjust.CanAddCard = null;
		SideboardListCardHolder sideboardCardHolder = SideboardCardHolder;
		sideboardCardHolder.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Remove(sideboardCardHolder.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(OnCardDroppedImpl));
		SideboardListCardHolder sideboardCardHolder2 = SideboardCardHolder;
		sideboardCardHolder2.OnCardAddClicked = (Action<MetaCardView>)Delegate.Remove(sideboardCardHolder2.OnCardAddClicked, new Action<MetaCardView>(DeckSideboard_OnCardAddClickedImpl));
		SideboardListCardHolder sideboardCardHolder3 = SideboardCardHolder;
		sideboardCardHolder3.OnCardRemoveClicked = (Action<MetaCardView>)Delegate.Remove(sideboardCardHolder3.OnCardRemoveClicked, new Action<MetaCardView>(DeckSideboard_OnCardRemoveClickedImpl));
		SideboardListCardHolder sideboardCardHolder4 = SideboardCardHolder;
		sideboardCardHolder4.OnCompanionCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Remove(sideboardCardHolder4.OnCompanionCardDropped, new Action<MetaCardView, MetaCardHolder>(OnCardDroppedImpl));
		SideboardListCardHolder sideboardCardHolder5 = SideboardCardHolder;
		sideboardCardHolder5.OnCompanionCardRemoveClicked = (Action<MetaCardView>)Delegate.Remove(sideboardCardHolder5.OnCompanionCardRemoveClicked, new Action<MetaCardView>(DeckCompanion_OnCardClickedImpl));
		SideboardCardHolder.CanDragCards = (MetaCardView _) => !PlatformUtils.IsHandheld();
		CommanderSlotCardHolder commanderCardHolder = CommanderCardHolder;
		commanderCardHolder.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Remove(commanderCardHolder.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(OnCardDroppedImpl));
		CommanderSlotCardHolder commanderCardHolder2 = CommanderCardHolder;
		commanderCardHolder2.OnCardClicked = (Action<MetaCardView>)Delegate.Remove(commanderCardHolder2.OnCardClicked, new Action<MetaCardView>(DeckCommander_OnCardClickedImpl));
		CommanderCardHolder.AddButton.OnClick.RemoveListener(OnAddCommanderClicked);
		CommanderSlotCardHolder partnerCardHolder = PartnerCardHolder;
		partnerCardHolder.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Remove(partnerCardHolder.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(OnCardDroppedImpl));
		CommanderSlotCardHolder partnerCardHolder2 = PartnerCardHolder;
		partnerCardHolder2.OnCardClicked = (Action<MetaCardView>)Delegate.Remove(partnerCardHolder2.OnCardClicked, new Action<MetaCardView>(DeckPartner_OnCardClickedImpl));
		PartnerCardHolder.AddButton.OnClick.RemoveListener(OnAddCommanderClicked);
		CommanderSlotCardHolder commanderSlotCardHolder = CommanderStackCardHolder.CommanderSlotCardHolder;
		commanderSlotCardHolder.OnCardClicked = (Action<MetaCardView>)Delegate.Remove(commanderSlotCardHolder.OnCardClicked, new Action<MetaCardView>(DeckCommander_OnCardClickedImpl));
		CommanderSlotCardHolder partnerSlotCardHolder = CommanderStackCardHolder.PartnerSlotCardHolder;
		partnerSlotCardHolder.OnCardClicked = (Action<MetaCardView>)Delegate.Remove(partnerSlotCardHolder.OnCardClicked, new Action<MetaCardView>(DeckPartner_OnCardClickedImpl));
		CommanderSlotCardHolder companionCardHolder = CompanionCardHolder;
		companionCardHolder.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Remove(companionCardHolder.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(OnCardDroppedImpl));
		CommanderSlotCardHolder companionCardHolder2 = CompanionCardHolder;
		companionCardHolder2.OnCardClicked = (Action<MetaCardView>)Delegate.Remove(companionCardHolder2.OnCardClicked, new Action<MetaCardView>(DeckCompanion_OnCardClickedImpl));
		CompanionCardHolder.AddButton.OnClick.RemoveListener(OnAddCompanionClicked);
		ExpandColumnViewButton.OnClick.RemoveListener(OnExpandColumnViewClicked);
		DeckSideboardTitlePanel sideboardTitlePanel = SideboardTitlePanel;
		sideboardTitlePanel.OnExpandChange = (Action)Delegate.Remove(sideboardTitlePanel.OnExpandChange, new Action(OnSideboardExpandChange));
		VisualsUpdater.OnLayoutUpdated -= OnLayoutUpdated;
		SleevesButton.OnMouseover.RemoveListener(DeckBuilderWidgetUtilities.Generic_OnMouseOver);
		SleevesButton.OnClick.RemoveListener(DeckBuilderWidgetUtilities.CardBackButton_OnClick);
	}

	public void DeckSideboard_OnCardAddClickedImpl(MetaCardView cardView)
	{
		ActionsHandler.DeckSideboard_OnCardAddClicked(cardView, ZoomHandler);
	}

	public void DeckSideboard_OnCardRemoveClickedImpl(MetaCardView cardView)
	{
		ActionsHandler.DeckSideboard_OnCardRemoveClicked(cardView, ZoomHandler);
	}

	public void OnCardDroppedImpl(MetaCardView cardView, MetaCardHolder destinationHolder)
	{
		ActionsHandler.OnCardDropped(cardView, destinationHolder, ZoomHandler);
	}

	public void DeckCommander_OnCardClickedImpl(MetaCardView cardView)
	{
		ActionsHandler.DeckCommander_OnCardClicked(cardView, ZoomHandler, isPartner: false);
	}

	public void DeckPartner_OnCardClickedImpl(MetaCardView cardView)
	{
		ActionsHandler.DeckCommander_OnCardClicked(cardView, ZoomHandler, isPartner: true);
	}

	public void DeckCompanion_OnCardClickedImpl(MetaCardView cardView)
	{
		ActionsHandler.DeckCompanion_OnCardClicked(cardView, ZoomHandler);
	}

	public void OnEnable()
	{
		PartnerCardHolder.gameObject.SetActive(value: false);
		CommanderStackCardHolder.gameObject.SetActive(value: false);
		ModelProvider.OnModelNameSet += OnModelNameSet;
		ModelProvider.OnDeckCardBackSet += OnDeckCardBackSet;
		OnModelNameSet(ModelProvider.Model._deckName);
		OnDeckCardBackSet(ModelProvider.Model._cardBack);
		VisualsUpdater.PreUpdateAllDeckVisuals += OnPreUpdateAllDeckVisuals;
		VisualsUpdater.MainDeckColumnVisualsUpdated += OnMainDeckColumnVisualsUpdated;
		VisualsUpdater.CommanderViewUpdated += OnCommanderViewUpdated;
		VisualsUpdater.PartnerViewUpdated += OnPartnerViewUpdated;
		VisualsUpdater.CompanionViewUpdated += OnCompanionViewUpdated;
		CardFilterProvider.OnApplyFilters += UpdateSpecialCardsActive;
		LayoutState.SideboardVisibilityUpdated += OnSideboardVisibilityUpdated;
		OnSideboardVisibilityUpdated(LayoutState.GetSideboardVisibility());
		ActionsHandler.ShowQuantityAdjust += MainHolder.ShowQuantityAdjust;
		ActionsHandler.OnHideQuantityAdjust += MainHolder.OnHideQuantityAdjust;
		ActionsHandler.OnCardRightClicked += MainHolder.ReleaseDraggedCards;
		ActionsHandler.OnCardRightClicked += CommanderCardHolder.ReleaseAllDraggingCards;
		ActionsHandler.OnCardRightClicked += PartnerCardHolder.ReleaseAllDraggingCards;
		CardDropHandler[] deckBoxDropTargets = DeckBoxDropTargets;
		foreach (CardDropHandler obj in deckBoxDropTargets)
		{
			obj.OnCardDropped = (Action<MetaCardView>)Delegate.Combine(obj.OnCardDropped, new Action<MetaCardView>(ModelProvider.SetDeckBoxTextureByCardData));
		}
		Pantry.Get<MetaCardViewDragState>().CompanionAddButtonStateChange += SetCompanionAddButtonState;
	}

	public void OnDisable()
	{
		ModelProvider.OnModelNameSet -= OnModelNameSet;
		ModelProvider.OnDeckCardBackSet -= OnDeckCardBackSet;
		VisualsUpdater.PreUpdateAllDeckVisuals -= OnPreUpdateAllDeckVisuals;
		VisualsUpdater.MainDeckColumnVisualsUpdated -= OnMainDeckColumnVisualsUpdated;
		VisualsUpdater.CommanderViewUpdated -= OnCommanderViewUpdated;
		VisualsUpdater.PartnerViewUpdated -= OnPartnerViewUpdated;
		VisualsUpdater.CompanionViewUpdated -= OnCompanionViewUpdated;
		CardFilterProvider.OnApplyFilters -= UpdateSpecialCardsActive;
		LayoutState.SideboardVisibilityUpdated -= OnSideboardVisibilityUpdated;
		ActionsHandler.ShowQuantityAdjust -= MainHolder.ShowQuantityAdjust;
		ActionsHandler.OnHideQuantityAdjust -= MainHolder.OnHideQuantityAdjust;
		ActionsHandler.OnCardRightClicked -= MainHolder.ReleaseDraggedCards;
		ActionsHandler.OnCardRightClicked -= CommanderCardHolder.ReleaseAllDraggingCards;
		ActionsHandler.OnCardRightClicked -= PartnerCardHolder.ReleaseAllDraggingCards;
		CardDropHandler[] deckBoxDropTargets = DeckBoxDropTargets;
		foreach (CardDropHandler obj in deckBoxDropTargets)
		{
			obj.OnCardDropped = (Action<MetaCardView>)Delegate.Remove(obj.OnCardDropped, new Action<MetaCardView>(ModelProvider.SetDeckBoxTextureByCardData));
		}
		Pantry.Get<MetaCardViewDragState>().CompanionAddButtonStateChange -= SetCompanionAddButtonState;
	}

	public void OnModelNameSet(string name)
	{
		DeckNameInput.text = name;
	}

	private void OnEndEditDeckName(string s)
	{
		ModelProvider.SetDeckName(s);
	}

	private void OnDeckCardBackSet(string cardBack)
	{
		SetSleeve(cardBack, Pantry.Get<DeckBuilderContextProvider>().Context?.IsSideboarding ?? false, Pantry.Get<CardViewBuilder>(), Pantry.Get<CardDatabase>());
	}

	public void SetSleeve(string cardBack, bool isSideboarding, CardViewBuilder builder, CardDatabase cardDatabase)
	{
		if (string.IsNullOrEmpty(cardBack))
		{
			cardBack = "CardBack_Default";
		}
		CardData data = CardDataExtensions.CreateSkinCard(0u, cardDatabase, null, cardBack, faceDown: true);
		if (_cdc == null)
		{
			_cdc = builder.CreateMetaCdc(data, SleeveCDCParent);
		}
		else
		{
			_cdc.SetModel(data);
		}
		_cdc.CollisionRoot.gameObject.SetActive(!isSideboarding);
		if (_sideboardCDCs == null)
		{
			_sideboardCDCs = new Meta_CDC[SideboardCardsParent.childCount];
			for (int i = 0; i < SideboardCardsParent.childCount; i++)
			{
				Transform child = SideboardCardsParent.GetChild(i);
				child.GetComponent<MeshRenderer>().enabled = false;
				_sideboardCDCs[i] = builder.CreateMetaCdc(data, child);
			}
		}
		else
		{
			Meta_CDC[] sideboardCDCs = _sideboardCDCs;
			for (int j = 0; j < sideboardCDCs.Length; j++)
			{
				sideboardCDCs[j].SetModel(data);
			}
		}
		SleeveDefault.UpdateActive(active: false);
	}

	public void UpdateSpecialCardsActive(IReadOnlyCardFilter filter)
	{
		bool flag = filter.IsSet(CardFilterType.Commanders);
		CommanderCardHolder.SetAddButtonActive(flag);
		bool flag2 = DeckBuilderWidgetUtilities.HasCommanderSet(ContextProvider.Context, ModelProvider.Model) == DeckBuilderWidgetUtilities.CommanderType.PartnerableCommander;
		PartnerCardHolder.SetAddButtonActive(flag && flag2);
		bool addButtonActive = filter.IsSet(CardFilterType.Companions);
		CompanionCardHolder.SetAddButtonActive(addButtonActive);
	}

	public void SetEnabledControls(bool canEditCards, bool canEditMetaData)
	{
		TooltipTrigger[] componentsInChildren = GetComponentsInChildren<TooltipTrigger>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].IsActive = canEditMetaData;
		}
		TMP_InputField[] componentsInChildren2 = GetComponentsInChildren<TMP_InputField>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].interactable = canEditMetaData;
		}
		CommanderCardHolder.CanDragCards = (MetaCardView _) => canEditCards;
		PartnerCardHolder.CanDragCards = (MetaCardView _) => canEditCards;
		CompanionCardHolder.CanDragCards = (MetaCardView _) => canEditCards;
		SleevesButton.enabled = canEditMetaData;
		SleevesButton.GetComponent<Animator>().enabled = canEditMetaData;
		SleevesButton.transform.Find("Anchor_RIGHT/DeckboxHighlight").gameObject.UpdateActive(canEditMetaData);
		DeckNameInput.enabled = canEditMetaData;
		CardDropHandler[] deckBoxDropTargets = DeckBoxDropTargets;
		for (int i = 0; i < deckBoxDropTargets.Length; i++)
		{
			deckBoxDropTargets[i].SetInteractable(canEditMetaData);
		}
	}

	public void SetExpandedView(bool isExpanded)
	{
		DeckBladeAnimator.SetBool(Expanded, isExpanded);
		MainHolder.ExpandedView = isExpanded;
		float y = (isExpanded ? 850f : 400f);
		_sideboardTransform.sizeDelta = new Vector2(_sideboardTransform.sizeDelta.x, y);
	}

	public void DeckBuilderWidgetInit(AssetLookupSystem assetLookupSystem, ICardRolloverZoom zoomHandler, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, CardBackCatalog cardBackCatalog)
	{
		MainHolder.Init(assetLookupSystem, cardDatabase, cardViewBuilder);
		GetComponent<RectTransform>().StretchToParent();
		_zoomHandler = zoomHandler;
		SideboardCardHolder.EnsureInit(cardDatabase, cardViewBuilder);
		SideboardCardHolder.RolloverZoomView = zoomHandler;
		SideboardCardHolder.CompanionCardHolder.RolloverZoomView = zoomHandler;
		CommanderCardHolder.EnsureInit(cardDatabase, cardViewBuilder);
		CommanderCardHolder.RolloverZoomView = zoomHandler;
		CommanderCardHolder.AddButton.OnClick.AddListener(OnAddCommanderClicked);
		PartnerCardHolder.EnsureInit(cardDatabase, cardViewBuilder);
		PartnerCardHolder.RolloverZoomView = zoomHandler;
		PartnerCardHolder.AddButton.OnClick.AddListener(OnAddCommanderClicked);
		CommanderStackCardHolder.Init(cardDatabase, cardViewBuilder, zoomHandler);
		CompanionCardHolder.EnsureInit(cardDatabase, cardViewBuilder);
		CompanionCardHolder.RolloverZoomView = zoomHandler;
		CompanionCardHolder.AddButton.OnClick.AddListener(OnAddCompanionClicked);
		DeckNameInput.gameObject.SetActive(value: true);
		DeckNameDraft.gameObject.SetActive(value: false);
		DeckImageTrigger.IsActive = true;
		SleevesButton.gameObject.SetActive(value: true);
		SideboardIconDraft.SetActive(value: false);
		SideboardCardsParent.gameObject.SetActive(value: true);
		MainHolder.RolloverZoomView = zoomHandler;
		StaticColumnManager mainHolder = MainHolder;
		mainHolder.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Combine(mainHolder.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(OnCardDroppedImpl));
		StaticColumnManager mainHolder2 = MainHolder;
		mainHolder2.OnCardClicked = (Action<MetaCardView>)Delegate.Combine(mainHolder2.OnCardClicked, new Action<MetaCardView>(DeckMainColumns_OnCardClicked));
		StaticColumnManager mainHolder3 = MainHolder;
		mainHolder3.OnCardUpdated = (Action<MetaCardView>)Delegate.Combine(mainHolder3.OnCardUpdated, new Action<MetaCardView>(DeckMainColumns_OnCardUpdate));
		MainHolder.CanDragCards = (MetaCardView _) => true;
		MainHolder.CanDropCards = (MetaCardView _) => true;
		MainHolder.CanDoubleClickCards = (MetaCardView _) => false;
		MainHolder.CanSingleClickCards = (MetaCardView _) => true;
		MainHolder.CanAddCard = ModelProvider.CanAddCardToMainDeck;
		ColumnCardQuantityAdjust quantityAdjust = MainHolder.QuantityAdjust;
		quantityAdjust.CardQuantityIncrease = (Action<CardData>)Delegate.Combine(quantityAdjust.CardQuantityIncrease, new Action<CardData>(DeckMainColumns_OnCardAdded));
		ColumnCardQuantityAdjust quantityAdjust2 = MainHolder.QuantityAdjust;
		quantityAdjust2.CardQuantityDecrease = (Action<CardData>)Delegate.Combine(quantityAdjust2.CardQuantityDecrease, new Action<CardData>(DeckMainColumns_OnCardRemoved));
		MainHolder.QuantityAdjust.CanAddCard = ModelProvider.CanAddCardToMainDeck;
		SideboardListCardHolder sideboardCardHolder = SideboardCardHolder;
		sideboardCardHolder.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Combine(sideboardCardHolder.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(OnCardDroppedImpl));
		SideboardListCardHolder sideboardCardHolder2 = SideboardCardHolder;
		sideboardCardHolder2.OnCardAddClicked = (Action<MetaCardView>)Delegate.Combine(sideboardCardHolder2.OnCardAddClicked, new Action<MetaCardView>(DeckSideboard_OnCardAddClickedImpl));
		SideboardListCardHolder sideboardCardHolder3 = SideboardCardHolder;
		sideboardCardHolder3.OnCardRemoveClicked = (Action<MetaCardView>)Delegate.Combine(sideboardCardHolder3.OnCardRemoveClicked, new Action<MetaCardView>(DeckSideboard_OnCardRemoveClickedImpl));
		SideboardCardHolder.CanDropCards = (MetaCardView _) => true;
		SideboardCardHolder.CanDoubleClickCards = (MetaCardView _) => false;
		SideboardCardHolder.CanSingleClickCards = (MetaCardView _) => true;
		SideboardListCardHolder sideboardCardHolder4 = SideboardCardHolder;
		sideboardCardHolder4.OnCompanionCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Combine(sideboardCardHolder4.OnCompanionCardDropped, new Action<MetaCardView, MetaCardHolder>(OnCardDroppedImpl));
		SideboardListCardHolder sideboardCardHolder5 = SideboardCardHolder;
		sideboardCardHolder5.OnCompanionCardRemoveClicked = (Action<MetaCardView>)Delegate.Combine(sideboardCardHolder5.OnCompanionCardRemoveClicked, new Action<MetaCardView>(DeckCompanion_OnCardClickedImpl));
		CommanderCardHolder.SetActive(active: true);
		CommanderSlotCardHolder commanderCardHolder = CommanderCardHolder;
		commanderCardHolder.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Combine(commanderCardHolder.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(OnCardDroppedImpl));
		CommanderSlotCardHolder commanderCardHolder2 = CommanderCardHolder;
		commanderCardHolder2.OnCardClicked = (Action<MetaCardView>)Delegate.Combine(commanderCardHolder2.OnCardClicked, new Action<MetaCardView>(DeckCommander_OnCardClickedImpl));
		CommanderCardHolder.CanDragCards = (MetaCardView _) => true;
		CommanderCardHolder.CanDropCards = (MetaCardView _) => true;
		CommanderCardHolder.CanDoubleClickCards = (MetaCardView _) => false;
		CommanderCardHolder.CanSingleClickCards = (MetaCardView _) => true;
		PartnerCardHolder.SetActive(active: true);
		CommanderSlotCardHolder partnerCardHolder = PartnerCardHolder;
		partnerCardHolder.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Combine(partnerCardHolder.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(OnCardDroppedImpl));
		CommanderSlotCardHolder partnerCardHolder2 = PartnerCardHolder;
		partnerCardHolder2.OnCardClicked = (Action<MetaCardView>)Delegate.Combine(partnerCardHolder2.OnCardClicked, new Action<MetaCardView>(DeckPartner_OnCardClickedImpl));
		PartnerCardHolder.CanDragCards = (MetaCardView _) => true;
		PartnerCardHolder.CanDropCards = (MetaCardView _) => true;
		PartnerCardHolder.CanDoubleClickCards = (MetaCardView _) => false;
		PartnerCardHolder.CanSingleClickCards = (MetaCardView _) => true;
		CommanderStackCardHolder.SetActive(isActive: true);
		CommanderSlotCardHolder commanderSlotCardHolder = CommanderStackCardHolder.CommanderSlotCardHolder;
		commanderSlotCardHolder.OnCardClicked = (Action<MetaCardView>)Delegate.Combine(commanderSlotCardHolder.OnCardClicked, new Action<MetaCardView>(DeckCommander_OnCardClickedImpl));
		CommanderStackCardHolder.CommanderSlotCardHolder.CanDragCards = (MetaCardView _) => true;
		CommanderStackCardHolder.CommanderSlotCardHolder.CanDoubleClickCards = (MetaCardView _) => false;
		CommanderStackCardHolder.CommanderSlotCardHolder.CanSingleClickCards = (MetaCardView _) => true;
		CommanderSlotCardHolder partnerSlotCardHolder = CommanderStackCardHolder.PartnerSlotCardHolder;
		partnerSlotCardHolder.OnCardClicked = (Action<MetaCardView>)Delegate.Combine(partnerSlotCardHolder.OnCardClicked, new Action<MetaCardView>(DeckPartner_OnCardClickedImpl));
		CommanderStackCardHolder.PartnerSlotCardHolder.CanDragCards = (MetaCardView _) => true;
		CommanderStackCardHolder.PartnerSlotCardHolder.CanDoubleClickCards = (MetaCardView _) => false;
		CommanderStackCardHolder.PartnerSlotCardHolder.CanSingleClickCards = (MetaCardView _) => true;
		CompanionCardHolder.SetActive(active: true);
		CompanionCardHolder.IsCompanion = true;
		CommanderSlotCardHolder companionCardHolder = CompanionCardHolder;
		companionCardHolder.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Combine(companionCardHolder.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(OnCardDroppedImpl));
		CommanderSlotCardHolder companionCardHolder2 = CompanionCardHolder;
		companionCardHolder2.OnCardClicked = (Action<MetaCardView>)Delegate.Combine(companionCardHolder2.OnCardClicked, new Action<MetaCardView>(DeckCompanion_OnCardClickedImpl));
		CompanionCardHolder.CanDragCards = (MetaCardView _) => true;
		CompanionCardHolder.CanDropCards = (MetaCardView _) => true;
		CompanionCardHolder.CanDoubleClickCards = (MetaCardView _) => false;
		CompanionCardHolder.CanSingleClickCards = (MetaCardView _) => true;
		ExpandColumnViewButton.OnClick.AddListener(OnExpandColumnViewClicked);
		DeckSideboardTitlePanel sideboardTitlePanel = SideboardTitlePanel;
		sideboardTitlePanel.OnExpandChange = (Action)Delegate.Combine(sideboardTitlePanel.OnExpandChange, new Action(OnSideboardExpandChange));
		VisualsUpdater.OnLayoutUpdated += OnLayoutUpdated;
		MainTitlePanel.Button.OnClick.AddListener(ActionsHandler.OpenDeckDetails);
		if (cardBackCatalog != null && cardBackCatalog.Count > 0)
		{
			SleevesButton.OnMouseover.AddListener(DeckBuilderWidgetUtilities.Generic_OnMouseOver);
			SleevesButton.OnClick.AddListener(DeckBuilderWidgetUtilities.CardBackButton_OnClick);
		}
	}

	public void OnSideboardVisibilityUpdated(bool showSideboard)
	{
		SideboardTitlePanel.gameObject.SetActive(showSideboard);
	}

	public void OnPreUpdateAllDeckVisuals()
	{
		MainHolder.ExpandedView = LayoutState.IsColumnViewExpanded;
	}

	public void OnMainDeckColumnVisualsUpdated(List<ListMetaCardViewDisplayInformation> displayInfos, CardFilter textFilter)
	{
		MainHolder.SetCards(displayInfos, textFilter);
	}

	public void OnCommanderViewUpdated(ListMetaCardViewDisplayInformation display, PagesMetaCardViewDisplayInformation pageDisplay, bool isReadOnly)
	{
		if (display != null)
		{
			MainHolder.SetCommanderCards(display, isReadOnly);
			CommanderStackCardHolder.SetCard(display, isReadOnly, DeckBuilderPile.Commander);
		}
		else
		{
			CommanderStackCardHolder.ClearCard(DeckBuilderPile.Commander);
			MainHolder.ClearCommanderCards();
		}
	}

	public void OnPartnerViewUpdated(ListMetaCardViewDisplayInformation display, PagesMetaCardViewDisplayInformation pageDisplay, bool isReadOnly)
	{
		SetIsCommanderStack(display?.Card != null);
		if (display != null)
		{
			CommanderStackCardHolder.SetCard(display, isReadOnly, DeckBuilderPile.Partner);
		}
		else
		{
			CommanderStackCardHolder.ClearCard(DeckBuilderPile.Partner);
		}
	}

	public void OnCompanionViewUpdated(ListMetaCardViewDisplayInformation display, PagesMetaCardViewDisplayInformation pageDisplay, bool isReadOnly)
	{
		if (display != null)
		{
			MainHolder.SetCompanionCard(display, isReadOnly);
			SideboardCardHolder.SetCompanionCards(display);
		}
		else
		{
			MainHolder.ClearCompanionCard();
			SideboardCardHolder.ClearCompanionCards();
		}
	}

	public void DeckMainColumns_OnCardRemoved(CardData cardData)
	{
		if (!ContextProvider.Context.IsReadOnly)
		{
			if (ContextProvider.Context.IsLimited && LayoutState.IsColumnViewExpanded)
			{
				VisualsUpdater.UnsuggestedCardsInPool.Add(cardData.Printing);
				VisualsUpdater.SuggestedCardsInDeck.Remove(cardData.Printing);
			}
			ModelProvider.RemoveCardFromPile(DeckBuilderPile.MainDeck, ZoomHandler, cardData);
		}
	}

	public void DeckMainColumns_OnCardAdded(CardData cardData)
	{
		if (!ContextProvider.Context.IsReadOnly)
		{
			ModelProvider.AddCardToDeckPile(DeckBuilderPile.MainDeck, cardData, ZoomHandler);
		}
	}

	public void DeckMainColumns_OnCardClicked(MetaCardView cardView)
	{
		DeckBuilderContext context = ContextProvider.Context;
		if (context.Mode != DeckBuilderMode.ReadOnly)
		{
			if (context.CanCraftDeck(Pantry.Get<DecksManager>()) && context.Mode != DeckBuilderMode.DeckBuilding)
			{
				ActionsHandler.OpenCardViewer(cardView, ZoomHandler);
			}
			else if ((context.Format.MaxCardsByTitle == 1 || context.IsLimited) && !CardUtilities.IsBasicLand(cardView.Card))
			{
				MainHolder.OnHideQuantityAdjust(repositionCards: false);
				ModelProvider.RemoveCardFromPile(DeckBuilderPile.MainDeck, ZoomHandler, cardView.Card);
			}
			else if (MainHolder.QuantityAdjust.CurrentCardData?.GrpId == cardView.Card.GrpId)
			{
				MainHolder.OnHideQuantityAdjust(repositionCards: true);
			}
			else
			{
				MainHolder.ShowQuantityAdjust((StaticColumnMetaCardView)cardView, repositionCards: true);
			}
		}
	}

	private void DeckMainColumns_OnCardUpdate(MetaCardView cardView)
	{
		if (cardView is StaticColumnMetaCardView staticColumnMetaCardView)
		{
			staticColumnMetaCardView.SetData(staticColumnMetaCardView.Card, staticColumnMetaCardView.FrontOfColumn ? CardHolderType.Collection : CardHolderType.Deckbuilder);
		}
	}

	public void SetCompanionAddButtonState(bool showAddButton)
	{
		CompanionCardHolder.SetAddButtonActive(showAddButton);
	}

	public void SetIsCommanderStack(bool isStacked)
	{
		CommanderStackCardHolder.gameObject.SetActive(isStacked);
		CommanderCardHolder.gameObject.SetActive(!isStacked);
		PartnerCardHolder.gameObject.SetActive(!isStacked);
	}

	public void OnLayoutUpdated()
	{
		bool isColumnViewExpanded = LayoutState.IsColumnViewExpanded;
		DeckBuilderContext context = ContextProvider.Context;
		bool flag = context.Mode == DeckBuilderMode.ReadOnly;
		bool flag2 = context.Mode == DeckBuilderMode.ReadOnlyCollection;
		bool num = (LayoutState.IsColumnViewExpanded && context.IsReadOnly) || flag || flag2;
		SetExpandedView(isColumnViewExpanded || flag);
		bool canEditCards = !num;
		bool canEditMetaData = !num && !context.IsSideboarding;
		SetEnabledControls(canEditCards, canEditMetaData);
		UpdateSpecialCardsActive(CardFilterProvider.Filter);
	}

	private void OnSideboardExpandChange()
	{
		if (_isSideboardExpandedOnly && !LayoutState.IsColumnViewExpanded && SideboardTitlePanel.Expanded)
		{
			OnExpandColumnViewClicked();
		}
	}

	public void OnExpandColumnViewClicked()
	{
		DeckBuilderContext context = ContextProvider.Context;
		DeckBuilderModel model = ModelProvider.Model;
		LayoutState.IsColumnViewExpanded = !LayoutState.IsColumnViewExpanded;
		MainHolder.OnHideQuantityAdjust(repositionCards: false);
		VisualsUpdater.UnsuggestedCardsInPool.Clear();
		VisualsUpdater.SuggestedCardsInDeck.Clear();
		if ((bool)this && (bool)SideboardTitlePanel && !LayoutState.IsColumnViewExpanded && _isSideboardExpandedOnly)
		{
			SideboardTitlePanel.SetExpand(expand: false);
		}
		if (context.IsLimited && LayoutState.IsColumnViewExpanded)
		{
			CardFilterProvider.ApplyFilters(FilterValueChangeSource.Miscellaneous);
			foreach (CardPrintingQuantity item in model.GetFilteredPool())
			{
				if (item.Quantity != 0)
				{
					uint num = item.Quantity - model.GetQuantityInMainDeck(item.Printing.GrpId);
					for (int i = 0; i < num; i++)
					{
						VisualsUpdater.UnsuggestedCardsInPool.Add(item.Printing);
					}
				}
			}
		}
		if (!LayoutState.IsColumnViewExpanded)
		{
			CardFilterProvider.ApplyFilters(FilterValueChangeSource.Miscellaneous);
		}
		VisualsUpdater.UpdateAllDeckVisuals();
		VisualsUpdater.RefreshPoolView(scrollToTop: true, null);
		SetExpandedView(LayoutState.IsColumnViewExpanded);
		StartCoroutine(UpdateViewYield());
		IEnumerator UpdateViewYield()
		{
			yield return new WaitForSeconds(LayoutState.IsColumnViewExpanded ? _expandColumnPoolUpdateDelay : _contractColumnPoolUpdateDelay);
			VisualsUpdater.UpdateView();
		}
	}

	private void OnAddCommanderClicked()
	{
		if (LayoutState.LayoutInUse == DeckBuilderLayout.Column && LayoutState.IsColumnViewExpanded)
		{
			OnExpandColumnViewClicked();
			return;
		}
		bool flag = CardFilterProvider.Filter.IsSet(CardFilterType.Commanders);
		CardFilterProvider.SetFilter(FilterValueChangeSource.Miscellaneous, CardFilterType.Commanders, !flag);
		CardFilterProvider.ApplyFilters(FilterValueChangeSource.Miscellaneous);
	}

	private void OnAddCompanionClicked()
	{
		bool flag = CardFilterProvider.Filter.IsSet(CardFilterType.Companions);
		CardFilterProvider.SetFilter(FilterValueChangeSource.Miscellaneous, CardFilterType.Companions, !flag);
		CardFilterProvider.ApplyFilters(FilterValueChangeSource.Miscellaneous);
	}
}
