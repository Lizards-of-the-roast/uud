using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.General;

public class RollResult : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.DieRollNaturalResult.HasValue)
		{
			return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, (int)bb.DieRollNaturalResult.Value);
		}
		return false;
	}
}
