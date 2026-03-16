using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.General;

public class HighlightType : EvaluatorBase_List<global::HighlightType>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<global::HighlightType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.HighlightType);
	}
}
