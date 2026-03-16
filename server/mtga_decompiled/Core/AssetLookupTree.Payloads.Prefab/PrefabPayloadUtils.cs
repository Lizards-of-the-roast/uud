using UnityEngine;

namespace AssetLookupTree.Payloads.Prefab;

public static class PrefabPayloadUtils
{
	public static string GetPrefabPath<TPayload, TPrefab>(this AssetLookupSystem assets) where TPayload : PrefabPayload<TPrefab> where TPrefab : Object
	{
		assets.Blackboard.Clear();
		AssetLookupTree<TPayload> assetLookupTree = assets.TreeLoader.LoadTree<TPayload>(returnNewTree: false);
		return ((assetLookupTree != null) ? assetLookupTree.GetPayload(assets.Blackboard) : null)?.PrefabPath;
	}

	public static AltAssetReference<TPrefab> GetPrefab<TPayload, TPrefab>(this AssetLookupSystem assets) where TPayload : PrefabPayload<TPrefab> where TPrefab : Object
	{
		AssetLookupTree<TPayload> assetLookupTree = assets.TreeLoader.LoadTree<TPayload>(returnNewTree: false);
		return ((assetLookupTree != null) ? assetLookupTree.GetPayload(assets.Blackboard) : null)?.Prefab;
	}
}
