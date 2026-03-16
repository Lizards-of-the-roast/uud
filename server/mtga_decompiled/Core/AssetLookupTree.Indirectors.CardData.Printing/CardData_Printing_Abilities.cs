using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Indirectors.CardData.Printing;

public class CardData_Printing_Abilities : IIndirector
{
	private AbilityPrintingData _cache;

	public void SetCache(IBlackboard bb)
	{
		_cache = bb.Ability;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.Ability = _cache;
		_cache = null;
	}

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.CardData?.Printing == null)
		{
			yield break;
		}
		AbilityPrintingData cacheAbility = bb.Ability;
		foreach (AbilityPrintingData ability in bb.CardData.Printing.Abilities)
		{
			bb.Ability = ability;
			yield return bb;
		}
		bb.Ability = cacheAbility;
	}
}
