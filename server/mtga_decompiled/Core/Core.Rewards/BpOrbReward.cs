using System;
using System.Collections;
using System.Collections.Generic;
using Core.MainNavigation.RewardTrack;
using Core.Meta.MainNavigation.Store;
using Wizards.Mtga.FrontDoorModels;

namespace Core.Rewards;

[Serializable]
public class BpOrbReward : ItemReward<OrbInventoryChange, RewardDisplay>
{
	protected override RewardType _rewardType => RewardType.BpOrb;

	public override void AddFromInventoryUpdate(ClientInventoryUpdateReportItem inventoryUpdate, PrecalculatedRewardUpdateInfo cache)
	{
	}

	public override IEnumerable<Func<RewardDisplayContext, IEnumerator>> DisplayRewards(ContentControllerRewards ccr)
	{
		int orbTally = 0;
		foreach (OrbInventoryChange item in ToAdd)
		{
			orbTally += item.currentGenericOrbAmount - item.oldGenericOrbAmount;
		}
		if (orbTally > 0)
		{
			yield return (RewardDisplayContext ctxt) => ShowGenericOrbs(ccr, orbTally, ctxt.ChildIndex);
		}
	}

	private IEnumerator ShowGenericOrbs(ContentControllerRewards ccr, int genericOrbTally, int childIndex)
	{
		if (base.Instances.Count == 0)
		{
			Instantiate(ccr, childIndex);
		}
		RewardDisplay rewardDisplay = base.Instances[0];
		rewardDisplay.SetCountText(genericOrbTally);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_masterytree_orb_acquire, rewardDisplay.gameObject);
		yield return null;
	}
}
