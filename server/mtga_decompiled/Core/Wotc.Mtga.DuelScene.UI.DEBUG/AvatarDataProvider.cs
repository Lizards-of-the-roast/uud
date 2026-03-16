using System.Collections.Generic;
using AssetLookupTree.Extractors.Cosmetics;
using AssetLookupTree.Nodes;
using AssetLookupTree.Payloads.Avatar;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class AvatarDataProvider : IAvatarDataProvider
{
	private readonly AssetLookupTreeLoader _treeLoader;

	private List<string> _allAvatars;

	public AvatarDataProvider(AssetLookupTreeLoader treeLoader)
	{
		_treeLoader = treeLoader;
	}

	public IReadOnlyList<string> GetAllAvatars()
	{
		return _allAvatars ?? (_allAvatars = new List<string>(LoadAllAvatars(_treeLoader)));
	}

	private static IReadOnlyList<string> LoadAllAvatars(AssetLookupTreeLoader treeLoader)
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (INode<BustPayload> item in treeLoader.LoadTree<BustPayload>().EnumerateNodes())
		{
			if (item is BucketNode<BustPayload, string> bucketNode && bucketNode.Extractor is Cosmetic_AvatarId)
			{
				hashSet.UnionWith(bucketNode.Children.Keys);
			}
		}
		List<string> list = new List<string>(hashSet);
		list.Sort();
		return list;
	}
}
