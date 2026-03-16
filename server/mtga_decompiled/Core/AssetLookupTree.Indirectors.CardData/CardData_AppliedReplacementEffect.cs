using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;

namespace AssetLookupTree.Indirectors.CardData;

public class CardData_AppliedReplacementEffect : IIndirector
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
		if (bb.CardData?.Instance?.AppliedReplacementEffects == null)
		{
			yield break;
		}
		foreach (AppliedReplacementEffectData appliedReplacementEffect in bb.CardData.Instance.AppliedReplacementEffects)
		{
			AbilityPrintingData abilityPrintingById = bb.AbilityDataProvider.GetAbilityPrintingById(appliedReplacementEffect.AbilityId);
			if (abilityPrintingById != null)
			{
				bb.Ability = abilityPrintingById;
				yield return bb;
			}
		}
	}
}
