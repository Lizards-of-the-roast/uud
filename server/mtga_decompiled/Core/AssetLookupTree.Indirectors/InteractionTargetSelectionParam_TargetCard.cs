using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Indirectors;

public class InteractionTargetSelectionParam_TargetCard : IIndirector
{
	private ICardDataAdapter _cachedCardData;

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.TargetSelectionParams.TargetCard != null)
		{
			bb.SetCardDataExtensive(bb.TargetSelectionParams.TargetCard);
			yield return bb;
		}
	}

	public void SetCache(IBlackboard bb)
	{
		_cachedCardData = bb.CardData;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.SetCardDataRaw(_cachedCardData);
		_cachedCardData = null;
	}
}
