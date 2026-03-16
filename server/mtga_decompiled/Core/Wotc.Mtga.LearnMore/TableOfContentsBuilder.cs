using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Assets.Core.Meta.LearnMore;
using Core.Code.AssetLookupTree.AssetLookup;
using Pooling;
using UnityEngine;
using Wizards.Mtga;

namespace Wotc.Mtga.LearnMore;

public class TableOfContentsBuilder : ITableOfContentsSectionBuilder
{
	private readonly IUnityObjectPool _unityObjectPool;

	private readonly AssetLookupSystem _assetLookupSystem;

	public static ITableOfContentsSectionBuilder PantryCreate()
	{
		return new TableOfContentsBuilder(Pantry.Get<IUnityObjectPool>(), Pantry.Get<AssetLookupManager>());
	}

	public TableOfContentsBuilder(IUnityObjectPool unityObjectPool, AssetLookupManager assetLookupManager)
		: this(unityObjectPool, assetLookupManager.AssetLookupSystem)
	{
	}

	public TableOfContentsBuilder(IUnityObjectPool unityObjectPool, AssetLookupSystem assetLookupSystem)
	{
		_unityObjectPool = unityObjectPool ?? NullUnityObjectPool.Default;
		_assetLookupSystem = assetLookupSystem;
	}

	public TableOfContentsSection Create(int contentType, Transform parent)
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.TargetIndex = contentType;
		AssetLookupTree<LearnToPlayTableOfContentsPrefab> assetLookupTree = _assetLookupSystem.TreeLoader.LoadTree<LearnToPlayTableOfContentsPrefab>(returnNewTree: false);
		if (assetLookupTree == null)
		{
			SimpleLog.LogError("TOC Create: Failed to load Tree");
			return null;
		}
		LearnToPlayTableOfContentsPrefab payload = assetLookupTree.GetPayload(_assetLookupSystem.Blackboard);
		if (payload == null)
		{
			SimpleLog.LogError($"TOC Create: Null Payload for content type {contentType}");
			return null;
		}
		string prefabPath = payload.PrefabPath;
		if (string.IsNullOrEmpty(prefabPath))
		{
			SimpleLog.LogError($"TOC Create: Null/Empty Prefab Path {contentType}");
			return null;
		}
		GameObject gameObject = _unityObjectPool.PopObject(prefabPath, parent);
		if (gameObject == null)
		{
			SimpleLog.LogError("TOC Create: Failed to unpool prefab (" + prefabPath + ")");
			return null;
		}
		return gameObject.GetComponent<TableOfContentsSection>();
	}

	public void Destroy(TableOfContentsSection tableOfContentsSection)
	{
		tableOfContentsSection.DeInit();
		tableOfContentsSection.SetDisplayState(on: false);
		if (tableOfContentsSection.gameObject != null)
		{
			_unityObjectPool.PushObject(tableOfContentsSection.gameObject, worldPositionStays: true);
		}
	}
}
