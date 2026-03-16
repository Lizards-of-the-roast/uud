using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Indirectors.CardData;

public class CardData_Owner : IIndirector
{
	private MtgPlayer _cachePlayer;

	public void SetCache(IBlackboard bb)
	{
		_cachePlayer = bb.Player;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.Player = _cachePlayer;
		_cachePlayer = null;
	}

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.CardData?.Owner != null)
		{
			bb.Player = bb.CardData.Owner;
			yield return bb;
		}
	}
}
