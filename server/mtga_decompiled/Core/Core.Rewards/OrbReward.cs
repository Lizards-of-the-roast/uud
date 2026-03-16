using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.MainNavigation.RewardTrack;
using Core.Meta.MainNavigation.Store;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga.FrontDoorModels;

namespace Core.Rewards;

[Serializable]
public class OrbReward : ItemReward<OrbInventoryChange, RewardDisplay>
{
	protected override RewardType _rewardType => RewardType.Orb;

	private RewardDisplay GetIndexPrefab(int index, AssetLookupSystem assetLookupSystem)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.RewardType = _rewardType;
		assetLookupSystem.Blackboard.TargetIndex = index;
		RewardPrefabs payload = assetLookupSystem.TreeLoader.LoadTree<RewardPrefabs>().GetPayload(assetLookupSystem.Blackboard);
		return AssetLoader.AcquireAndTrackAsset<GameObject>(assetTracker, "OrbReward", payload.PrefabPath).GetComponent<RewardDisplay>();
	}

	public override float GetWidth(AssetLookupSystem als)
	{
		if (_prefab == null)
		{
			_prefab = GetIndexPrefab(0, als);
		}
		LayoutElement component = _prefab.gameObject.GetComponent<LayoutElement>();
		if (!component)
		{
			return 100f;
		}
		return component.minWidth;
	}

	public RewardDisplay Instantiate(int colorIndex, Transform parent, int childIndex, AssetLookupSystem als)
	{
		return Instantiate(GetIndexPrefab(colorIndex, als), parent, childIndex);
	}

	public override void AddFromInventoryUpdate(ClientInventoryUpdateReportItem inventoryUpdate, PrecalculatedRewardUpdateInfo cache)
	{
	}

	public override IEnumerable<Func<RewardDisplayContext, IEnumerator>> DisplayRewards(ContentControllerRewards ccr)
	{
		int orbTally = 0;
		foreach (OrbInventoryChange change in ToAdd)
		{
			yield return (RewardDisplayContext ctxt) => ShowOrbColors(ccr, change, ctxt.ChildIndex);
			orbTally += change.currentGenericOrbAmount - change.oldGenericOrbAmount;
		}
		if (orbTally > 0)
		{
			yield return (RewardDisplayContext ctxt) => ShowGenericOrbs(ccr, orbTally, ctxt.ChildIndex);
		}
	}

	private IEnumerator ShowOrbColors(ContentControllerRewards ccr, OrbInventoryChange change, int childIndex)
	{
		IEnumerable<int> recentlyUnlockedColors = change.RecentlyUnlockedColors;
		foreach (int item in recentlyUnlockedColors ?? Enumerable.Empty<int>())
		{
			RewardDisplay rewardDisplay = Instantiate(item, ccr.RewardParent, childIndex, ccr.AssetLookupSystem);
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_masterytree_orb_acquire, rewardDisplay.gameObject);
			yield return new WaitForSeconds(ccr._sequenceDelay);
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
