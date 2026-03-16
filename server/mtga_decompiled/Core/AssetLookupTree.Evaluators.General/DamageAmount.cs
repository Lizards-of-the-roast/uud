using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.General;

public class DamageAmount : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.DamageAmount.HasValue)
		{
			return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, bb.DamageAmount.Value);
		}
		return false;
	}
}
