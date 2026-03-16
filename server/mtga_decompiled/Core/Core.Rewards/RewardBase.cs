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
public abstract class RewardBase<P> : IRewardBase where P : Component
{
	private const float Z_SPACING = -50f;

	protected P _prefab;

	private readonly List<P> _instances = new List<P>();

	protected AssetTracker assetTracker = new AssetTracker();

	protected virtual RewardType _rewardType { get; set; }

	public IReadOnlyList<P> Instances => _instances;

	public int InstancesCount => _instances.Count;

	public virtual int ToAddCount { get; set; }

	public P Instantiate(ContentControllerRewards ccr, int childIndex)
	{
		return Instantiate(GetPrefab(ccr.AssetLookupSystem), ccr.RewardParent, childIndex);
	}

	protected P Instantiate(P prefab, Transform parent, int childIndex)
	{
		P val = UnityEngine.Object.Instantiate(prefab, parent, worldPositionStays: false);
		Transform transform = val.transform;
		transform.SetSiblingIndex(childIndex);
		Vector3 vector = -50f * (float)childIndex * Vector3.forward;
		transform.localPosition += vector;
		for (int i = childIndex + 1; i < parent.childCount; i++)
		{
			parent.GetChild(i).localPosition += vector;
		}
		_instances.Add(val);
		return val;
	}

	public void ClearInstances()
	{
		foreach (P instance in Instances)
		{
			if (instance != null && instance.gameObject != null)
			{
				UnityEngine.Object.Destroy(instance.gameObject);
			}
		}
		_instances.Clear();
		_prefab = null;
		assetTracker.Cleanup();
	}

	public void SetPrefab(P prefab)
	{
		_prefab = prefab;
	}

	protected virtual P GetPrefab(AssetLookupSystem assetLookupSystem)
	{
		if (_prefab == null)
		{
			assetLookupSystem.Blackboard.Clear();
			assetLookupSystem.Blackboard.RewardType = _rewardType;
			RewardPrefabs payload = assetLookupSystem.TreeLoader.LoadTree<RewardPrefabs>().GetPayload(assetLookupSystem.Blackboard);
			_prefab = AssetLoader.AcquireAndTrackAsset<GameObject>(assetTracker, "RewardPrefab", payload.PrefabPath).GetComponent<P>();
		}
		return _prefab;
	}

	public virtual float GetWidth(AssetLookupSystem als)
	{
		if (_prefab == null)
		{
			GetPrefab(als);
		}
		float result = 100f;
		if (_prefab == null)
		{
			return result;
		}
		LayoutElement component = _prefab.gameObject.GetComponent<LayoutElement>();
		if (component != null)
		{
			result = ((component.preferredWidth > 0f) ? component.preferredWidth : component.minWidth);
		}
		return result;
	}

	public abstract void ClearAdded();

	public abstract void AddFromInventoryUpdate(ClientInventoryUpdateReportItem inventoryUpdate, PrecalculatedRewardUpdateInfo cache);

	public abstract IEnumerable<Func<RewardDisplayContext, IEnumerator>> DisplayRewards(ContentControllerRewards ccr);
}
