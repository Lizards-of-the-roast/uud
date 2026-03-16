using AssetLookupTree.Blackboard;
using GreClient.CardData;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_GainedSpecificAbilityFromCounter : EvaluatorBase_Boolean
{
	public Wotc.Mtgo.Gre.External.Messaging.CounterType CounterType = Wotc.Mtgo.Gre.External.Messaging.CounterType.P1P1;

	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData == null)
		{
			return false;
		}
		if (!bb.CardData.Counters.TryGetValue(CounterType, out var value) || value <= 0)
		{
			return false;
		}
		if (!Constants.KEYWORD_COUNTER_ABILITY_IDS.TryGetValue(CounterType, out var value2))
		{
			return false;
		}
		if (bb.CardData.PrintingAbilityIds.Contains(value2))
		{
			return false;
		}
		foreach (AbilityPrintingData addedAbility in bb.CardData.AddedAbilities)
		{
			if (addedAbility.Id == value2)
			{
				return true;
			}
		}
		return false;
	}
}
