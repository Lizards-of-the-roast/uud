using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Indirectors.CardData;

public class CardData_AddedAbilities : IIndirector
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
		if (bb.CardData?.Instance == null)
		{
			yield break;
		}
		AbilityPrintingData cacheAbility = bb.Ability;
		foreach (AbilityPrintingData addedAbility in bb.CardData.AddedAbilities)
		{
			bb.Ability = addedAbility;
			yield return bb;
		}
		bb.Ability = cacheAbility;
	}
}
