using System.Collections.Generic;
using AssetLookupTree.Blackboard;

namespace Wotc.Mtga.DuelScene.Browsers;

public class AssignDamageProvider : ICardBrowserProvider, IDuelSceneBrowserProvider
{
	public readonly string Header;

	public readonly string SubHeader;

	public readonly string OrderIndicatorText_First;

	public readonly string OrderIndicatorText_Last;

	private static readonly Dictionary<string, ButtonStateData> EmptyButtons = new Dictionary<string, ButtonStateData>();

	public bool ApplyTargetOffset => false;

	public bool ApplySourceOffset => false;

	public bool ApplyControllerOffset => false;

	public AssignDamageProvider(string header, string subheader, string orderIndicatorTextFirst, string orderIndicatorTextLast)
	{
		Header = header;
		SubHeader = subheader;
		OrderIndicatorText_First = orderIndicatorTextFirst;
		OrderIndicatorText_Last = orderIndicatorTextLast;
	}

	public DuelSceneBrowserType GetBrowserType()
	{
		return DuelSceneBrowserType.AssignDamage;
	}

	public Dictionary<string, ButtonStateData> GetButtonStateData()
	{
		return EmptyButtons;
	}

	public string GetCardHolderLayoutKey()
	{
		return "AssignDamage";
	}

	public BrowserCardHeader.BrowserCardHeaderData GetCardHeaderData(DuelScene_CDC cardView)
	{
		return null;
	}

	public void SetFxBlackboardDataForCard(DuelScene_CDC cardView, IBlackboard bb)
	{
	}

	public void SetFxBlackboardData(IBlackboard bb)
	{
	}

	public static Dictionary<string, ButtonStateData> CreateButtonMap(bool canSubmit)
	{
		return new Dictionary<string, ButtonStateData> { ["DoneButton"] = new ButtonStateData
		{
			LocalizedString = "DuelScene/KeywordSelection/KeywordSelection_Done",
			Enabled = canSubmit,
			BrowserElementKey = "SubmitButton",
			StyleType = ButtonStyle.StyleType.Main
		} };
	}
}
