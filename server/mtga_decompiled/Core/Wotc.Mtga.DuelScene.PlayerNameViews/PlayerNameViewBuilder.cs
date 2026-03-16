using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using UnityEngine;
using Wotc.Mtga.DuelScene.UI;

namespace Wotc.Mtga.DuelScene.PlayerNameViews;

public class PlayerNameViewBuilder : IPlayerNameViewBuilder
{
	private readonly AssetLookupSystem _assetLookupSystem;

	public PlayerNameViewBuilder(AssetLookupSystem assetLookupSystem)
	{
		_assetLookupSystem = assetLookupSystem;
	}

	public PlayerName Create(GREPlayerNum playerNum, Transform root)
	{
		return AssetLoader.Instantiate<PlayerName>(GetPrefabPath(playerNum), root);
	}

	private string GetPrefabPath(GREPlayerNum playerNum)
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.GREPlayerNum = playerNum;
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<PlayerNameViewPrefab> loadedTree))
		{
			PlayerNameViewPrefab payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				return payload.PrefabPath;
			}
		}
		return string.Empty;
	}
}
