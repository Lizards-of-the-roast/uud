using System;
using System.Collections;
using Core.Meta.MainNavigation.Store;
using UnityEngine;

namespace Core.Rewards;

[Serializable]
public abstract class AmountReward<P> : RewardBase<P> where P : RewardDisplay
{
	public void AddIfPositive(int amount)
	{
		if (amount > 0)
		{
			ToAddCount += amount;
		}
	}

	public override void ClearAdded()
	{
		ToAddCount = 0;
	}

	protected IEnumerator DisplayAmountRewardPile(ContentControllerRewards ccr, int childIndex, WwiseEvents sfx, int amt, YieldInstruction andThen = null)
	{
		if (base.Instances.Count == 0)
		{
			RewardDisplay rewardDisplay = Instantiate(ccr, childIndex);
			rewardDisplay.SetCountText(amt);
			rewardDisplay.GetComponent<Animator>().SetTrigger(ContentControllerRewards.QuantityUpdate);
			AudioManager.PlayAudio(sfx, rewardDisplay.gameObject);
		}
		yield return andThen;
	}
}
