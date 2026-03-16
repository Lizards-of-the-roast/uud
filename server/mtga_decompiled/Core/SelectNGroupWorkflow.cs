using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.Browsers;

public class SelectNGroupWorkflow : GroupedBrowserWorkflow<SelectNGroupRequest>
{
	private readonly ICardViewProvider _cardViewProvider;

	private readonly IBrowserController _browserController;

	public SelectNGroupWorkflow(SelectNGroupRequest request, ICardViewProvider cardViewProvider, IBrowserController browserController)
		: base(request)
	{
		_cardViewProvider = cardViewProvider;
		_browserController = browserController;
	}

	protected override void ApplyInteractionInternal()
	{
		_cardsToDisplay = new List<List<DuelScene_CDC>>();
		InitializeButtonStateData();
		for (int i = 0; i < _request.Groups.Count; i++)
		{
			_cardsToDisplay.Add(_cardViewProvider.GetCardViews(_request.Groups[i].Ids));
		}
		SetOpenedBrowser(_browserController.OpenBrowser(this));
	}

	private void InitializeButtonStateData()
	{
		_buttonStateData = new Dictionary<string, ButtonStateData>();
		ButtonStateData buttonStateData = new ButtonStateData();
		buttonStateData.Enabled = true;
		buttonStateData.LocalizedString = "DuelScene/Browsers/Select_Title";
		buttonStateData.BrowserElementKey = "GroupAButton";
		buttonStateData.StyleType = ButtonStyle.StyleType.Secondary;
		_buttonStateData.Add("GroupAButton", buttonStateData);
		ButtonStateData buttonStateData2 = new ButtonStateData();
		buttonStateData2.Enabled = true;
		buttonStateData2.LocalizedString = "DuelScene/Browsers/Select_Title";
		buttonStateData2.BrowserElementKey = "GroupBButton";
		buttonStateData2.StyleType = ButtonStyle.StyleType.Secondary;
		_buttonStateData.Add("GroupBButton", buttonStateData2);
	}

	public void OnGroupSelected(int groupIndex)
	{
		_request.SubmitGroupSelection((uint)_request.Groups[groupIndex].GroupId);
	}

	public override DuelSceneBrowserType GetBrowserType()
	{
		return DuelSceneBrowserType.SelectGroup;
	}

	public override void Browser_OnButtonPressed(string buttonKey)
	{
		OnGroupSelected((!(buttonKey == "GroupAButton")) ? 1 : 0);
	}

	public override string GetCardHolderLayoutKey()
	{
		return "SelectGroup";
	}
}
