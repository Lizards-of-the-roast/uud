using System;
using System.Collections;
using System.Collections.Generic;
using Core.Meta.MainNavigation.Store;
using Wizards.Mtga.FrontDoorModels;

namespace Core.Rewards;

[Serializable]
public class GoldReward : AmountReward<RewardDisplay>
{
	protected override RewardType _rewardType => RewardType.Gold;

	public override void AddFromInventoryUpdate(ClientInventoryUpdateReportItem inventoryUpdate, PrecalculatedRewardUpdateInfo cache)
	{
		AddIfPositive(cache.GoldLessCards);
	}

	public override IEnumerable<Func<RewardDisplayContext, IEnumerator>> DisplayRewards(ContentControllerRewards ccr)
	{
		int toAddCount = ToAddCount;
		if (toAddCount > 0)
		{
			yield return (RewardDisplayContext ctxt) => DisplayAmountRewardPile(ccr, ctxt.ChildIndex, WwiseEvents.sfx_ui_main_rewards_coins_flipout, toAddCount);
		}
	}
}
