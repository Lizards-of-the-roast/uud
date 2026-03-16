using System;
using System.Collections;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Meta.MainNavigation.Store;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga.FrontDoorModels;

namespace Core.Rewards;

[Serializable]
public class CompleteSetReward : ItemReward<string, RewardCompleteSet>
{
	[SerializeField]
	public List<string> setsOfInterest;

	protected override RewardType _rewardType => RewardType.CompleteSet;

	private RewardCompleteSet GetSetPrefab(string setCode, AssetLookupSystem assetLookupSystem)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.RewardType = _rewardType;
		assetLookupSystem.Blackboard.SetCode = setCode;
		RewardPrefabs payload = assetLookupSystem.TreeLoader.LoadTree<RewardPrefabs>().GetPayload(assetLookupSystem.Blackboard);
		return AssetLoader.AcquireAndTrackAsset<GameObject>(assetTracker, "SetReward", payload.PrefabPath).GetComponent<RewardCompleteSet>();
	}

	public override float GetWidth(AssetLookupSystem als)
	{
		if (_prefab == null)
		{
			_prefab = GetSetPrefab("BASE", als);
		}
		LayoutElement component = _prefab.gameObject.GetComponent<LayoutElement>();
		if (!component)
		{
			return 100f;
		}
		return component.minWidth;
	}

	public RewardCompleteSet Instantiate(string setCode, ContentControllerRewards ccr, int childIndex)
	{
		if (setsOfInterest.IndexOf(setCode) >= 0)
		{
			RewardCompleteSet rewardCompleteSet = Instantiate(GetSetPrefab(setCode, ccr.AssetLookupSystem), ccr.RewardParent, childIndex);
			rewardCompleteSet.Init(setCode);
			return rewardCompleteSet;
		}
		RewardCompleteSet rewardCompleteSet2 = Instantiate(ccr, childIndex);
		rewardCompleteSet2.Init(setCode);
		return rewardCompleteSet2;
	}

	public override void AddFromInventoryUpdate(ClientInventoryUpdateReportItem inventoryUpdate, PrecalculatedRewardUpdateInfo cache)
	{
		foreach (string item in cache.SetsToGrant)
		{
			ToAdd.Enqueue(item);
		}
	}

	public override IEnumerable<Func<RewardDisplayContext, IEnumerator>> DisplayRewards(ContentControllerRewards ccr)
	{
		foreach (string setCode in ToAdd)
		{
			yield return (RewardDisplayContext ctxt) => ShowCompleteSetReward(ccr, setCode, ctxt.ChildIndex);
		}
	}

	private IEnumerator ShowCompleteSetReward(ContentControllerRewards ccr, string setCode, int childIndex)
	{
		RewardCompleteSet rewardCompleteSet = Instantiate(setCode, ccr, childIndex);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rewards_card_flipout, rewardCompleteSet.gameObject);
		yield return null;
	}
}
