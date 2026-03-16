using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.General;

public class DieFaces : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, (int)bb.DieFaces);
	}
}
