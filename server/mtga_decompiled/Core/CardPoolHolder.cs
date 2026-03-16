using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Code.Decks;
using Core.Shared.Code.CardFilters;
using DG.Tweening;
using GreClient.CardData;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Wizards.Mtga;
using Wizards.Mtga.Platforms;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class CardPoolHolder : MetaCardHolder, IScrollHandler, IEventSystemHandler
{
	private class Page
	{
		public RectTransform Trans;

		public readonly List<PagesMetaCardView> CardViews = new List<PagesMetaCardView>();
	}

	[SerializeField]
	protected PagesMetaCardView _cardPrefab;

	[SerializeField]
	protected RectTransform _pageParent;

	[SerializeField]
	private CustomButton _previousButton;

	[SerializeField]
	private CustomButton _nextButton;

	[SerializeField]
	private TMP_Text _noCardsText;

	[SerializeField]
	private float _pageSpacing = 500f;

	[SerializeField]
	private float _scrollTime = 0.2f;

	[Tooltip("The pool will shrink the views a little to allow for an extra column, up to this percent of total width.")]
	[SerializeField]
	private Ease _scrollEase = Ease.OutCubic;

	[SerializeField]
	private float _maxShrinkPercent = 0.4f;

	[Tooltip("This is how frequently the pool will attempt to load offscreen cards in order to increase scroll performance")]
	[SerializeField]
	protected float _offscreenLoadPulse = 0.1f;

	[Header("Card Rows")]
	[SerializeField]
	private int _VerticalViewRowsLarge = 2;

	[SerializeField]
	private int _VerticalViewRowsSmall = 3;

	[SerializeField]
	private int _ColumnViewRowsLarge = 1;

	[SerializeField]
	private int _ColumnViewRowsSmall = 2;

	protected int Columns;

	private List<Page> _pages;

	private List<Vector3> _pagePositions;

	protected Rect _lastRect;

	protected int _lastRows;

	protected IReadOnlyList<PagesMetaCardViewDisplayInformation> _cardDisplayInfos;

	protected List<Func<CardFilterGroup, CardFilterGroup>> _savedFilters;

	protected bool _cardsDirty;

	protected bool _forceCardsLocRefresh;

	private int _currentPage;

	private bool _isScrolling;

	protected float _offscreenLoadTimer;

	protected int _offscreenLoadIndex;

	protected bool _useNewTags;

	protected RectTransform _rectTransform;

	protected bool _usesPaging = true;

	protected int Rows => ExpectedPoolRows();

	protected RectTransform RectTransform
	{
		get
		{
			if (_rectTransform == null)
			{
				_rectTransform = GetComponent<RectTransform>();
			}
			return _rectTransform;
		}
	}

	private int PageSize => Rows * Columns;

	private int PageCount => Mathf.Max(1, Mathf.CeilToInt((float)_cardDisplayInfos.Count / (float)PageSize));

	public TMP_Text NoCardsText => _noCardsText;

	private DeckBuilderContextProvider ContextProvider => Pantry.Get<DeckBuilderContextProvider>();

	private DeckBuilderModelProvider ModelProvider => Pantry.Get<DeckBuilderModelProvider>();

	private DeckBuilderVisualsUpdater VisualsUpdater => Pantry.Get<DeckBuilderVisualsUpdater>();

	private DeckBuilderCardFilterProvider CardFilterProvider => Pantry.Get<DeckBuilderCardFilterProvider>();

	private DeckBuilderPreferredPrintingState PreferredPrintingState => Pantry.Get<DeckBuilderPreferredPrintingState>();

	private DeckBuilderActionsHandler ActionsHandler => Pantry.Get<DeckBuilderActionsHandler>();

	private DeckBuilderLayoutState LayoutState => Pantry.Get<DeckBuilderLayoutState>();

	public ICardRolloverZoom DirectlySetZoomHandler { get; set; }

	private ICardRolloverZoom ZoomHandler
	{
		get
		{
			if (DirectlySetZoomHandler == null)
			{
				return Pantry.Get<ICardRolloverZoom>();
			}
			return DirectlySetZoomHandler;
		}
	}

	public event Action<int> OnPageChanged;

	protected virtual void Awake()
	{
		if (_usesPaging)
		{
			_pages = new List<Page>(3);
			for (int i = 0; i < 3; i++)
			{
				RectTransform component = new GameObject("Page" + i, typeof(RectTransform)).GetComponent<RectTransform>();
				component.SetParent(_pageParent);
				component.ZeroOut();
				_pages.Add(new Page
				{
					Trans = component
				});
			}
		}
		_cardDisplayInfos = new List<PagesMetaCardViewDisplayInformation>();
		Languages.LanguageChangedSignal.Listeners += ResetLanguage;
		VisualsUpdater.OnPoolViewRefreshed += UpdateViewData;
		Dictionary<uint, uint> unownedCardSlots = ModelProvider.Model.GetCardsNeededToFinishDeck().ToDictionary((KeyValuePair<uint, CardPrintingQuantity> k) => k.Value.Printing.GrpId, (KeyValuePair<uint, CardPrintingQuantity> v) => v.Value.Quantity);
		(IReadOnlyList<PagesMetaCardViewDisplayInformation>, bool) cardPoolViewInfo = VisualsUpdater.GetCardPoolViewInfo(unownedCardSlots);
		UpdateViewData(cardPoolViewInfo.Item1, scrollToTop: false, cardPoolViewInfo.Item2);
	}

	private void OnDestroy()
	{
		Languages.LanguageChangedSignal.Listeners -= ResetLanguage;
		base.OnCardDragged = (Action<MetaCardView>)Delegate.Remove(base.OnCardDragged, new Action<MetaCardView>(CardDragged));
		base.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Remove(base.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(OnCardDroppedImpl));
		base.OnCardClicked = (Action<MetaCardView>)Delegate.Remove(base.OnCardClicked, new Action<MetaCardView>(OnCardClickedImpl));
		base.OnCardRightClicked = (Action<MetaCardView>)Delegate.Remove(base.OnCardRightClicked, new Action<MetaCardView>(OnCardRightClickedImpl));
		ActionsHandler.OnCardRightClicked -= base.ReleaseAllDraggingCards;
		VisualsUpdater.OnPoolViewRefreshed -= UpdateViewData;
	}

	private void OnEnable()
	{
		ModelProvider.OnDeckFormatSetResetPool += OnDeckFormatPoolReset;
		VisualsUpdater.CommanderViewUpdated += OnCommanderViewUpdated;
		VisualsUpdater.PartnerViewUpdated += OnPartnerViewUpdated;
		VisualsUpdater.CompanionViewUpdated += OnCompanionViewUpdated;
		CardFilterProvider.OnUpdatedFilterFunctions += SetSavedFilters;
		PreferredPrintingState.OnPreferredPrintingsChanged += SetCardsDirty;
	}

	private void OnDisable()
	{
		ModelProvider.OnDeckFormatSetResetPool -= OnDeckFormatPoolReset;
		VisualsUpdater.CommanderViewUpdated -= OnCommanderViewUpdated;
		VisualsUpdater.PartnerViewUpdated -= OnPartnerViewUpdated;
		VisualsUpdater.CompanionViewUpdated -= OnCompanionViewUpdated;
		CardFilterProvider.OnUpdatedFilterFunctions -= SetSavedFilters;
		PreferredPrintingState.OnPreferredPrintingsChanged -= SetCardsDirty;
	}

	protected override void Init(CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		base.Init(cardDatabase, cardViewBuilder);
		_previousButton.OnClick.AddListener(ScrollPrevious);
		_previousButton.OnMouseover.AddListener(Button_OnMouseover);
		_nextButton.OnClick.AddListener(ScrollNext);
		_nextButton.OnMouseover.AddListener(Button_OnMouseover);
		base.OnCardDragged = (Action<MetaCardView>)Delegate.Combine(base.OnCardDragged, new Action<MetaCardView>(CardDragged));
		base.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Combine(base.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(OnCardDroppedImpl));
		base.OnCardClicked = (Action<MetaCardView>)Delegate.Combine(base.OnCardClicked, new Action<MetaCardView>(OnCardClickedImpl));
		base.OnCardRightClicked = (Action<MetaCardView>)Delegate.Combine(base.OnCardRightClicked, new Action<MetaCardView>(OnCardRightClickedImpl));
		base.CanSingleClickCards = (MetaCardView _) => true;
		base.CanDragCards = poolCardHolderCanDragCards;
		base.CanDropCards = poolCardHolderCanDropCards;
		ActionsHandler.OnCardRightClicked += base.ReleaseAllDraggingCards;
		bool poolCardHolderCanDragCards(MetaCardView cardView)
		{
			DeckBuilderContext context = ContextProvider.Context;
			if (context == null)
			{
				return false;
			}
			if (context.IsReadOnly || PlatformUtils.IsHandheld())
			{
				return false;
			}
			if (cardView == null || cardView.Card == null)
			{
				return false;
			}
			if (context.IsEditingDeck)
			{
				return !cardView.Card.IsWildcard;
			}
			return false;
		}
		bool poolCardHolderCanDropCards(MetaCardView cardView)
		{
			DeckBuilderContext context = ContextProvider.Context;
			if (context.IsReadOnly)
			{
				return false;
			}
			return context.IsEditingDeck;
		}
	}

	public void OnCardDroppedImpl(MetaCardView cardView, MetaCardHolder destinationHolder)
	{
		ActionsHandler.OnCardDropped(cardView, destinationHolder, ZoomHandler);
	}

	public void OnCardClickedImpl(MetaCardView cardView)
	{
		ActionsHandler.OnCardClicked(cardView, ZoomHandler);
	}

	public void OnCardRightClickedImpl(MetaCardView cardView)
	{
		ActionsHandler.CardRightClicked(cardView, ZoomHandler);
	}

	private static void Button_OnMouseover()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, AudioManager.Default);
	}

	public void SetCardsDirty()
	{
		_cardsDirty = true;
	}

	public void DismissNew(ICardDataAdapter card)
	{
		if (!_useNewTags)
		{
			return;
		}
		Dictionary<uint, int> newCards = WrapperController.Instance.InventoryManager.newCards;
		if (newCards == null || !newCards.ContainsKey(card.GrpId))
		{
			Dictionary<uint, int> cardsToTagNew = WrapperController.Instance.InventoryManager.CardsToTagNew;
			if (cardsToTagNew == null || !cardsToTagNew.ContainsKey(card.GrpId))
			{
				return;
			}
		}
		WrapperController.Instance.InventoryManager.AcknowledgeNew(card.GrpId);
	}

	public void SetCards(IReadOnlyList<PagesMetaCardViewDisplayInformation> displayInfo, bool useNewTags)
	{
		_cardDisplayInfos = displayInfo;
		_cardsDirty = true;
		_useNewTags = useNewTags;
	}

	private void OnDeckFormatPoolReset()
	{
		ClearCards();
	}

	public override void ClearCards()
	{
		_cardDisplayInfos = new List<PagesMetaCardViewDisplayInformation>();
		_cardsDirty = true;
		_currentPage = 0;
		this.OnPageChanged?.Invoke(_currentPage);
	}

	protected void ResetLanguage()
	{
		SetForceCardsLocRefresh(refresh: true);
	}

	public void SetForceCardsLocRefresh(bool refresh = false)
	{
		_forceCardsLocRefresh = refresh;
	}

	public virtual void ScrollToTop()
	{
		_cardsDirty = true;
		_currentPage = 0;
		this.OnPageChanged?.Invoke(_currentPage);
	}

	public virtual bool ScrollForCollapsedIfNeeded(PagesMetaCardView cardView)
	{
		uint titleId = cardView.TitleId;
		int num = PageForFirstOfTitleId(titleId);
		if (num != _currentPage)
		{
			ScrollToPage(num);
			return true;
		}
		return false;
	}

	private int PageForFirstOfTitleId(uint titleId)
	{
		int num = _cardDisplayInfos.FindIndex(delegate(PagesMetaCardViewDisplayInformation a)
		{
			CardPrintingData card = a.Card;
			return card != null && card.TitleId == titleId;
		});
		return ((num != -1) ? num : 0) / PageSize;
	}

	private void ScrollToPage(int page)
	{
		_cardsDirty = true;
		_currentPage = page;
		this.OnPageChanged?.Invoke(_currentPage);
	}

	public virtual void OnScroll(PointerEventData eventData)
	{
		if (!base.IsDragging && base.IsPointerOver)
		{
			if (eventData.scrollDelta.y > 0f)
			{
				ScrollPrevious();
			}
			else
			{
				ScrollNext();
			}
			eventData.Use();
		}
	}

	public void OnCommanderViewUpdated(ListMetaCardViewDisplayInformation display, PagesMetaCardViewDisplayInformation pageDisplay, bool isReadOnly)
	{
		SetCommanderCards(pageDisplay);
	}

	public void OnPartnerViewUpdated(ListMetaCardViewDisplayInformation display, PagesMetaCardViewDisplayInformation pageDisplay, bool isReadOnly)
	{
		if (pageDisplay != null)
		{
			SetPartnerCards(pageDisplay);
		}
		else
		{
			ClearPartnerCards();
		}
	}

	public void OnCompanionViewUpdated(ListMetaCardViewDisplayInformation display, PagesMetaCardViewDisplayInformation pageDisplay, bool isReadOnly)
	{
		SetCompanionCards(pageDisplay);
	}

	public virtual void SetCommanderCards(PagesMetaCardViewDisplayInformation di)
	{
	}

	public virtual void SetPartnerCards(PagesMetaCardViewDisplayInformation di)
	{
	}

	public virtual void SetCompanionCards(PagesMetaCardViewDisplayInformation di)
	{
	}

	public virtual void ClearCommanderCards()
	{
	}

	public virtual void ClearPartnerCards()
	{
	}

	public virtual void ClearCompanionCards()
	{
	}

	public virtual void SetSavedFilters(List<Func<CardFilterGroup, CardFilterGroup>> filters)
	{
		if (Pantry.Get<DeckBuilderContextProvider>().Context.IsReadOnly)
		{
			_savedFilters = filters;
		}
	}

	private void ScrollPrevious()
	{
		if (!_isScrolling)
		{
			if (_currentPage > 0)
			{
				_pages = new List<Page>
				{
					_pages[2],
					_pages[0],
					_pages[1]
				};
				_pageParent.localPosition = _pagePositions[0];
				_currentPage--;
				_cardsDirty = true;
				this.OnPageChanged?.Invoke(_currentPage);
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_deck_page_turn, AudioManager.Default);
			}
			else
			{
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_invalid, AudioManager.Default);
			}
		}
	}

	private void ScrollNext()
	{
		if (!_isScrolling)
		{
			if (_currentPage < PageCount - 1)
			{
				_pages = new List<Page>
				{
					_pages[1],
					_pages[2],
					_pages[0]
				};
				_pageParent.localPosition = _pagePositions[2];
				_currentPage++;
				_cardsDirty = true;
				this.OnPageChanged?.Invoke(_currentPage);
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_deck_page_turn, AudioManager.Default);
			}
			else
			{
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_invalid, AudioManager.Default);
			}
		}
	}

	protected virtual void Update()
	{
		if (_isScrolling)
		{
			return;
		}
		int num;
		if (!(_lastRect != RectTransform.rect))
		{
			num = ((_lastRows != Rows) ? 1 : 0);
			if (num == 0)
			{
				goto IL_003d;
			}
		}
		else
		{
			num = 1;
		}
		UpdateLayout();
		goto IL_003d;
		IL_003d:
		bool cardsDirty = _cardsDirty;
		if (_cardsDirty)
		{
			UpdateCards();
		}
		if (num == 0 && !cardsDirty)
		{
			UpdateOffscreenCards();
		}
	}

	private void UpdateLayout()
	{
		_cardsDirty = true;
		_currentPage = 0;
		_lastRect = RectTransform.rect;
		_lastRows = Rows;
		float num = _lastRect.height / (float)Rows;
		Rect rect = _cardPrefab.RectTransform.rect;
		float num2 = num / rect.height;
		float num3 = num2 * rect.width;
		float num4 = _lastRect.width / num3;
		Columns = Mathf.FloorToInt(num4);
		if (num4 - (float)Columns >= 1f - _maxShrinkPercent)
		{
			Columns++;
			num3 = _lastRect.width / (float)Columns;
			num2 = num3 / rect.width;
			num = num2 * rect.height;
		}
		if (Columns < 1)
		{
			Columns = 1;
		}
		int pageSize = PageSize;
		float num5 = -0.5f * num3 * (float)(Columns - 1);
		float num6 = 0.5f * num * (float)(Rows - 1);
		foreach (Page page in _pages)
		{
			for (int i = page.CardViews.Count; i < pageSize; i++)
			{
				PagesMetaCardView pagesMetaCardView = UnityEngine.Object.Instantiate(_cardPrefab);
				pagesMetaCardView.Holder = this;
				pagesMetaCardView.Init(base.CardDatabase, base.CardViewBuilder);
				ICardRolloverZoom rolloverZoomView = pagesMetaCardView.Holder.RolloverZoomView;
				rolloverZoomView.OnRolloverStart = (Action<ICardDataAdapter>)Delegate.Combine(rolloverZoomView.OnRolloverStart, new Action<ICardDataAdapter>(DismissNew));
				pagesMetaCardView.OnPrefPrintExpansionToggled = (Action<PagesMetaCardView>)Delegate.Combine(pagesMetaCardView.OnPrefPrintExpansionToggled, new Action<PagesMetaCardView>(CardExpansionToggled));
				pagesMetaCardView.OnPreferredPrintingToggleClicked = (Action<PagesMetaCardView, bool>)Delegate.Combine(pagesMetaCardView.OnPreferredPrintingToggleClicked, new Action<PagesMetaCardView, bool>(CardPreferredPrintingToggleClicked));
				page.CardViews.Add(pagesMetaCardView);
			}
			for (int j = 0; j < page.CardViews.Count; j++)
			{
				PagesMetaCardView pagesMetaCardView2 = page.CardViews[j];
				if (j < pageSize)
				{
					Transform obj = pagesMetaCardView2.transform;
					obj.SetParent(page.Trans);
					obj.localScale = num2 * Vector3.one;
					obj.localRotation = Quaternion.Euler(Vector3.zero);
					int num7 = j % Columns;
					int num8 = j / Columns;
					obj.localPosition = new Vector3(num5 + (float)num7 * num3, num6 + (float)num8 * (0f - num));
				}
				else
				{
					pagesMetaCardView2.gameObject.UpdateActive(active: false);
				}
			}
		}
		_pagePositions = new List<Vector3>
		{
			new Vector3(0f - (_lastRect.width + _pageSpacing), 0f, 0f),
			Vector3.zero,
			new Vector3(_lastRect.width + _pageSpacing, 0f, 0f)
		};
		this.OnPageChanged?.Invoke(_currentPage);
	}

	private void UpdateCards()
	{
		_cardsDirty = false;
		_offscreenLoadIndex = 0;
		_offscreenLoadTimer = 0f;
		for (int i = 0; i < 3; i++)
		{
			_pages[i].Trans.localPosition = _pagePositions[i];
		}
		if (_pageParent.localPosition.x != 0f)
		{
			StartCoroutine(Coroutine_AnimatePage());
		}
		int pageSize = PageSize;
		for (int j = 0; j < pageSize; j++)
		{
			UpdateCardView(_currentPage, _pages[1].CardViews, j);
		}
		_forceCardsLocRefresh = false;
		_previousButton.gameObject.UpdateActive(_currentPage > 0);
		_nextButton.gameObject.UpdateActive(_currentPage < PageCount - 1);
	}

	private IEnumerator Coroutine_AnimatePage()
	{
		BeginScrolling();
		yield return 0;
		yield return _pageParent.DOLocalMoveX(0f, _scrollTime).SetEase(_scrollEase).WaitForCompletion();
		EndScrolling();
	}

	private void UpdateOffscreenCards()
	{
		_offscreenLoadTimer += Time.deltaTime;
		if (!(_offscreenLoadTimer >= _offscreenLoadPulse))
		{
			return;
		}
		_offscreenLoadTimer = 0f;
		int pageSize = PageSize;
		while (true)
		{
			if (_offscreenLoadIndex < pageSize)
			{
				if (_currentPage < PageCount - 1)
				{
					if (UpdateCardView(_currentPage + 1, _pages[2].CardViews, _offscreenLoadIndex++))
					{
						break;
					}
				}
				else
				{
					_offscreenLoadIndex = pageSize;
				}
			}
			else if (_offscreenLoadIndex >= pageSize * 2 || _currentPage <= 0 || UpdateCardView(_currentPage - 1, _pages[0].CardViews, _offscreenLoadIndex++ - pageSize))
			{
				break;
			}
		}
	}

	private bool UpdateCardView(int cardPage, List<PagesMetaCardView> cardViews, int viewIndex)
	{
		bool result = false;
		if (cardViews.Count <= viewIndex)
		{
			return result;
		}
		PagesMetaCardView pagesMetaCardView = cardViews[viewIndex];
		int num = viewIndex + cardPage * PageSize;
		if (num >= 0 && num < _cardDisplayInfos.Count)
		{
			PagesMetaCardViewDisplayInformation displayInfo = _cardDisplayInfos[num];
			result = pagesMetaCardView.UpdateDisplayInfo(displayInfo, 0, _forceCardsLocRefresh);
			pagesMetaCardView.gameObject.UpdateActive(active: true);
		}
		else
		{
			if (pagesMetaCardView.Holder.RolloverZoomView.IsActive)
			{
				pagesMetaCardView.Holder.RolloverZoomView.CardRolledOff(pagesMetaCardView.VisualCard);
			}
			pagesMetaCardView.gameObject.UpdateActive(active: false);
		}
		return result;
	}

	private void BeginScrolling()
	{
		_isScrolling = true;
		base.RolloverZoomView.Close();
		base.RolloverZoomView.IsActive = false;
		foreach (Page page in _pages)
		{
			foreach (PagesMetaCardView cardView in page.CardViews)
			{
				cardView.CardCollider.enabled = false;
			}
		}
	}

	private void EndScrolling()
	{
		_isScrolling = false;
		base.RolloverZoomView.IsActive = true;
		foreach (Page page in _pages)
		{
			foreach (PagesMetaCardView cardView in page.CardViews)
			{
				cardView.CardCollider.enabled = true;
			}
		}
	}

	protected void CardExpansionToggled(PagesMetaCardView cardView)
	{
		if (cardView.GetIsExpanded())
		{
			PreferredPrintingState.CollapseCard(cardView);
		}
		else
		{
			PreferredPrintingState.ExpandCard(cardView);
		}
	}

	protected void CardPreferredPrintingToggleClicked(PagesMetaCardView cardView, bool toggleValue)
	{
		PreferredPrintingState.OnCardPreferredPrintingToggleClicked(cardView, toggleValue);
	}

	protected void InvokePageChange(int currentPage)
	{
		this.OnPageChanged?.Invoke(currentPage);
	}

	private void CardDragged(MetaCardView _)
	{
		ActionsHandler.HideQuantityAdjust();
	}

	private void UpdateViewData(IReadOnlyList<PagesMetaCardViewDisplayInformation> displayInfos, bool scrollToTop, bool useNewTags)
	{
		SetCards(displayInfos, useNewTags);
		if (scrollToTop)
		{
			ScrollToTop();
		}
		bool flag = displayInfos.Count == 0;
		NoCardsText.gameObject.SetActive(flag);
		if (flag)
		{
			string key = "MainNav/Deckbuilder/NoFilteredCards/Default";
			if (DeckBuilderWidgetUtilities.HasCommanderSet(ContextProvider.Context, ModelProvider.Model) == DeckBuilderWidgetUtilities.CommanderType.CompleteCommander)
			{
				key = "MainNav/Deckbuilder/NoFilteredCards/Commander";
			}
			NoCardsText.text = Languages.ActiveLocProvider.GetLocalizedText(key);
		}
	}

	private int ExpectedPoolRows()
	{
		if (ContextProvider.Context == null)
		{
			return _lastRows;
		}
		if (ContextProvider.Context.IsEditingDeck && LayoutState.LayoutInUse == DeckBuilderLayout.Column)
		{
			return LayoutState.LargeCardsInPool ? _ColumnViewRowsLarge : _ColumnViewRowsSmall;
		}
		return LayoutState.LargeCardsInPool ? _VerticalViewRowsLarge : _VerticalViewRowsSmall;
	}

	public override DeckBuilderPile? GetParentDeckBuilderPile()
	{
		return DeckBuilderPile.Pool;
	}
}
