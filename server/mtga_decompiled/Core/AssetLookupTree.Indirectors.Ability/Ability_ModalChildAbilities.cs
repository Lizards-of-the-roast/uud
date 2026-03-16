using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Indirectors.Ability;

public class Ability_ModalChildAbilities : IIndirector
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
		if (bb.Ability == null)
		{
			yield break;
		}
		AbilityPrintingData cacheAbility = bb.Ability;
		foreach (AbilityPrintingData modalAbilityChild in bb.Ability.ModalAbilityChildren)
		{
			bb.Ability = modalAbilityChild;
			yield return bb;
		}
		bb.Ability = cacheAbility;
	}
}
