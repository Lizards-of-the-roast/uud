using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Interaction;

public class Interaction_ManaPaymentConditionColorsCount : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.ManaPaymentCondition?.Colors != null)
		{
			return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, bb.ManaPaymentCondition.Colors.Count);
		}
		return false;
	}
}
