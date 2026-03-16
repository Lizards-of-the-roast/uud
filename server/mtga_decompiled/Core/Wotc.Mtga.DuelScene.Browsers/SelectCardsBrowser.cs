using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.DuelScene.Interactions.SelectN;

namespace Wotc.Mtga.DuelScene.Browsers;

public class SelectCardsBrowser : CardBrowserBase
{
	protected readonly ISelectCardsBrowserProvider selectCardsProvider;

	private readonly IHighlightController _highlightController;

	protected readonly IDimmingController _dimmingController;

	protected BrowserHeader browserHeader;

	protected Scrollbar scrollbar;

	protected StyledButton singleButton;

	protected StyledButton twoButton_Left;

	protected StyledButton twoButton_Right;

	public SelectCardsBrowser(BrowserManager browserManager, IDuelSceneBrowserProvider provider, IHighlightController highlightController, IDimmingController dimmingController, GameManager gameManager)
		: base(browserManager, provider, gameManager)
	{
		_highlightController = highlightController ?? NullHighlightController.Default;
		_dimmingController = dimmingController ?? NullDimmingController.Default;
		base.AllowsHoverInteractions = true;
		selectCardsProvider = provider as ISelectCardsBrowserProvider;
	}

	public override void Init()
	{
		base.Init();
		cardHolder.OnCardHolderUpdated += OnNextCardHolderUpdate;
	}

	protected void OnNextCardHolderUpdate()
	{
		UpdateHighlightsAndDimming();
		cardHolder.OnCardHolderUpdated -= OnNextCardHolderUpdate;
	}

	public override void Close()
	{
		base.Close();
		ClearHighlightsAndDimming();
		cardHolder.OnCardHolderUpdated -= OnNextCardHolderUpdate;
	}

	protected override void SetupCards()
	{
		cardViews = selectCardsProvider.GetCardsToDisplay();
		float scrollPosition = scrollableLayout.ScrollPosition;
		CardLayout_ScrollableBrowser cardHolderLayoutData = GetCardHolderLayoutData(cardBrowserProvider.GetCardHolderLayoutKey(), (uint)cardViews.Count);
		scrollableLayout.CopyFrom(cardHolderLayoutData);
		scrollableLayout.ScrollPosition = scrollPosition;
		MoveCardViewsToBrowser(cardViews);
	}

	public override void SetVisibility(bool visible)
	{
		base.SetVisibility(visible);
		if (visible)
		{
			UpdateHighlightsAndDimming();
		}
		else
		{
			ForceNoHighlightsWithDefaultDimming();
		}
	}

	protected override void InitializeUIElements()
	{
		browserHeader = GetBrowserElement("Header").GetComponent<BrowserHeader>();
		browserHeader.SetHeaderText(selectCardsProvider.GetHeaderText());
		browserHeader.SetSubheaderText(selectCardsProvider.GetSubHeaderText());
		GameObject browserElement = GetBrowserElement("SingleButton");
		if (browserElement != null)
		{
			singleButton = browserElement.GetComponent<StyledButton>();
		}
		browserElement = GetBrowserElement("2Button_Left");
		if (browserElement != null)
		{
			twoButton_Left = browserElement.GetComponent<StyledButton>();
		}
		browserElement = GetBrowserElement("2Button_Right");
		if (browserElement != null)
		{
			twoButton_Right = browserElement.GetComponent<StyledButton>();
		}
		scrollbar = GetBrowserElement("Scrollbar").GetComponent<Scrollbar>();
		scrollbar.onValueChanged.RemoveAllListeners();
		scrollbar.onValueChanged.AddListener(OnScroll);
		scrollbar.value = scrollableLayout.ScrollPosition;
		scrollbar.gameObject.SetActive(cardViews.Count > scrollableLayout.FrontCount);
		ScrollWheelInputForScrollbar component = GetBrowserElement("Scrollbar").GetComponent<ScrollWheelInputForScrollbar>();
		component.ElementCount = cardViews.Count;
		component.ElementsFeatured = scrollableLayout.FrontCount;
		base.InitializeUIElements();
	}

	public override void UpdateButtons()
	{
		UpdateButtons(selectCardsProvider.GetButtonStateData());
	}

	protected override void UpdateButtons(Dictionary<string, ButtonStateData> buttonStateData)
	{
		mainButton = BrowserBase.UpdateBasicThreeButtonStates(singleButton, twoButton_Left, twoButton_Right, OnButtonCallback, buttonStateData);
		if (buttonStateData != null && buttonStateData.Count == 1 && buttonStateData.TryGetValue("DoneButton", out var value) && value.StyleType == ButtonStyle.StyleType.Secondary)
		{
			GameObject browserElement = GetBrowserElement("ButtonMarker_BottomRight1");
			singleButton.transform.position = browserElement.transform.position;
		}
	}

	public void Refresh(float scrollPos)
	{
		ReleaseCards();
		SetupCards();
		RefreshCardVFX();
		browserHeader = GetBrowserElement("Header").GetComponent<BrowserHeader>();
		browserHeader.SetHeaderText(selectCardsProvider.GetHeaderText());
		browserHeader.SetSubheaderText(selectCardsProvider.GetSubHeaderText());
		UpdateButtons();
		scrollbar.value = scrollPos;
		scrollbar.gameObject.SetActive(cardViews.Count > scrollableLayout.FrontCount);
		cardHolder.OnCardHolderUpdated += OnNextCardHolderUpdate;
		cardHolder.LayoutNow();
	}

	private void RefreshCardVFX()
	{
		SetupCardFX();
		foreach (DuelScene_CDC cardView in cardViews)
		{
			if (_cardVFX.TryGetValue(cardView, out var value))
			{
				PlayCardVFX(cardView, value);
			}
		}
	}

	private void UpdateHighlights()
	{
		Dictionary<DuelScene_CDC, HighlightType> browserHighlights = selectCardsProvider.GetBrowserHighlights();
		_highlightController.SetForceHighlightsOff(forceHighlightsOff: false);
		_highlightController.SetBrowserHighlights(browserHighlights);
		RefreshCardVFX();
	}

	private void UpdateDimming()
	{
		Dictionary<DuelScene_CDC, bool> dictionary = new Dictionary<DuelScene_CDC, bool>();
		foreach (DuelScene_CDC selectableCdc in selectCardsProvider.GetSelectableCdcs())
		{
			dictionary[selectableCdc] = false;
		}
		foreach (DuelScene_CDC nonSelectableCdc in selectCardsProvider.GetNonSelectableCdcs())
		{
			dictionary[nonSelectableCdc] = true;
		}
		_dimmingController.SetBrowserDimming(dictionary, defaultState: true);
	}

	public void UpdateHighlightsAndDimming()
	{
		if (!base.IsClosed)
		{
			UpdateHighlights();
			UpdateDimming();
		}
	}

	private void ClearHighlightsAndDimming()
	{
		_highlightController.SetForceHighlightsOff(forceHighlightsOff: false);
		_highlightController.SetBrowserHighlights(null);
		_dimmingController.SetBrowserDimming(null, defaultState: false);
	}

	private void ForceNoHighlightsWithDefaultDimming()
	{
		_highlightController.SetForceHighlightsOff(forceHighlightsOff: true);
		_dimmingController.SetBrowserDimming(null, defaultState: false);
	}

	public override void OnCardViewSelected(DuelScene_CDC cardView)
	{
		base.OnCardViewSelected(cardView);
		PostCardViewSelection();
	}

	protected virtual void PostCardViewSelection()
	{
		if (!base.IsClosed)
		{
			UpdateHighlightsAndDimming();
		}
	}

	public void MakeKeyboardSelection()
	{
		if (!base.IsVisible)
		{
			return;
		}
		using IEnumerator<DuelScene_CDC> enumerator = selectCardsProvider.GetSelectableCdcs().GetEnumerator();
		if (enumerator.MoveNext())
		{
			DuelScene_CDC current = enumerator.Current;
			OnCardViewSelected(current);
		}
	}

	public bool AllowsKeyboardSelection()
	{
		return selectCardsProvider.AllowKeyboardSelection;
	}

	public override void OnKeyUp(KeyCode keyCode)
	{
		if (keyCode == KeyCode.Z)
		{
			(selectCardsProvider as SelectNWorkflow_Selection_Weighted_Browser)?.TryUndo();
		}
		base.OnKeyUp(keyCode);
	}
}
