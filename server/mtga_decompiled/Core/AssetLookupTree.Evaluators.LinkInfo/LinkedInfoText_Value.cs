using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.LinkInfo;

public class LinkedInfoText_Value : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.LinkedInfoText.Value);
	}
}
