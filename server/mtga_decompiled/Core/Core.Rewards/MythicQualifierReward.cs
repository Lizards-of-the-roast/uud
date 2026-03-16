using System;
using System.Collections;
using System.Collections.Generic;
using Core.Meta.MainNavigation.Store;
using UnityEngine;
using Wizards.Mtga.FrontDoorModels;

namespace Core.Rewards;

[Serializable]
public class MythicQualifierReward : AmountReward<RewardDisplay>
{
	protected override RewardType _rewardType => RewardType.MythicQualifier;

	public override void AddFromInventoryUpdate(ClientInventoryUpdateReportItem inventoryUpdate, PrecalculatedRewardUpdateInfo cache)
	{
	}

	public override IEnumerable<Func<RewardDisplayContext, IEnumerator>> DisplayRewards(ContentControllerRewards ccr)
	{
		int toAddCount = ToAddCount;
		if (toAddCount > 0)
		{
			yield return (RewardDisplayContext ctxt) => DisplayAmountRewardPile(ccr, ctxt.ChildIndex, WwiseEvents.sfx_ui_main_rewards_coins_flipout, toAddCount, new WaitForSeconds(1f));
		}
	}
}
