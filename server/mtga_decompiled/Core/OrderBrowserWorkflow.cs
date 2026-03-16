using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

public abstract class OrderBrowserWorkflow<T> : BrowserWorkflowBase<T>, IOrderBrowserProvider, IBasicBrowserProvider, ICardBrowserProvider, IDuelSceneBrowserProvider, IBrowserHeaderProvider where T : BaseUserRequest
{
	protected string leftOrderIndicatorText;

	protected string rightOrderIndicatorText;

	protected OrderIndicator.ArrowDirection orderIndicatorArrowDirection;

	public override DuelSceneBrowserType GetBrowserType()
	{
		return DuelSceneBrowserType.Order;
	}

	public override string GetCardHolderLayoutKey()
	{
		return "Order";
	}

	protected OrderBrowserWorkflow(T request)
		: base(request)
	{
	}

	protected void SetupButton()
	{
		_buttonStateData = new Dictionary<string, ButtonStateData>();
		ButtonStateData buttonStateData = new ButtonStateData();
		buttonStateData.LocalizedString = "DuelScene/KeywordSelection/KeywordSelection_Done";
		buttonStateData.Enabled = true;
		buttonStateData.BrowserElementKey = "SubmitButton";
		buttonStateData.StyleType = ButtonStyle.StyleType.Main;
		_buttonStateData.Add("DoneButton", buttonStateData);
	}

	public string GetLeftOrderText()
	{
		return leftOrderIndicatorText;
	}

	public string GetRightOrderText()
	{
		return rightOrderIndicatorText;
	}

	public OrderIndicator.ArrowDirection GetArrowDirection()
	{
		return orderIndicatorArrowDirection;
	}

	protected override void Browser_OnBrowserButtonPressed(string buttonKey)
	{
		if (buttonKey == "DoneButton")
		{
			Submit();
		}
	}

	protected abstract void Submit();

	public abstract OrderingContext GetOrderingContext();
}
