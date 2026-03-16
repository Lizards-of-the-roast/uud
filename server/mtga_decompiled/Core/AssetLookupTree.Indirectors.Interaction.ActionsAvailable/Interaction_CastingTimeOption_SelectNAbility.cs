using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;

namespace AssetLookupTree.Indirectors.Interaction.ActionsAvailable;

public class Interaction_CastingTimeOption_SelectNAbility : IIndirector
{
	private AbilityPrintingData _cachedAbility;

	public void SetCache(IBlackboard bb)
	{
		_cachedAbility = bb.Ability;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.Ability = _cachedAbility;
		_cachedAbility = null;
	}

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.AbilityDataProvider == null || !(bb.Request is CastingTimeOptionRequest castingTimeOptionRequest))
		{
			yield break;
		}
		foreach (BaseUserRequest childRequest in castingTimeOptionRequest.ChildRequests)
		{
			if (childRequest is CastingTimeOption_SelectNRequest castingTimeOption_SelectNRequest && bb.AbilityDataProvider.TryGetAbilityPrintingById(castingTimeOption_SelectNRequest.GrpId, out var ability))
			{
				bb.Ability = ability;
				yield return bb;
			}
		}
	}
}
