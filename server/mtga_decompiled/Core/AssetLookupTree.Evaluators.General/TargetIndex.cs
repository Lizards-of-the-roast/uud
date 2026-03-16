using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.General;

public class TargetIndex : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.TargetIndex.HasValue)
		{
			return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, bb.TargetIndex.Value);
		}
		return false;
	}
}
