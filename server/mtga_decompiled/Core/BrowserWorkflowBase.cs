using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtga.CardParts;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtga.DuelScene.Interactions;

public abstract class BrowserWorkflowBase<T> : WorkflowBase<T>, IBasicBrowserProvider, ICardBrowserProvider, IDuelSceneBrowserProvider, IBrowserHeaderProvider where T : BaseUserRequest
{
	protected List<DuelScene_CDC> _cardsToDisplay = new List<DuelScene_CDC>();

	protected Dictionary<string, ButtonStateData> _buttonStateData;

	protected string _header = string.Empty;

	protected string _subHeader = string.Empty;

	protected bool _closeBrowserOnCleanup = true;

	protected BrowserBase _openedBrowser;

	public bool ApplyTargetOffset { get; protected set; }

	public bool ApplySourceOffset { get; protected set; }

	public bool ApplyControllerOffset { get; protected set; }

	public List<DuelScene_CDC> GetCardsToDisplay()
	{
		return _cardsToDisplay;
	}

	public virtual string GetCardHolderLayoutKey()
	{
		return "Default";
	}

	public virtual string GetHeaderText()
	{
		return _header;
	}

	public virtual string GetSubHeaderText()
	{
		return _subHeader;
	}

	public Dictionary<string, ButtonStateData> GetButtonStateData()
	{
		return _buttonStateData;
	}

	public abstract DuelSceneBrowserType GetBrowserType();

	protected BrowserWorkflowBase(T request)
		: base(request)
	{
	}

	protected override void SetPrompt()
	{
	}

	public virtual BrowserCardHeader.BrowserCardHeaderData GetCardHeaderData(DuelScene_CDC cardView)
	{
		return null;
	}

	public override void CleanUp()
	{
		if (_openedBrowser != null && _closeBrowserOnCleanup)
		{
			_openedBrowser.ClosedHandlers -= Browser_OnBrowserClosed;
			_openedBrowser.ShownHandlers -= Browser_OnBrowserShown;
			_openedBrowser.HiddenHandlers -= Browser_OnBrowserHidden;
			_openedBrowser.ButtonPressedHandlers -= Browser_OnBrowserButtonPressed;
			if (_openedBrowser is ICardBrowser cardBrowser)
			{
				cardBrowser.CardViewSelectedHandlers -= CardBrowser_OnCardViewSelected;
				cardBrowser.CardViewRemovedHandlers -= CardBrowser_OnCardViewRemoved;
				cardBrowser.ReleaseCardViewHandlers -= CardBrowser_OnReleaseCardView;
				cardBrowser.PreReleaseCardViewsHandlers -= CardBrowser_OnPreReleaseCardViews;
			}
			_openedBrowser.Close();
		}
		base.CleanUp();
	}

	public void SetOpenedBrowser(IBrowser browser)
	{
		_openedBrowser = (BrowserBase)browser;
		browser.ClosedHandlers += Browser_OnBrowserClosed;
		browser.ShownHandlers += Browser_OnBrowserShown;
		browser.HiddenHandlers += Browser_OnBrowserHidden;
		browser.ButtonPressedHandlers += Browser_OnBrowserButtonPressed;
		if (browser is ICardBrowser cardBrowser)
		{
			cardBrowser.CardViewSelectedHandlers += CardBrowser_OnCardViewSelected;
			cardBrowser.CardViewRemovedHandlers += CardBrowser_OnCardViewRemoved;
			cardBrowser.ReleaseCardViewHandlers += CardBrowser_OnReleaseCardView;
			cardBrowser.PreReleaseCardViewsHandlers += CardBrowser_OnPreReleaseCardViews;
		}
		OnBrowserOpened();
	}

	protected virtual void OnBrowserOpened()
	{
	}

	protected virtual void CardBrowser_OnPreReleaseCardViews()
	{
	}

	protected virtual void CardBrowser_OnReleaseCardView(DuelScene_CDC cardView)
	{
	}

	protected virtual void CardBrowser_OnCardViewRemoved(DuelScene_CDC cardView)
	{
	}

	protected virtual void CardBrowser_OnCardViewSelected(DuelScene_CDC cardView)
	{
	}

	protected virtual void Browser_OnBrowserButtonPressed(string buttonKey)
	{
	}

	protected virtual void Browser_OnBrowserHidden()
	{
	}

	protected virtual void Browser_OnBrowserShown()
	{
	}

	protected virtual void Browser_OnBrowserClosed()
	{
	}

	public virtual void SetFxBlackboardData(IBlackboard bb)
	{
	}

	public virtual void SetFxBlackboardDataForCard(DuelScene_CDC cardView, IBlackboard bb)
	{
		if ((bool)cardView && cardView.Model != null)
		{
			bb.SetCardDataExtensive(cardView.Model);
			bb.SetCdcViewMetadata(new CDCViewMetadata(cardView));
		}
	}
}
