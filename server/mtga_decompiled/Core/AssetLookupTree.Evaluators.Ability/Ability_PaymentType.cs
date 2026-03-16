using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Ability;

public class Ability_PaymentType : EvaluatorBase_List<AbilityPaymentType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Ability != null)
		{
			return EvaluatorBase_List<AbilityPaymentType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.Ability.PaymentType);
		}
		return false;
	}
}
