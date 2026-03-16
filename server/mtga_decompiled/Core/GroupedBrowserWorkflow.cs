using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtga.DuelScene.Interactions;

public abstract class GroupedBrowserWorkflow<T> : WorkflowBase<T>, IGroupBrowserProvider, ICardBrowserProvider, IDuelSceneBrowserProvider where T : BaseUserRequest
{
	protected List<List<DuelScene_CDC>> _cardsToDisplay;

	protected Dictionary<string, ButtonStateData> _buttonStateData;

	protected CardBrowserBase _openedBrowser;

	public bool ApplyTargetOffset => false;

	public bool ApplySourceOffset => false;

	public bool ApplyControllerOffset => false;

	public GroupedBrowserWorkflow(T request)
		: base(request)
	{
	}

	public List<List<DuelScene_CDC>> GetCardsToDisplay()
	{
		return _cardsToDisplay;
	}

	public Dictionary<string, ButtonStateData> GetButtonStateData()
	{
		return _buttonStateData;
	}

	public BrowserCardHeader.BrowserCardHeaderData GetCardHeaderData(DuelScene_CDC cardView)
	{
		return null;
	}

	public abstract DuelSceneBrowserType GetBrowserType();

	public abstract string GetCardHolderLayoutKey();

	public abstract void Browser_OnButtonPressed(string buttonKey);

	protected virtual void Browser_OnClosed()
	{
		_openedBrowser.ButtonPressedHandlers -= Browser_OnButtonPressed;
		_openedBrowser.ClosedHandlers -= Browser_OnClosed;
		_openedBrowser = null;
	}

	public override void CleanUp()
	{
		if (_openedBrowser != null)
		{
			_openedBrowser.Close();
		}
		base.CleanUp();
	}

	public virtual void SetOpenedBrowser(IBrowser browser)
	{
		_openedBrowser = (CardBrowserBase)browser;
		_openedBrowser.ButtonPressedHandlers += Browser_OnButtonPressed;
		_openedBrowser.ClosedHandlers += Browser_OnClosed;
	}

	public void SetFxBlackboardData(IBlackboard bb)
	{
	}

	public void SetFxBlackboardDataForCard(DuelScene_CDC cardView, IBlackboard bb)
	{
	}
}
