using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Interaction;

public class Interaction_SelectableCount : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, (int)bb.SelectionParams.Selectable);
	}
}
