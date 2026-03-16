using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Assets.Core.Meta.LearnMore;
using Core.Code.AssetLookupTree.AssetLookup;
using Pooling;
using UnityEngine;
using Wizards.Mtga;

namespace Wotc.Mtga.LearnMore;

public class LearnToPlayContentBuilder : ILearnToPlayContentBuilder
{
	private readonly IUnityObjectPool _unityObjectPool;

	private readonly AssetLookupSystem _assetLookupSystem;

	public static ILearnToPlayContentBuilder PantryCreate()
	{
		return new LearnToPlayContentBuilder(Pantry.Get<IUnityObjectPool>(), Pantry.Get<AssetLookupManager>());
	}

	public LearnToPlayContentBuilder(IUnityObjectPool unityObjectPool, AssetLookupManager assetLookupManager)
		: this(unityObjectPool, assetLookupManager.AssetLookupSystem)
	{
	}

	public LearnToPlayContentBuilder(IUnityObjectPool unityObjectPool, AssetLookupSystem assetLookupSystem)
	{
		_unityObjectPool = unityObjectPool ?? NullUnityObjectPool.Default;
		_assetLookupSystem = assetLookupSystem;
	}

	public LearnToPlayContents InstantiateContent(string assetPath, string contentName, Transform parent)
	{
		if (string.IsNullOrEmpty(assetPath))
		{
			SimpleLog.LogError("LTP: Null/Empty asset path");
			return null;
		}
		string prefabPath = _assetLookupSystem.GetPrefabPath<LearnToPlayContentsPrefab, LearnToPlayContents>();
		if (string.IsNullOrEmpty(prefabPath))
		{
			SimpleLog.LogError("LTP: Failed prefab path lookup");
			return null;
		}
		GameObject gameObject = _unityObjectPool.PopObject(prefabPath, parent);
		if (gameObject == null)
		{
			SimpleLog.LogError("LTP: Failed to instantiate section content gameobject");
			return null;
		}
		LearnToPlayContents component = gameObject.GetComponent<LearnToPlayContents>();
		if (component == null)
		{
			SimpleLog.LogError("LTP: Null Content component");
			return null;
		}
		component.Init(contentName, _assetLookupSystem, _unityObjectPool);
		return component;
	}

	public void DestroyContent(LearnToPlayContents contents)
	{
		contents.DeInit();
		if (contents.gameObject != null)
		{
			_unityObjectPool.PushObject(contents.gameObject, worldPositionStays: true);
		}
	}
}
