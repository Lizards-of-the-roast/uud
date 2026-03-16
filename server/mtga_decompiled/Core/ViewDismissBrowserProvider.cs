using System;
using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using Wotc.Mtga.DuelScene.Browsers;

public class ViewDismissBrowserProvider : IViewDismissBrowserProvider, ICardBrowserProvider, IDuelSceneBrowserProvider, ICardLock
{
	private readonly List<DuelScene_CDC> _cardsToDisplay;

	private readonly Dictionary<string, ButtonStateData> _buttonStateData;

	private List<uint> _selectableCards;

	public ViewDismissBrowser OpenedBrowser { get; private set; }

	public string Header { get; }

	public string SubHeader { get; private set; } = string.Empty;

	public bool ApplyTargetOffset { get; }

	public bool ApplySourceOffset { get; }

	public bool ApplyControllerOffset { get; }

	public bool LockCardDetails { get; set; }

	public Action<DuelScene_CDC> OnCardSelected { get; private set; }

	public GREPlayerNum PlayerNum { get; private set; }

	public event Action DismissedHandlers;

	public event Action PreDismissHandlers;

	public ViewDismissBrowserProvider(List<DuelScene_CDC> cardViews, Action dismissedHandler, string header = "", Action<DuelScene_CDC> onCardSelected = null, Action preDismissHandler = null, GREPlayerNum playerNum = GREPlayerNum.Invalid, List<uint> cardIds = null)
	{
		Header = header;
		_cardsToDisplay = cardViews;
		_selectableCards = ((cardIds == null) ? new List<uint>() : cardIds);
		SortCardsToDisplay();
		OnCardSelected = onCardSelected;
		DismissedHandlers += dismissedHandler;
		PreDismissHandlers += preDismissHandler;
		_buttonStateData = new Dictionary<string, ButtonStateData>();
		ButtonStateData value = new ButtonStateData
		{
			BrowserElementKey = "DismissButton",
			LocalizedString = "DuelScene/Browsers/ViewDismiss_Done",
			Enabled = true,
			StyleType = ButtonStyle.StyleType.Main
		};
		_buttonStateData.Add("DismissButton", value);
		ApplyTargetOffset = true;
		ApplySourceOffset = false;
		ApplyControllerOffset = false;
		PlayerNum = playerNum;
	}

	public ViewDismissBrowserProvider(List<DuelScene_CDC> cardViews, Action dismissedHandler, string header, string subHeader, Action<DuelScene_CDC> onCardSelected = null, Action preDismissHandler = null)
		: this(cardViews, dismissedHandler, header, onCardSelected, preDismissHandler)
	{
		SubHeader = (string.IsNullOrEmpty(subHeader) ? string.Empty : subHeader);
	}

	~ViewDismissBrowserProvider()
	{
		OnCardSelected = null;
		this.DismissedHandlers = null;
		this.PreDismissHandlers = null;
	}

	public string GetCardHolderLayoutKey()
	{
		return "Default";
	}

	public DuelSceneBrowserType GetBrowserType()
	{
		return DuelSceneBrowserType.ViewDismiss;
	}

	public List<DuelScene_CDC> GetCardsToDisplay()
	{
		return _cardsToDisplay;
	}

	private void SortCardsToDisplay()
	{
		if (_selectableCards.Count > 0)
		{
			_cardsToDisplay.Sort(delegate(DuelScene_CDC x, DuelScene_CDC y)
			{
				bool value = _selectableCards.Contains(x.Model.InstanceId);
				return _selectableCards.Contains(y.Model.InstanceId).CompareTo(value);
			});
		}
	}

	public BrowserCardHeader.BrowserCardHeaderData GetCardHeaderData(DuelScene_CDC cardView)
	{
		return null;
	}

	private void OnButtonPressed(string buttonKey)
	{
		if (buttonKey == "DismissButton")
		{
			OpenedBrowser.Close();
		}
	}

	public void SetOpenedBrowser(IBrowser openedBrowser)
	{
		OpenedBrowser = openedBrowser as ViewDismissBrowser;
		openedBrowser.ClosedHandlers += OnBrowserClosed;
		openedBrowser.ButtonPressedHandlers += OnButtonPressed;
		if (openedBrowser is ICardBrowser cardBrowser)
		{
			cardBrowser.CardViewSelectedHandlers += OnCardSelected;
			cardBrowser.PreReleaseCardViewsHandlers += OnPreReleaseCardViews;
		}
	}

	public Dictionary<string, ButtonStateData> GetButtonStateData()
	{
		return _buttonStateData;
	}

	private void OnBrowserClosed()
	{
		this.DismissedHandlers?.Invoke();
		OpenedBrowser.ClosedHandlers -= OnBrowserClosed;
		OpenedBrowser.ButtonPressedHandlers -= OnButtonPressed;
		ICardBrowser openedBrowser = OpenedBrowser;
		if (openedBrowser != null)
		{
			openedBrowser.CardViewSelectedHandlers -= OnCardSelected;
			openedBrowser.PreReleaseCardViewsHandlers -= OnPreReleaseCardViews;
		}
	}

	private void OnPreReleaseCardViews()
	{
		this.PreDismissHandlers?.Invoke();
	}

	public void SetFxBlackboardData(IBlackboard bb)
	{
	}

	public void SetFxBlackboardDataForCard(DuelScene_CDC cardView, IBlackboard bb)
	{
	}
}
