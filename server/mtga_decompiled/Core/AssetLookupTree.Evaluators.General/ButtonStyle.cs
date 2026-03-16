using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.General;

public class ButtonStyle : EvaluatorBase_List<global::ButtonStyle.StyleType>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<global::ButtonStyle.StyleType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.ButtonStyle);
	}
}
