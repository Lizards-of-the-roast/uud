using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Indirectors.CardHolder;

public class CardHolder_Cards : IIndirector
{
	private ICardDataAdapter _cache;

	public void SetCache(IBlackboard bb)
	{
		_cache = bb.CardData;
		bb.SetCardDataRaw(null);
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.SetCardDataRaw(_cache);
		_cache = null;
	}

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.CardHolder == null)
		{
			yield break;
		}
		foreach (DuelScene_CDC cardView in bb.CardHolder.CardViews)
		{
			if ((bool)cardView && cardView.Model != null)
			{
				bb.SetCardDataExtensive(cardView.Model);
				yield return bb;
			}
		}
	}
}
