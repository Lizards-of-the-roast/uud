using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Indirectors.CardData;

public class CardData_LinkedFaces : IIndirector
{
	private ICardDataAdapter _cacheCardData;

	public void SetCache(IBlackboard bb)
	{
		_cacheCardData = bb.CardData;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.SetCardDataRaw(_cacheCardData);
		_cacheCardData = null;
	}

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.CardData != null && bb.CardData.LinkedFacePrintings.Count != 0)
		{
			for (int i = 0; i < bb.CardData.LinkedFacePrintings.Count; i++)
			{
				bb.SetCardDataExtensive(bb.CardData.GetLinkedFaceAtIndex(i, ignoreInstance: false, bb.CardDataProvider));
				yield return bb;
			}
		}
	}
}
