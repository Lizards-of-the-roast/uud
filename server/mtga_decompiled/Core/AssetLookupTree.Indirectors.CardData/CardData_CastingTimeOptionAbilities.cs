using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;

namespace AssetLookupTree.Indirectors.CardData;

public class CardData_CastingTimeOptionAbilities : IIndirector
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
		if (bb.AbilityDataProvider == null || bb.CardData == null)
		{
			yield break;
		}
		IEnumerable<CastingTimeOption> castingTimeOptions = bb.CardData.CastingTimeOptions;
		if (castingTimeOptions == null)
		{
			yield break;
		}
		foreach (CastingTimeOption item in castingTimeOptions)
		{
			if (item.AbilityId.HasValue)
			{
				AbilityPrintingData abilityPrintingById = bb.AbilityDataProvider.GetAbilityPrintingById(item.AbilityId.Value);
				if (abilityPrintingById != null)
				{
					bb.Ability = abilityPrintingById;
					yield return bb;
				}
			}
		}
	}
}
