using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.General;

public class FixedRulesTextSize : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.FixedRulesTextSize);
	}
}
