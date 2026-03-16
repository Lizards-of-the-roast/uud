using System.Collections.Generic;
using AssetLookupTree.Blackboard;

namespace Wotc.Mtga.DuelScene.Browsers;

public class OpeningHandBrowserProvider : ICardBrowserProvider, IDuelSceneBrowserProvider
{
	public bool ApplyTargetOffset => false;

	public bool ApplySourceOffset => false;

	public bool ApplyControllerOffset => false;

	public DuelSceneBrowserType GetBrowserType()
	{
		return DuelSceneBrowserType.OpeningHand;
	}

	public string GetCardHolderLayoutKey()
	{
		return "Mulligan";
	}

	public BrowserCardHeader.BrowserCardHeaderData GetCardHeaderData(DuelScene_CDC cardView)
	{
		return null;
	}

	public Dictionary<string, ButtonStateData> GetButtonStateData()
	{
		return null;
	}

	public void SetFxBlackboardData(IBlackboard bb)
	{
	}

	public void SetFxBlackboardDataForCard(DuelScene_CDC cardView, IBlackboard bb)
	{
	}
}
