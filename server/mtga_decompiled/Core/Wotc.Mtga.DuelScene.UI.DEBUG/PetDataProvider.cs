using System.Collections.Generic;
using AssetLookupTree.Extractors.General;
using AssetLookupTree.Nodes;
using AssetLookupTree.Payloads.Cosmetic;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class PetDataProvider : IPetDataProvider
{
	private readonly AssetLookupTreeLoader _treeLoader;

	private List<(string petId, string variantId)> _allPets;

	public PetDataProvider(AssetLookupTreeLoader treeLoader)
	{
		_treeLoader = treeLoader;
	}

	public IReadOnlyList<(string petId, string variantId)> GetAllPetData()
	{
		return _allPets ?? (_allPets = new List<(string, string)>(LoadAllPetData(_treeLoader)));
	}

	private static IReadOnlyList<(string petId, string variantId)> LoadAllPetData(AssetLookupTreeLoader treeLoader)
	{
		HashSet<(string, string)> hashSet = new HashSet<(string, string)> { (string.Empty, string.Empty) };
		foreach (INode<PetPayload> item in treeLoader.LoadTree<PetPayload>().EnumerateNodes())
		{
			if (!(item is BucketNode<PetPayload, string> bucketNode) || !(bucketNode.Extractor is PetId))
			{
				continue;
			}
			foreach (KeyValuePair<string, INode<PetPayload>> child in bucketNode.Children)
			{
				string key = child.Key;
				if (!(child.Value is BucketNode<PetPayload, string> bucketNode2) || !(bucketNode2.Extractor is PetVariantId))
				{
					continue;
				}
				foreach (string key2 in bucketNode2.Children.Keys)
				{
					hashSet.Add((key, key2));
				}
			}
		}
		List<(string, string)> list = new List<(string, string)>(hashSet);
		list.Sort(delegate((string petId, string variantId) x, (string petId, string variantId) y)
		{
			int num = x.petId.CompareTo(y.petId);
			return (num != 0) ? num : x.variantId.CompareTo(y.variantId);
		});
		return list;
	}
}
