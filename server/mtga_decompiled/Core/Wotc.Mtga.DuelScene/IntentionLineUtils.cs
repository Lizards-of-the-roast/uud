using System;
using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.DuelScene;
using Pooling;
using UnityEngine;
using Wotc.Mtga.Unity;

namespace Wotc.Mtga.DuelScene;

public static class IntentionLineUtils
{
	private static IntentionLinePrefab GetIntentionLinePayload(AssetLookupSystem assetLookupSystem, string prefabName, int? targetIndex)
	{
		IBlackboard blackboard = assetLookupSystem.Blackboard;
		blackboard.Clear();
		blackboard.PrefabName = prefabName;
		if (targetIndex.HasValue)
		{
			blackboard.TargetIndex = targetIndex.Value;
		}
		IntentionLinePrefab payload = assetLookupSystem.TreeLoader.LoadTree<IntentionLinePrefab>().GetPayload(blackboard);
		if (payload != null)
		{
			return payload;
		}
		return null;
	}

	public static DreamteckIntentionArrowBehavior CreateIntentionLine(AssetLookupSystem assetLookupSystem, string prefabName, IUnityObjectPool unityObjectPool, int? targetIndex = null)
	{
		IntentionLinePrefab intentionLinePayload = GetIntentionLinePayload(assetLookupSystem, prefabName, targetIndex);
		if (intentionLinePayload == null)
		{
			return null;
		}
		string prefabPath = intentionLinePayload.PrefabPath;
		if (string.IsNullOrEmpty(prefabPath))
		{
			throw new NullReferenceException($"PrefabPath for {prefabName} was null/empty.");
		}
		GameObject gameObject = unityObjectPool.PopObject(prefabPath);
		if (gameObject == null)
		{
			throw new NullReferenceException($"Failed to instantiate prefab (\"{prefabPath}\").");
		}
		DreamteckIntentionArrowBehavior component = gameObject.GetComponent<DreamteckIntentionArrowBehavior>();
		if (component == null)
		{
			throw new NullReferenceException($"Null ArrowBehavior on {gameObject.name} prefab");
		}
		return component;
	}
}
