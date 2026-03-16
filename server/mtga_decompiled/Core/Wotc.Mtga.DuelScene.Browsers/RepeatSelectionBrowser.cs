using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Wotc.Mtga.DuelScene.Browsers;

public class RepeatSelectionBrowser : CardBrowserBase, IGroupedCardProvider
{
	private readonly CastingTimeOption_ModalRepeatWorkflow _modalRepeatWorkflow;

	private readonly IHighlightController _highlightController;

	private readonly IDimmingController _dimmingController;

	private CardLayout_MultiLayout _multiLayout;

	private List<DuelScene_CDC> _options;

	private List<DuelScene_CDC> _selections;

	private BrowserHeader _browserHeader;

	private Scrollbar _scrollbar;

	private StyledButton _singleButton;

	private StyledButton _twoButton_Left;

	private StyledButton _twoButton_Right;

	private List<List<DuelScene_CDC>> _emptyCardGroups = new List<List<DuelScene_CDC>>(2)
	{
		new List<DuelScene_CDC>(0),
		new List<DuelScene_CDC>(0)
	};

	private List<List<DuelScene_CDC>> _cardGroups;

	private Vector3 _choicesCenterPoint = new Vector3(0f, 0f, 0f);

	private Vector3 _selectionsCenterPoint = new Vector3(-7.8f, -4.75f, 0f);

	private List<BrowserCardCounter> _cardCounters = new List<BrowserCardCounter>();

	public RepeatSelectionBrowser(BrowserManager browserManager, IDuelSceneBrowserProvider provider, IHighlightController highlightController, IDimmingController dimmingController, GameManager gameManager)
		: base(browserManager, provider, gameManager)
	{
		_modalRepeatWorkflow = provider as CastingTimeOption_ModalRepeatWorkflow;
		_highlightController = highlightController ?? NullHighlightController.Default;
		_dimmingController = dimmingController ?? NullDimmingController.Default;
		base.AllowsHoverInteractions = true;
	}

	public override void Init()
	{
		_cardGroups = new List<List<DuelScene_CDC>>();
		_options = _modalRepeatWorkflow.GetOptionViews();
		_cardGroups.Add(_options);
		_selections = _modalRepeatWorkflow.GetSelectionsViews();
		_cardGroups.Add(_selections);
		base.Init();
		cardHolder.OnCardHolderUpdated += OnFirstCardHolderUpdate;
	}

	private void OnFirstCardHolderUpdate()
	{
		UpdateHighlightsAndDimming();
		cardHolder.OnCardHolderUpdated -= OnFirstCardHolderUpdate;
	}

	protected override ICardLayout GetCardHolderLayout()
	{
		return _multiLayout;
	}

	protected override void InitCardHolder()
	{
		base.InitCardHolder();
		cardHolder.CardGroupProvider = this;
	}

	protected override void SetupCards()
	{
		cardViews.AddRange(_options);
		cardViews.AddRange(_selections);
		MoveCardViewsToBrowser(cardViews);
	}

	public void AddSelection(DuelScene_CDC selection)
	{
		_cardMovementController.MoveCard(selection, cardHolder);
	}

	public void RemoveSelection(DuelScene_CDC selection)
	{
		cardHolder.RemoveCard(selection);
	}

	public List<List<DuelScene_CDC>> GetCardGroups()
	{
		if (base.IsVisible)
		{
			return _cardGroups;
		}
		return _emptyCardGroups;
	}

	protected override void InitializeUIElements()
	{
		_choicesCenterPoint = GetBrowserElement("CardGroupAMarker").transform.localPosition;
		_selectionsCenterPoint = GetBrowserElement("CardGroupBMarker").transform.localPosition;
		CreateLayout();
		_browserHeader = GetBrowserElement("Header").GetComponent<BrowserHeader>();
		_browserHeader.SetHeaderText(_modalRepeatWorkflow.GetHeaderText());
		_browserHeader.SetSubheaderText(_modalRepeatWorkflow.GetSubHeaderText());
		GameObject browserElement = GetBrowserElement("SingleButton");
		if (browserElement != null)
		{
			_singleButton = browserElement.GetComponent<StyledButton>();
		}
		browserElement = GetBrowserElement("2Button_Left");
		if (browserElement != null)
		{
			_twoButton_Left = browserElement.GetComponent<StyledButton>();
		}
		browserElement = GetBrowserElement("2Button_Right");
		if (browserElement != null)
		{
			_twoButton_Right = browserElement.GetComponent<StyledButton>();
		}
		_scrollbar = GetBrowserElement("Scrollbar").GetComponent<Scrollbar>();
		_scrollbar.onValueChanged.RemoveAllListeners();
		_scrollbar.onValueChanged.AddListener(OnScroll);
		if (_multiLayout.GetLayout(0) is CardLayout_ScrollableBrowser cardLayout_ScrollableBrowser)
		{
			_scrollbar.value = cardLayout_ScrollableBrowser.ScrollPosition;
			_scrollbar.gameObject.SetActive(_options.Count > cardLayout_ScrollableBrowser.FrontCount);
			ScrollWheelInputForScrollbar component = GetBrowserElement("Scrollbar").GetComponent<ScrollWheelInputForScrollbar>();
			component.ElementCount = _options.Count;
			component.ElementsFeatured = cardLayout_ScrollableBrowser.FrontCount;
		}
		InitializeCardCounters();
		base.InitializeUIElements();
	}

	protected override void OnScroll(float scrollValue)
	{
		if (_multiLayout.GetLayout(0) is CardLayout_ScrollableBrowser cardLayout_ScrollableBrowser)
		{
			cardLayout_ScrollableBrowser.ScrollPosition = scrollValue;
			cardHolder.LayoutNow();
		}
	}

	public override void UpdateButtons()
	{
		UpdateButtons(_modalRepeatWorkflow.GetButtonStateData());
	}

	protected override void UpdateButtons(Dictionary<string, ButtonStateData> buttonStateData)
	{
		mainButton = BrowserBase.UpdateBasicThreeButtonStates(_singleButton, _twoButton_Left, _twoButton_Right, OnButtonCallback, buttonStateData);
	}

	public override void SetVisibility(bool visible)
	{
		base.SetVisibility(visible);
		if (visible)
		{
			UpdateHighlightsAndDimming();
			RefreshCounters();
			RefreshSelections();
		}
		else
		{
			ForceNoHighlightsWithDefaultDimming();
		}
	}

	protected virtual void UpdateHighlights()
	{
		Dictionary<DuelScene_CDC, HighlightType> cardHighlights = _modalRepeatWorkflow.GetCardHighlights();
		_highlightController.SetForceHighlightsOff(forceHighlightsOff: false);
		_highlightController.SetBrowserHighlights(cardHighlights);
	}

	protected virtual void UpdateDimming()
	{
		_dimmingController.SetBrowserDimming(_modalRepeatWorkflow.GetCardDimming(), defaultState: true);
	}

	private void UpdateHighlightsAndDimming()
	{
		if (!base.IsClosed)
		{
			UpdateHighlights();
			UpdateDimming();
		}
	}

	protected virtual void ClearHighlightsAndDimming()
	{
		_highlightController.SetForceHighlightsOff(forceHighlightsOff: false);
		_highlightController.SetBrowserHighlights(null);
		_dimmingController.SetBrowserDimming(null, defaultState: false);
	}

	protected virtual void ForceNoHighlightsWithDefaultDimming()
	{
		_highlightController.SetForceHighlightsOff(forceHighlightsOff: true);
		_dimmingController.SetBrowserDimming(null, defaultState: false);
	}

	public override void Close()
	{
		base.Close();
		ClearHighlightsAndDimming();
		cardHolder.OnCardHolderUpdated -= OnFirstCardHolderUpdate;
		cardHolder.CardGroupProvider = null;
	}

	public void Refresh()
	{
		_browserHeader.SetSubheaderText(_modalRepeatWorkflow.GetSubHeaderText());
		UpdateButtons();
		UpdateHighlightsAndDimming();
		RefreshCounters();
		RefreshSelections();
	}

	private void CreateLayout()
	{
		List<ICardLayout> list = new List<ICardLayout>();
		list.Add(new CardLayout_ScrollableBrowser(GetCardHolderLayoutData(_modalRepeatWorkflow.GetCardHolderLayoutKey())));
		list.Add(new CardLayout_ScrollableBrowser(GetCardHolderLayoutData("Repeat_Selections_Small")));
		List<Vector3> layoutCenters = new List<Vector3> { _choicesCenterPoint, _selectionsCenterPoint };
		List<Quaternion> layoutRotations = new List<Quaternion>
		{
			Quaternion.identity,
			Quaternion.identity
		};
		_multiLayout = new CardLayout_MultiLayout(list, layoutCenters, layoutRotations, this);
	}

	private void InitializeCardCounters()
	{
		_cardCounters.Clear();
		InitializeCardCounter("CardCounterA", GetCardCounterSpaceMarkers("CardSpacingAMarker"));
		InitializeCardCounter("CardCounterB", GetCardCounterSpaceMarkers("CardSpacingBMarker"));
		InitializeCardCounter("CardCounterC", GetCardCounterSpaceMarkers("CardSpacingCMarker"));
		if (_modalRepeatWorkflow.ModalOptionTotal() == 3)
		{
			HideCardCounter("CardCounterD");
		}
		else
		{
			InitializeCardCounter("CardCounterD");
		}
	}

	private Transform GetCardCounterSpaceMarkers(string id)
	{
		GameObject browserElement = GetBrowserElement(id);
		if (browserElement != null)
		{
			return browserElement.transform;
		}
		return null;
	}

	private void HideCardCounter(string id)
	{
		GameObject browserElement = GetBrowserElement(id);
		if (browserElement != null)
		{
			browserElement.SetActive(value: false);
		}
	}

	private void InitializeCardCounter(string id, Transform pos = null)
	{
		GameObject browserElement = GetBrowserElement(id);
		if (!(browserElement != null))
		{
			return;
		}
		BrowserCardCounter counter = browserElement.GetComponent<BrowserCardCounter>();
		if (counter != null)
		{
			if (pos != null && _modalRepeatWorkflow.ModalOptionTotal() == 3)
			{
				counter.transform.position = pos.position;
			}
			_cardCounters.Add(counter);
			counter.IncrementButton.onClick.RemoveAllListeners();
			counter.DecrementButton.onClick.RemoveAllListeners();
			counter.IncrementButton.onClick.AddListener(delegate
			{
				OnIncrement(counter);
			});
			counter.DecrementButton.onClick.AddListener(delegate
			{
				OnDecrement(counter);
			});
		}
	}

	private void OnIncrement(BrowserCardCounter counter)
	{
		DuelScene_CDC closestCDCOption = GetClosestCDCOption(counter);
		if ((bool)closestCDCOption)
		{
			OnCardViewSelected(closestCDCOption);
		}
	}

	private void OnDecrement(BrowserCardCounter counter)
	{
		DuelScene_CDC closestCDCOption = GetClosestCDCOption(counter);
		if (!closestCDCOption)
		{
			return;
		}
		foreach (DuelScene_CDC selectionsView in _modalRepeatWorkflow.GetSelectionsViews())
		{
			if (selectionsView.VisualModel.GrpId == closestCDCOption.VisualModel.GrpId)
			{
				OnCardViewSelected(selectionsView);
				break;
			}
		}
	}

	private DuelScene_CDC GetClosestCDCOption(BrowserCardCounter counter)
	{
		float num = 0f;
		DuelScene_CDC duelScene_CDC = null;
		Vector2 screenPosition = counter.GetScreenPosition();
		foreach (DuelScene_CDC optionView in _modalRepeatWorkflow.GetOptionViews())
		{
			float num2 = Vector2.Distance(screenPosition, GetScreenPosition(optionView));
			if (duelScene_CDC == null || num2 < num)
			{
				num = num2;
				duelScene_CDC = optionView;
			}
		}
		return duelScene_CDC;
	}

	private void RefreshSelections()
	{
		if (!IsUsingCounters())
		{
			return;
		}
		foreach (DuelScene_CDC selectionsView in _modalRepeatWorkflow.GetSelectionsViews())
		{
			selectionsView.gameObject.SetActive(value: false);
		}
	}

	private void RefreshCounters()
	{
		if (!IsUsingCounters())
		{
			return;
		}
		foreach (BrowserCardCounter cardCounter in _cardCounters)
		{
			cardCounter.SetCardCount(0);
		}
		foreach (DuelScene_CDC selectionsView in _modalRepeatWorkflow.GetSelectionsViews())
		{
			foreach (DuelScene_CDC optionView in _modalRepeatWorkflow.GetOptionViews())
			{
				if (optionView.VisualModel.GrpId == selectionsView.VisualModel.GrpId)
				{
					GetClosestCounter(optionView)?.IncrementCardCount();
					break;
				}
			}
		}
	}

	private BrowserCardCounter GetClosestCounter(DuelScene_CDC cdc)
	{
		float num = 0f;
		BrowserCardCounter browserCardCounter = null;
		Vector2 screenPosition = GetScreenPosition(cdc);
		foreach (BrowserCardCounter cardCounter in _cardCounters)
		{
			float num2 = Vector2.Distance(screenPosition, cardCounter.GetScreenPosition());
			if (browserCardCounter == null || num2 < num)
			{
				num = num2;
				browserCardCounter = cardCounter;
			}
		}
		return browserCardCounter;
	}

	public bool IsUsingCounters()
	{
		return _cardCounters.Count > 0;
	}

	private Vector2 GetScreenPosition(DuelScene_CDC card)
	{
		return CurrentCamera.Value.WorldToScreenPoint(card.transform.position);
	}
}
