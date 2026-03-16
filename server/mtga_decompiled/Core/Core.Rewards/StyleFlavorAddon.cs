using System;
using System.Collections;
using System.Collections.Generic;
using Core.Meta.MainNavigation.Store;
using Wizards.Mtga.FrontDoorModels;

namespace Core.Rewards;

[Serializable]
public class StyleFlavorAddon : ItemReward<string, RewardDisplayFlavorText>
{
	protected override RewardType _rewardType => RewardType.StyleFlavor;

	public override void AddFromInventoryUpdate(ClientInventoryUpdateReportItem inventoryUpdate, PrecalculatedRewardUpdateInfo cache)
	{
	}

	public override IEnumerable<Func<RewardDisplayContext, IEnumerator>> DisplayRewards(ContentControllerRewards ccr)
	{
		yield break;
	}
}
