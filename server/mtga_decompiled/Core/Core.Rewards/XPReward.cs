using System;
using System.Collections;
using System.Collections.Generic;
using Core.MainNavigation.RewardTrack;
using Core.Meta.MainNavigation.Store;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;

namespace Core.Rewards;

[Serializable]
public class XPReward : AmountReward<RewardDisplay>
{
	protected override RewardType _rewardType => RewardType.Xp;

	private SetMasteryDataProvider MasteryPassProvider => Pantry.Get<SetMasteryDataProvider>();

	public void AddIfMasteryActive(SetMasteryDataProvider masteryPassProvider, int amount)
	{
		if (!masteryPassProvider.HasTrackExpired(masteryPassProvider.CurrentBpName))
		{
			ToAddCount += amount;
		}
	}

	public override void AddFromInventoryUpdate(ClientInventoryUpdateReportItem inventoryUpdate, PrecalculatedRewardUpdateInfo cache)
	{
		AddIfMasteryActive(MasteryPassProvider, inventoryUpdate.xpGained);
	}

	public override IEnumerable<Func<RewardDisplayContext, IEnumerator>> DisplayRewards(ContentControllerRewards ccr)
	{
		int toAddCount = ToAddCount;
		if (toAddCount > 0)
		{
			yield return (RewardDisplayContext ctxt) => DisplayAmountRewardPile(ccr, ctxt.ChildIndex, WwiseEvents.sfx_ui_reward_rollover_reveal_generic, toAddCount);
		}
	}
}
