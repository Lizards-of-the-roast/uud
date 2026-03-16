using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Indirectors.CardData;

public class CardData_LinkedTokens : IIndirector
{
	private ICardDataAdapter _cache;

	public void SetCache(IBlackboard bb)
	{
		_cache = bb.CardData;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.SetCardDataExtensive(_cache);
		_cache = null;
	}

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.CardData == null)
		{
			yield break;
		}
		ICardDataAdapter cachedCardData = bb.CardData;
		foreach (KeyValuePair<uint, IReadOnlyList<CardPrintingData>> item in cachedCardData.Printing.AbilityIdToLinkedTokenPrinting)
		{
			foreach (CardPrintingData item2 in item.Value)
			{
				bb.SetCardDataExtensive(new GreClient.CardData.CardData(null, item2));
				yield return bb;
			}
		}
		bb.SetCardDataExtensive(cachedCardData);
	}
}
