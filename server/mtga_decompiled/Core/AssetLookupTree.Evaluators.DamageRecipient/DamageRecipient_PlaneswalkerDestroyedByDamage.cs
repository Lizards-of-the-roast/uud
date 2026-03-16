using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.DamageRecipient;

public class DamageRecipient_PlaneswalkerDestroyedByDamage : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.DamageRecipientEntity != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.DamageRecipientEntity is MtgCardInstance mtgCardInstance && mtgCardInstance.CardTypes.Contains(CardType.Planeswalker) && mtgCardInstance.Counters[CounterType.Loyalty] - bb.DamageAmount <= 0);
		}
		return false;
	}
}
