using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Indirectors;

public class ActiveResolution_Card : IIndirector
{
	private ICardDataAdapter _cachedModel;

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.ActiveResolution?.Model != null)
		{
			bb.SetCardDataExtensive(bb.ActiveResolution.Model);
			yield return bb;
		}
	}

	public void SetCache(IBlackboard bb)
	{
		_cachedModel = bb.CardData;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.SetCardDataRaw(_cachedModel);
		_cachedModel = null;
	}
}
