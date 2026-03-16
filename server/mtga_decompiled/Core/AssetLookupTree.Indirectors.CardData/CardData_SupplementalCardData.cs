using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Indirectors.CardData;

public class CardData_SupplementalCardData : IndirectorBase_Enum<SupplementalKey>
{
	private ICardDataAdapter _cacheCardData;

	public override void SetCache(IBlackboard bb)
	{
		_cacheCardData = bb.CardData;
	}

	public override void ClearCache(IBlackboard bb)
	{
		bb.SetCardDataExtensive(_cacheCardData);
		_cacheCardData = null;
	}

	public override IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.CardData != null && bb.SupplementalCardData.TryGetValue(base.ExpectedValue, out var value))
		{
			bb.SetCardDataExtensive(value);
			yield return bb;
		}
	}
}
