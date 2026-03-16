using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Interaction;

public class Interaction_ManaPaymentsCount : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.GreAction?.AutoTapSolution?.ManaPayments != null)
		{
			return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, bb.GreAction.AutoTapSolution.ManaPayments.Count);
		}
		return false;
	}
}
