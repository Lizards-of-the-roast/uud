using System;
using System.Collections.Generic;
using Core.Code.Decks;
using Core.Meta.Cards.Views;
using Core.Meta.MainNavigation.Cosmetics;
using Core.Shared.Code.CardFilters;
using GreClient.CardData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;
using Wizards.Mtga.Platforms;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Extensions;

public class DeckListView : MonoBehaviour
{
	[Header("Card Holder References")]
	public ListMetaCardHolder_Expanding MainDeckCardHolder;

	public SideboardListCardHolder SideboardCardHolder;

	[Header("Main Deck Title Panel References")]
	public DeckMainTitlePanel MainDeckTitlePanel;

	public TMP_InputField DeckNameInput;

	public CardDropHandler[] DeckBoxDropTargets;

	public DeckCostsSummary CostWidget;

	public CustomButton SleevesButton;

	public RectTransform SleeveCDCParent;

	public GameObject SleeveDefault;

	[Header("Sideboard Title Panel References")]
	public DeckSideboardTitlePanel SideboardTitlePanel;

	public Toggle ShowSideboardToggle;

	public Transform SideboardCardsParent;

	public GameObject SideboardToggleOn;

	public GameObject SideboardToggleOff;

	private Meta_CDC _cdc;

	private Meta_CDC[] _sideboardCDCs;

	private ICardRolloverZoom _zoomHandler;

	private static readonly int NoticeMe = Animator.StringToHash("NoticeMe");

	private DeckBuilderContextProvider ContextProvider => Pantry.Get<DeckBuilderContextProvider>();

	private DeckBuilderModelProvider ModelProvider => Pantry.Get<DeckBuilderModelProvider>();

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

	private void Awake()
	{
		ShowSideboardToggle.onValueChanged.AddListener(ShowSideboardToggle_OnValueChanged);
		SideboardToggleOn.UpdateActive(ShowSideboardToggle.isOn);
		SideboardToggleOff.UpdateActive(!ShowSideboardToggle.isOn);
		DeckNameInput.onEndEdit.AddListener(OnEndEditDeckName);
	}

	private void OnDestroy()
	{
		ShowSideboardToggle.onValueChanged.RemoveListener(ShowSideboardToggle_OnValueChanged);
		DeckNameInput.onEndEdit.RemoveListener(OnEndEditDeckName);
		SleevesButton.OnMouseover.RemoveListener(DeckBuilderWidgetUtilities.Generic_OnMouseOver);
		SleevesButton.OnClick.RemoveListener(DeckBuilderWidgetUtilities.CardBackButton_OnClick);
		ListMetaCardHolder_Expanding mainDeckCardHolder = MainDeckCardHolder;
		mainDeckCardHolder.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Remove(mainDeckCardHolder.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(OnCardDroppedImpl));
		ListMetaCardHolder_Expanding mainDeckCardHolder2 = MainDeckCardHolder;
		mainDeckCardHolder2.OnCardAddClicked = (Action<MetaCardView>)Delegate.Remove(mainDeckCardHolder2.OnCardAddClicked, new Action<MetaCardView>(DeckList_OnCardAddClicked));
		ListMetaCardHolder_Expanding mainDeckCardHolder3 = MainDeckCardHolder;
		mainDeckCardHolder3.OnCardRemoveClicked = (Action<MetaCardView>)Delegate.Remove(mainDeckCardHolder3.OnCardRemoveClicked, new Action<MetaCardView>(DeckList_OnCardRemoveClicked));
		SideboardListCardHolder sideboardCardHolder = SideboardCardHolder;
		sideboardCardHolder.OnCardAddClicked = (Action<MetaCardView>)Delegate.Remove(sideboardCardHolder.OnCardAddClicked, new Action<MetaCardView>(DeckSideboard_OnCardAddClickedImpl));
		SideboardListCardHolder sideboardCardHolder2 = SideboardCardHolder;
		sideboardCardHolder2.OnCardRemoveClicked = (Action<MetaCardView>)Delegate.Remove(sideboardCardHolder2.OnCardRemoveClicked, new Action<MetaCardView>(DeckSideboard_OnCardRemoveClickedImpl));
		SideboardListCardHolder sideboardCardHolder3 = SideboardCardHolder;
		sideboardCardHolder3.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Remove(sideboardCardHolder3.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(OnCardDroppedImpl));
		SideboardListCardHolder sideboardCardHolder4 = SideboardCardHolder;
		sideboardCardHolder4.OnCompanionCardRemoveClicked = (Action<MetaCardView>)Delegate.Remove(sideboardCardHolder4.OnCompanionCardRemoveClicked, new Action<MetaCardView>(DeckCompanion_OnCardClickedImpl));
		SideboardListCardHolder sideboardCardHolder5 = SideboardCardHolder;
		sideboardCardHolder5.OnCompanionCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Remove(sideboardCardHolder5.OnCompanionCardDropped, new Action<MetaCardView, MetaCardHolder>(OnCardDroppedImpl));
		ListCommanderHolder commanderHolder = MainDeckCardHolder._commanderHolder;
		commanderHolder.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Remove(commanderHolder.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(OnCardDroppedImpl));
		ListCommanderHolder commanderHolder2 = MainDeckCardHolder._commanderHolder;
		commanderHolder2.OnCardClicked = (Action<MetaCardView>)Delegate.Remove(commanderHolder2.OnCardClicked, new Action<MetaCardView>(DeckCommander_OnCardClickedImpl));
		ListCommanderHolder commanderHolder3 = MainDeckCardHolder._commanderHolder;
		commanderHolder3.OnCardAddClicked = (Action<MetaCardView>)Delegate.Remove(commanderHolder3.OnCardAddClicked, new Action<MetaCardView>(DeckCommander_OnCardAddClickedImpl));
		MainDeckCardHolder._commanderHolder.AddButton.OnClick.RemoveListener(OnAddCommanderClicked);
		ListCommanderHolder partnerHolder = MainDeckCardHolder._partnerHolder;
		partnerHolder.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Remove(partnerHolder.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(OnCardDroppedImpl));
		ListCommanderHolder partnerHolder2 = MainDeckCardHolder._partnerHolder;
		partnerHolder2.OnCardClicked = (Action<MetaCardView>)Delegate.Remove(partnerHolder2.OnCardClicked, new Action<MetaCardView>(DeckPartner_OnCardClickedImpl));
		ListCommanderHolder partnerHolder3 = MainDeckCardHolder._partnerHolder;
		partnerHolder3.OnCardAddClicked = (Action<MetaCardView>)Delegate.Remove(partnerHolder3.OnCardAddClicked, new Action<MetaCardView>(DeckPartner_OnCardAddClickedImpl));
		MainDeckCardHolder._partnerHolder.AddButton.OnClick.RemoveListener(OnAddCommanderClicked);
		ListCommanderHolder companionHolder = MainDeckCardHolder._companionHolder;
		companionHolder.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Remove(companionHolder.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(OnCardDroppedImpl));
		ListCommanderHolder companionHolder2 = MainDeckCardHolder._companionHolder;
		companionHolder2.OnCardClicked = (Action<MetaCardView>)Delegate.Remove(companionHolder2.OnCardClicked, new Action<MetaCardView>(DeckCompanion_OnCardClickedImpl));
		ListCommanderHolder companionHolder3 = MainDeckCardHolder._companionHolder;
		companionHolder3.OnCardAddClicked = (Action<MetaCardView>)Delegate.Remove(companionHolder3.OnCardAddClicked, new Action<MetaCardView>(DeckCommander_OnCardAddClickedImpl));
		MainDeckCardHolder._companionHolder.AddButton.OnClick.RemoveListener(OnAddCompanionClicked);
		MainDeckTitlePanel.Button.OnClick.RemoveListener(ActionsHandler.OpenDeckDetails);
		VisualsUpdater.OnLayoutUpdated -= OnLayoutUpdated;
		ActionsHandler.DeckDetailsCosmeticsSelectorRequested -= OnDeckDetailsCosmeticsSelectorRequested;
	}

	public void OnCardDroppedImpl(MetaCardView cardView, MetaCardHolder destinationHolder)
	{
		ActionsHandler.OnCardDropped(cardView, destinationHolder, ZoomHandler);
	}

	public void DeckSideboard_OnCardAddClickedImpl(MetaCardView cardView)
	{
		ActionsHandler.DeckSideboard_OnCardAddClicked(cardView, ZoomHandler);
	}

	public void DeckSideboard_OnCardRemoveClickedImpl(MetaCardView cardView)
	{
		ActionsHandler.DeckSideboard_OnCardRemoveClicked(cardView, ZoomHandler);
	}

	public void DeckCompanion_OnCardClickedImpl(MetaCardView cardView)
	{
		ActionsHandler.DeckCompanion_OnCardClicked(cardView, ZoomHandler);
	}

	public void DeckCommander_OnCardClickedImpl(MetaCardView cardView)
	{
		ActionsHandler.DeckCommander_OnCardClicked(cardView, ZoomHandler, isPartner: false);
	}

	public void DeckPartner_OnCardClickedImpl(MetaCardView cardView)
	{
		ActionsHandler.DeckCommander_OnCardClicked(cardView, ZoomHandler, isPartner: true);
	}

	public void DeckCommander_OnCardAddClickedImpl(MetaCardView cardView)
	{
		ActionsHandler.DeckCommander_OnCardAddClicked(cardView, ZoomHandler);
	}

	public void DeckPartner_OnCardAddClickedImpl(MetaCardView cardView)
	{
		ActionsHandler.DeckCommander_OnCardAddClicked(cardView, ZoomHandler);
	}

	private void OnEnable()
	{
		MainDeckCardHolder.EnsureInit(Pantry.Get<CardDatabase>(), Pantry.Get<CardViewBuilder>());
		if (MDNPlayerPrefs.FirstTimeSleeveNotify)
		{
			Animator component = SleevesButton.GetComponent<Animator>();
			if (component.isActiveAndEnabled)
			{
				component.SetBool(NoticeMe, value: true);
			}
		}
		MainDeckCardHolder._partnerHolder.gameObject.SetActive(value: false);
		ModelProvider.OnModelNameSet += OnModelNameSet;
		ModelProvider.OnDeckCardBackSet += OnDeckCardBackSet;
		ModelProvider.CardModifiedInPile += CardModifiedInPile;
		OnModelNameSet(ModelProvider.Model._deckName);
		OnDeckCardBackSet(ModelProvider.Model._cardBack);
		VisualsUpdater.MainDeckListVisualsUpdated += OnMainDeckListVisualsUpdated;
		VisualsUpdater.CommanderViewUpdated += OnCommanderViewUpdated;
		VisualsUpdater.PartnerViewUpdated += OnPartnerViewUpdated;
		VisualsUpdater.CompanionViewUpdated += OnCompanionViewUpdated;
		CardFilterProvider.OnApplyFilters += UpdateSpecialCardsActive;
		LayoutState.SideboardVisibilityUpdated += OnSideboardVisibilityUpdated;
		OnSideboardVisibilityUpdated(LayoutState.GetSideboardVisibility());
		LayoutState.IsListViewSideboardingUpdated += OnIsListViewSideboardVisualsUpdated;
		ShowMainDeckOrSideboard(LayoutState.IsListViewSideboarding);
		ActionsHandler.OnCardRightClicked += MainDeckCardHolder.ReleaseAllDraggingCards;
		CardDropHandler[] deckBoxDropTargets = DeckBoxDropTargets;
		foreach (CardDropHandler obj in deckBoxDropTargets)
		{
			obj.OnCardDropped = (Action<MetaCardView>)Delegate.Combine(obj.OnCardDropped, new Action<MetaCardView>(ModelProvider.SetDeckBoxTextureByCardData));
		}
		Pantry.Get<MetaCardViewDragState>().CompanionAddButtonStateChange += SetCompanionAddButtonState;
	}

	private void OnDisable()
	{
		ModelProvider.OnModelNameSet -= OnModelNameSet;
		ModelProvider.OnDeckCardBackSet -= OnDeckCardBackSet;
		ModelProvider.CardModifiedInPile -= CardModifiedInPile;
		VisualsUpdater.MainDeckListVisualsUpdated -= OnMainDeckListVisualsUpdated;
		VisualsUpdater.CommanderViewUpdated -= OnCommanderViewUpdated;
		VisualsUpdater.PartnerViewUpdated -= OnPartnerViewUpdated;
		VisualsUpdater.CompanionViewUpdated -= OnCompanionViewUpdated;
		CardFilterProvider.OnApplyFilters -= UpdateSpecialCardsActive;
		LayoutState.SideboardVisibilityUpdated -= OnSideboardVisibilityUpdated;
		LayoutState.IsListViewSideboardingUpdated -= OnIsListViewSideboardVisualsUpdated;
		ActionsHandler.OnCardRightClicked -= MainDeckCardHolder.ReleaseAllDraggingCards;
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

	public void DeckBuilderWidgetInit(ICardRolloverZoom zoomHandler, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, CardBackCatalog cardBackCatalog)
	{
		GetComponent<RectTransform>().StretchToParent();
		_zoomHandler = zoomHandler;
		MainDeckCardHolder.EnsureInit(cardDatabase, cardViewBuilder);
		MainDeckCardHolder.RolloverZoomView = zoomHandler;
		SideboardCardHolder.EnsureInit(cardDatabase, cardViewBuilder);
		SideboardCardHolder.RolloverZoomView = zoomHandler;
		SideboardCardHolder.CompanionCardHolder.RolloverZoomView = zoomHandler;
		MainDeckCardHolder._commanderHolder.EnsureInit(cardDatabase, cardViewBuilder);
		MainDeckCardHolder._commanderHolder.RolloverZoomView = zoomHandler;
		MainDeckCardHolder._commanderHolder.AddButton.OnClick.AddListener(OnAddCommanderClicked);
		MainDeckCardHolder._partnerHolder.EnsureInit(cardDatabase, cardViewBuilder);
		MainDeckCardHolder._partnerHolder.RolloverZoomView = zoomHandler;
		MainDeckCardHolder._partnerHolder.AddButton.OnClick.AddListener(OnAddCommanderClicked);
		MainDeckCardHolder._companionHolder.EnsureInit(cardDatabase, cardViewBuilder);
		MainDeckCardHolder._companionHolder.RolloverZoomView = zoomHandler;
		MainDeckCardHolder._companionHolder.AddButton.OnClick.AddListener(OnAddCompanionClicked);
		ListMetaCardHolder_Expanding mainDeckCardHolder = MainDeckCardHolder;
		mainDeckCardHolder.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Combine(mainDeckCardHolder.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(OnCardDroppedImpl));
		ListMetaCardHolder_Expanding mainDeckCardHolder2 = MainDeckCardHolder;
		mainDeckCardHolder2.OnCardAddClicked = (Action<MetaCardView>)Delegate.Combine(mainDeckCardHolder2.OnCardAddClicked, new Action<MetaCardView>(DeckList_OnCardAddClicked));
		ListMetaCardHolder_Expanding mainDeckCardHolder3 = MainDeckCardHolder;
		mainDeckCardHolder3.OnCardRemoveClicked = (Action<MetaCardView>)Delegate.Combine(mainDeckCardHolder3.OnCardRemoveClicked, new Action<MetaCardView>(DeckList_OnCardRemoveClicked));
		MainDeckCardHolder.CanDropCards = deckListCardHolderCanDropCards;
		MainDeckCardHolder.ShowHighlight = (MetaCardView _) => false;
		MainDeckCardHolder.CanDragCards = (MetaCardView _) => !PlatformUtils.IsHandheld();
		SideboardListCardHolder sideboardCardHolder = SideboardCardHolder;
		sideboardCardHolder.OnCardAddClicked = (Action<MetaCardView>)Delegate.Combine(sideboardCardHolder.OnCardAddClicked, new Action<MetaCardView>(DeckSideboard_OnCardAddClickedImpl));
		SideboardListCardHolder sideboardCardHolder2 = SideboardCardHolder;
		sideboardCardHolder2.OnCardRemoveClicked = (Action<MetaCardView>)Delegate.Combine(sideboardCardHolder2.OnCardRemoveClicked, new Action<MetaCardView>(DeckSideboard_OnCardRemoveClickedImpl));
		SideboardListCardHolder sideboardCardHolder3 = SideboardCardHolder;
		sideboardCardHolder3.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Combine(sideboardCardHolder3.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(OnCardDroppedImpl));
		SideboardListCardHolder sideboardCardHolder4 = SideboardCardHolder;
		sideboardCardHolder4.OnCompanionCardRemoveClicked = (Action<MetaCardView>)Delegate.Combine(sideboardCardHolder4.OnCompanionCardRemoveClicked, new Action<MetaCardView>(DeckCompanion_OnCardClickedImpl));
		SideboardListCardHolder sideboardCardHolder5 = SideboardCardHolder;
		sideboardCardHolder5.OnCompanionCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Combine(sideboardCardHolder5.OnCompanionCardDropped, new Action<MetaCardView, MetaCardHolder>(OnCardDroppedImpl));
		SideboardCardHolder.CanDropCards = (MetaCardView _) => true;
		SideboardCardHolder.CanDragCards = (MetaCardView _) => !PlatformUtils.IsHandheld();
		ListCommanderHolder commanderHolder = MainDeckCardHolder._commanderHolder;
		commanderHolder.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Combine(commanderHolder.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(OnCardDroppedImpl));
		ListCommanderHolder commanderHolder2 = MainDeckCardHolder._commanderHolder;
		commanderHolder2.OnCardClicked = (Action<MetaCardView>)Delegate.Combine(commanderHolder2.OnCardClicked, new Action<MetaCardView>(DeckCommander_OnCardClickedImpl));
		ListCommanderHolder commanderHolder3 = MainDeckCardHolder._commanderHolder;
		commanderHolder3.OnCardAddClicked = (Action<MetaCardView>)Delegate.Combine(commanderHolder3.OnCardAddClicked, new Action<MetaCardView>(DeckCommander_OnCardAddClickedImpl));
		MainDeckCardHolder._commanderHolder.CanDropCards = (MetaCardView _) => true;
		MainDeckCardHolder._commanderHolder.CanDoubleClickCards = (MetaCardView _) => false;
		MainDeckCardHolder._commanderHolder.CanSingleClickCards = (MetaCardView _) => true;
		MainDeckCardHolder._commanderHolder.CanDragCards = (MetaCardView _) => !PlatformUtils.IsHandheld();
		ListCommanderHolder partnerHolder = MainDeckCardHolder._partnerHolder;
		partnerHolder.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Combine(partnerHolder.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(OnCardDroppedImpl));
		ListCommanderHolder partnerHolder2 = MainDeckCardHolder._partnerHolder;
		partnerHolder2.OnCardClicked = (Action<MetaCardView>)Delegate.Combine(partnerHolder2.OnCardClicked, new Action<MetaCardView>(DeckPartner_OnCardClickedImpl));
		ListCommanderHolder partnerHolder3 = MainDeckCardHolder._partnerHolder;
		partnerHolder3.OnCardAddClicked = (Action<MetaCardView>)Delegate.Combine(partnerHolder3.OnCardAddClicked, new Action<MetaCardView>(DeckPartner_OnCardAddClickedImpl));
		MainDeckCardHolder._partnerHolder.CanDropCards = (MetaCardView _) => true;
		MainDeckCardHolder._partnerHolder.CanDoubleClickCards = (MetaCardView _) => false;
		MainDeckCardHolder._partnerHolder.CanSingleClickCards = (MetaCardView _) => true;
		MainDeckCardHolder._partnerHolder.CanDragCards = (MetaCardView _) => !PlatformUtils.IsHandheld();
		MainDeckCardHolder._companionHolder.IsCompanion = true;
		ListCommanderHolder companionHolder = MainDeckCardHolder._companionHolder;
		companionHolder.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Combine(companionHolder.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(OnCardDroppedImpl));
		ListCommanderHolder companionHolder2 = MainDeckCardHolder._companionHolder;
		companionHolder2.OnCardClicked = (Action<MetaCardView>)Delegate.Combine(companionHolder2.OnCardClicked, new Action<MetaCardView>(DeckCompanion_OnCardClickedImpl));
		ListCommanderHolder companionHolder3 = MainDeckCardHolder._companionHolder;
		companionHolder3.OnCardAddClicked = (Action<MetaCardView>)Delegate.Combine(companionHolder3.OnCardAddClicked, new Action<MetaCardView>(DeckCommander_OnCardAddClickedImpl));
		MainDeckCardHolder._companionHolder.CanDropCards = (MetaCardView _) => true;
		MainDeckCardHolder._companionHolder.CanDoubleClickCards = (MetaCardView _) => false;
		MainDeckCardHolder._companionHolder.CanSingleClickCards = (MetaCardView _) => true;
		MainDeckCardHolder._companionHolder.CanDragCards = (MetaCardView _) => !PlatformUtils.IsHandheld();
		MainDeckTitlePanel.Button.OnClick.AddListener(ActionsHandler.OpenDeckDetails);
		VisualsUpdater.OnLayoutUpdated += OnLayoutUpdated;
		ActionsHandler.DeckDetailsCosmeticsSelectorRequested += OnDeckDetailsCosmeticsSelectorRequested;
		if (cardBackCatalog != null && cardBackCatalog.Count > 0)
		{
			SleevesButton.OnMouseover.AddListener(DeckBuilderWidgetUtilities.Generic_OnMouseOver);
			SleevesButton.OnClick.AddListener(DeckBuilderWidgetUtilities.CardBackButton_OnClick);
			return;
		}
		Animator component = SleevesButton.GetComponent<Animator>();
		if (component != null)
		{
			component.enabled = false;
		}
		bool deckListCardHolderCanDropCards(MetaCardView cardView)
		{
			if (ContextProvider.Context.IsEditingDeck)
			{
				return !cardView.Card.IsWildcard;
			}
			return false;
		}
	}

	private void ShowSideboardToggle_OnValueChanged(bool value)
	{
		if (LayoutState.IsListViewSideboarding != value)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
			LayoutState.IsListViewSideboarding = value;
		}
	}

	public void ShowMainDeckOrSideboard(bool value)
	{
		MainDeckCardHolder.SetActive(!value);
		MainDeckTitlePanel.SetActive(!value);
		SideboardToggleOff.UpdateActive(!value);
		SideboardCardHolder.SetActive(value);
		SideboardTitlePanel.SetActive(value);
		SideboardToggleOn.UpdateActive(value);
	}

	private void OnDeckCardBackSet(string cardBack)
	{
		SetSleeve(cardBack, Pantry.Get<CardViewBuilder>(), Pantry.Get<CardDatabase>());
	}

	public void SetSleeve(string cardBack, CardViewBuilder builder, CardDatabase cardDatabase)
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

	public void CardModifiedInPile(DeckBuilderPile pile, CardData card)
	{
		if (pile != DeckBuilderPile.MainDeck)
		{
			return;
		}
		DeckBuilderLayoutState deckBuilderLayoutState = Pantry.Get<DeckBuilderLayoutState>();
		if (deckBuilderLayoutState.LayoutInUse == DeckBuilderLayout.List)
		{
			CardPrintingData cardPrintingById = Pantry.Get<ICardDatabaseAdapter>().CardDataProvider.GetCardPrintingById(card.GrpId);
			if (!deckBuilderLayoutState.IsListViewSideboarding)
			{
				MainDeckCardHolder.ScrollToGrpId(cardPrintingById.GrpId);
			}
		}
	}

	public void UpdateSpecialCardsActive(IReadOnlyCardFilter filter)
	{
		bool flag = filter.IsSet(CardFilterType.Commanders);
		MainDeckCardHolder._commanderHolder.SetAddButtonActive(flag);
		bool flag2 = DeckBuilderWidgetUtilities.HasCommanderSet(ContextProvider.Context, ModelProvider.Model) == DeckBuilderWidgetUtilities.CommanderType.PartnerableCommander;
		MainDeckCardHolder._partnerHolder.SetAddButtonActive(flag2 && flag);
		bool addButtonActive = filter.IsSet(CardFilterType.Companions);
		MainDeckCardHolder._companionHolder.SetAddButtonActive(addButtonActive);
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
		SleevesButton.enabled = canEditMetaData;
		SleevesButton.GetComponent<Animator>().enabled = canEditMetaData;
		DeckNameInput.enabled = canEditMetaData;
		CardDropHandler[] deckBoxDropTargets = DeckBoxDropTargets;
		for (int i = 0; i < deckBoxDropTargets.Length; i++)
		{
			deckBoxDropTargets[i].SetInteractable(canEditMetaData);
		}
	}

	public void OnDeckDetailsCosmeticsSelectorRequested(CardDatabase cardDatabase, DisplayCosmeticsTypes cosmeticType)
	{
		DisableNoticeMe();
	}

	public void DisableNoticeMe()
	{
		MDNPlayerPrefs.FirstTimeSleeveNotify = false;
		Animator component = SleevesButton.GetComponent<Animator>();
		if (component.isActiveAndEnabled)
		{
			component.SetBool(NoticeMe, value: false);
		}
	}

	public void OnSideboardVisibilityUpdated(bool showSideboard)
	{
		ShowSideboardToggle.gameObject.SetActive(showSideboard);
		ShowSideboardToggle.isOn = LayoutState.IsListViewSideboarding;
	}

	public void OnMainDeckListVisualsUpdated(List<ListMetaCardViewDisplayInformation> displayInfos)
	{
		MainDeckCardHolder.SetCards(displayInfos);
	}

	public void OnCommanderViewUpdated(ListMetaCardViewDisplayInformation display, PagesMetaCardViewDisplayInformation pageDisplay, bool isReadOnly)
	{
		if (display != null && !(display.Card == null && isReadOnly))
		{
			MainDeckCardHolder.SetCommanderCards(display);
		}
		else
		{
			MainDeckCardHolder.ClearCommanderCards();
		}
	}

	public void OnPartnerViewUpdated(ListMetaCardViewDisplayInformation display, PagesMetaCardViewDisplayInformation pageDisplay, bool isReadOnly)
	{
		if (display != null && !(display.Card == null && isReadOnly))
		{
			MainDeckCardHolder.SetPartnerCards(display);
		}
		else
		{
			MainDeckCardHolder.ClearPartnerCards();
		}
	}

	public void OnCompanionViewUpdated(ListMetaCardViewDisplayInformation display, PagesMetaCardViewDisplayInformation pageDisplay, bool isReadOnly)
	{
		if (display != null && !(display.Card == null && isReadOnly))
		{
			MainDeckCardHolder.SetCompanionCards(display);
			SideboardCardHolder.SetCompanionCards(display);
		}
		else
		{
			MainDeckCardHolder.ClearCompanionCards();
			SideboardCardHolder.ClearCompanionCards();
		}
	}

	public void OnIsListViewSideboardVisualsUpdated(bool isListViewSideboarding)
	{
		ShowMainDeckOrSideboard(isListViewSideboarding);
		VisualsUpdater.UpdateAllDeckVisuals();
		if (ContextProvider.Context.IsReadOnly)
		{
			VisualsUpdater.RefreshPoolView(scrollToTop: true, null);
		}
	}

	private void DeckList_OnCardAddClicked(MetaCardView cardView)
	{
		int num = 0;
		if (ContextProvider.Context.CanCraft && cardView.ShowUnCollectedTreatment)
		{
			uint quantityInWholeDeck = ModelProvider.Model.GetQuantityInWholeDeck(cardView.Card.GrpId);
			int quantityInCardPool = (int)ModelProvider.Model.GetQuantityInCardPool(cardView.Card.GrpId);
			num = (int)quantityInWholeDeck - quantityInCardPool;
		}
		if (num > 0)
		{
			ActionsHandler.OpenCardViewer(cardView, ZoomHandler, num);
			return;
		}
		DeckBuilderPile pile = (LayoutState.IsListViewSideboarding ? DeckBuilderPile.Sideboard : DeckBuilderPile.MainDeck);
		ModelProvider.AddCardToDeckPile(pile, cardView.Card, ZoomHandler);
	}

	private void DeckList_OnCardRemoveClicked(MetaCardView cardView)
	{
		if (!ContextProvider.Context.IsReadOnly)
		{
			if (ContextProvider.Context.CanCraft && ContextProvider.Context.Mode != DeckBuilderMode.DeckBuilding)
			{
				ActionsHandler.OpenCardViewer(cardView, ZoomHandler);
				return;
			}
			DeckBuilderPile pile = (LayoutState.IsListViewSideboarding ? DeckBuilderPile.Sideboard : DeckBuilderPile.MainDeck);
			ModelProvider.RemoveCardFromPile(pile, ZoomHandler, cardView.Card);
		}
	}

	public void SetCompanionAddButtonState(bool showAddButton)
	{
		MainDeckCardHolder._companionHolder.SetAddButtonActive(showAddButton);
	}

	private void OnLayoutUpdated()
	{
		DeckBuilderContext context = ContextProvider.Context;
		bool flag = context.Mode == DeckBuilderMode.ReadOnly;
		bool flag2 = context.Mode == DeckBuilderMode.ReadOnlyCollection;
		bool num = (LayoutState.IsColumnViewExpanded && context.IsReadOnly) || flag || flag2;
		bool canEditCards = !num;
		bool canEditMetaData = !num && !context.IsSideboarding;
		SetEnabledControls(canEditCards, canEditMetaData);
		UpdateSpecialCardsActive(CardFilterProvider.Filter);
	}

	private void OnAddCommanderClicked()
	{
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
