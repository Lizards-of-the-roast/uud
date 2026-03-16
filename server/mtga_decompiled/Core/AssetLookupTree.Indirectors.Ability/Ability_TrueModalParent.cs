using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Indirectors.Ability;

public class Ability_TrueModalParent : IIndirector
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
		if (bb.Ability != null && bb.CardData != null)
		{
			AbilityPrintingData cacheAbility = bb.Ability;
			if (bb.Ability.TryGetModalAbilityParent(bb.CardData.Abilities, out var parent))
			{
				bb.Ability = parent;
				yield return bb;
			}
			bb.Ability = cacheAbility;
		}
	}
}
