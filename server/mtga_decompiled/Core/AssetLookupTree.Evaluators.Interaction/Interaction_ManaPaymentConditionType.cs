using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Interaction;

public class Interaction_ManaPaymentConditionType : EvaluatorBase_List<ManaPaymentConditionType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.ManaPaymentCondition != null)
		{
			return EvaluatorBase_List<ManaPaymentConditionType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.ManaPaymentCondition.Type);
		}
		return false;
	}
}
