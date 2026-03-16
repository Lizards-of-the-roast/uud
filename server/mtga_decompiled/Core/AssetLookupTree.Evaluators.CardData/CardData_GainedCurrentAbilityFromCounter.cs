using AssetLookupTree.Blackboard;
using GreClient.CardData;
using Wotc.Mtga.Extensions;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_GainedCurrentAbilityFromCounter : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData == null || bb.Ability == null)
		{
			return false;
		}
		uint id = bb.Ability.Id;
		if (bb.CardData.PrintingAbilityIds.Contains(id))
		{
			return false;
		}
		if (!Constants.KEYWORD_COUNTER_ABILITY_IDS.TryGetKey(id, out var key))
		{
			return false;
		}
		if (!bb.CardData.Counters.TryGetValue(key, out var value) || value <= 0)
		{
			return false;
		}
		foreach (AbilityPrintingData addedAbility in bb.CardData.AddedAbilities)
		{
			if (addedAbility.Id == id)
			{
				return true;
			}
		}
		return false;
	}
}
