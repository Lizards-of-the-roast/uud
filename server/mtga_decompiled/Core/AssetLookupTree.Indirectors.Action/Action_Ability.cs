using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Indirectors.Action;

public class Action_Ability : IIndirector
{
	private AbilityPrintingData _cacheAbility;

	public void SetCache(IBlackboard bb)
	{
		_cacheAbility = bb.Ability;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.Ability = _cacheAbility;
		_cacheAbility = null;
	}

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.GreAction == null)
		{
			yield break;
		}
		if (bb.GreAction.AlternativeGrpId != 0)
		{
			AbilityPrintingData abilityPrintingData = bb.AbilityDataProvider?.GetAbilityPrintingById(bb.GreAction.AlternativeGrpId);
			if (abilityPrintingData != null)
			{
				bb.Ability = abilityPrintingData;
				yield return bb;
				yield break;
			}
		}
		if (bb.GreAction.AbilityGrpId != 0)
		{
			AbilityPrintingData abilityPrintingData2 = bb.AbilityDataProvider?.GetAbilityPrintingById(bb.GreAction.AbilityGrpId);
			if (abilityPrintingData2 != null)
			{
				bb.Ability = abilityPrintingData2;
				yield return bb;
			}
		}
	}
}
