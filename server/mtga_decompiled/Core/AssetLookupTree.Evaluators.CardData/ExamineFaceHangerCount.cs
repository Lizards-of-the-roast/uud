using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class ExamineFaceHangerCount : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, (int)bb.ExamineFaceHangerCount);
	}
}
