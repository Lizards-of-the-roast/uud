using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Extractors.Cosmetics;
using AssetLookupTree.Nodes;
using AssetLookupTree.Payloads.Card;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class SleeveDataProvider : ISleeveDataProvider
{
	private readonly AssetLookupTreeLoader _treeLoader;

	private List<string> _allSleeves;

	public SleeveDataProvider(AssetLookupTreeLoader treeLoader)
	{
		_treeLoader = treeLoader;
	}

	public IReadOnlyList<string> GetAllSleeves()
	{
		return _allSleeves ?? (_allSleeves = new List<string>(LoadAllSleeves(_treeLoader)));
	}

	private static IReadOnlyList<string> LoadAllSleeves(AssetLookupTreeLoader treeLoader)
	{
		HashSet<string> hashSet = new HashSet<string> { string.Empty };
		AssetLookupTree<TextureOverride> assetLookupTree = treeLoader.LoadTree<TextureOverride>();
		AssetLookupTree<Sleeve> assetLookupTree2 = treeLoader.LoadTree<Sleeve>();
		foreach (INode<TextureOverride> item in assetLookupTree.EnumerateNodes())
		{
			foreach (string sleeveId in GetSleeveIds(item))
			{
				hashSet.Add(sleeveId);
			}
		}
		foreach (INode<Sleeve> item2 in assetLookupTree2.EnumerateNodes())
		{
			foreach (string sleeveId2 in GetSleeveIds(item2))
			{
				hashSet.Add(sleeveId2);
			}
		}
		List<string> list = new List<string>(hashSet);
		list.Sort();
		return list;
	}

	private static IEnumerable<string> GetSleeveIds<T>(INode<T> node) where T : class, IPayload
	{
		if (!(node is BucketNode<T, string> bucketNode) || !(bucketNode.Extractor is Cosmetic_SleeveId))
		{
			yield break;
		}
		foreach (string key in bucketNode.Children.Keys)
		{
			yield return key;
		}
	}
}
