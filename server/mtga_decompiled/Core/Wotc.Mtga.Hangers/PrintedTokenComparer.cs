using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.FaceInfo;
using GreClient.CardData;

namespace Wotc.Mtga.Hangers;

public class PrintedTokenComparer : IComparer<FaceHanger.FaceCardInfo>
{
	private readonly AssetLookupSystem _assetLookupSystem;

	public PrintedTokenComparer(AssetLookupSystem assetLookupTreeSystem)
	{
		_assetLookupSystem = assetLookupTreeSystem;
	}

	public int Compare(FaceHanger.FaceCardInfo x, FaceHanger.FaceCardInfo y)
	{
		bool flag = x == null;
		bool value = y == null;
		int num = flag.CompareTo(value);
		if (num != 0)
		{
			return num;
		}
		ICardDataAdapter cardData = x.CardData;
		ICardDataAdapter cardData2 = y.CardData;
		flag = cardData == null;
		value = cardData2 == null;
		num = flag.CompareTo(value);
		if (num != 0)
		{
			return num;
		}
		bool value2 = false;
		bool flag2 = false;
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<RulesDefinedToken> loadedTree))
		{
			_assetLookupSystem.Blackboard.Clear();
			_assetLookupSystem.Blackboard.SetCardDataExtensive(cardData);
			value2 = loadedTree.GetPayload(_assetLookupSystem.Blackboard) != null;
			_assetLookupSystem.Blackboard.Clear();
			_assetLookupSystem.Blackboard.SetCardDataExtensive(cardData2);
			flag2 = loadedTree.GetPayload(_assetLookupSystem.Blackboard) != null;
			_assetLookupSystem.Blackboard.Clear();
		}
		return flag2.CompareTo(value2);
	}
}
