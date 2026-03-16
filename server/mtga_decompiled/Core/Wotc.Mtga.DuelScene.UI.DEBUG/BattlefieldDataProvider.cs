using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Extractors.General;
using AssetLookupTree.Nodes;
using AssetLookupTree.Payloads.Battlefield;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class BattlefieldDataProvider : IBattlefieldDataProvider
{
	private readonly AssetLookupSystem _assetLookupSystem;

	private List<BattlefieldData> _allBattlefields;

	public BattlefieldDataProvider(AssetLookupSystem assetLookupSystem)
	{
		_assetLookupSystem = assetLookupSystem;
	}

	public IReadOnlyList<BattlefieldData> GetAllBattlefields()
	{
		return _allBattlefields ?? (_allBattlefields = new List<BattlefieldData>(LoadAllBattlefieldData()));
	}

	private IReadOnlyList<BattlefieldData> LoadAllBattlefieldData()
	{
		AssetLookupTree<BattlefieldScenePayload> assetLookupTree = _assetLookupSystem.TreeLoader.LoadTree<BattlefieldScenePayload>(returnNewTree: false);
		List<BattlefieldData> list = new List<BattlefieldData>();
		foreach (INode<BattlefieldScenePayload> item in assetLookupTree.EnumerateNodes())
		{
			if (!(item is BucketNode<BattlefieldScenePayload, string> bucketNode) || !(bucketNode.Extractor is BattlefieldId))
			{
				continue;
			}
			foreach (INode<BattlefieldScenePayload> item2 in bucketNode.EnumerateNodes())
			{
				BattlefieldScenePayload result = item2.GetPayload(_assetLookupSystem.Blackboard, _assetLookupSystem.WatcherLogger);
				if (result != null && !list.Exists((BattlefieldData x) => x.Name == result.BattlefieldID) && AssetLoader.HaveAsset(result.ScenePath))
				{
					list.Add(new BattlefieldData(result.BattlefieldID, result.InRandomPool, result.ScenePath));
				}
			}
		}
		list.Sort((BattlefieldData x, BattlefieldData y) => x.Name.CompareTo(y.Name));
		return list;
	}

	public BattlefieldData? GetBattlefieldByName(string name)
	{
		foreach (BattlefieldData allBattlefield in GetAllBattlefields())
		{
			if (allBattlefield.Name == name)
			{
				return allBattlefield;
			}
		}
		return null;
	}

	public string GetDefaultBattlefield()
	{
		IReadOnlyList<BattlefieldData> allBattlefields = GetAllBattlefields();
		if (allBattlefields.Exists((BattlefieldData battlefield) => battlefield.Name == "LGS"))
		{
			return "LGS";
		}
		if (allBattlefields.Count > 0)
		{
			return allBattlefields[0].Name;
		}
		return null;
	}
}
