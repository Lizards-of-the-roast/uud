using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Indirectors;

public class ActiveResolution_Ability : IIndirector
{
	private AbilityPrintingData _cachedAbility;

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.ActiveResolution?.AbilityPrinting != null)
		{
			bb.Ability = bb.ActiveResolution.AbilityPrinting;
			yield return bb;
		}
		if (bb.ActiveResolution?.Model == null)
		{
			yield break;
		}
		foreach (AbilityPrintingData ability in bb.ActiveResolution.Model.Abilities)
		{
			AbilityCategory category = ability.Category;
			if ((uint)(category - 3) <= 1u || category == AbilityCategory.Chained)
			{
				bb.Ability = ability;
				yield return bb;
			}
		}
	}

	public void SetCache(IBlackboard bb)
	{
		_cachedAbility = bb.Ability;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.Ability = _cachedAbility;
		_cachedAbility = null;
	}
}
