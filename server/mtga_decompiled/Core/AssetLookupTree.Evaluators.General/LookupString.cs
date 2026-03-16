using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.General;

public class LookupString : EvaluatorBase_String
{
	public override bool Execute(IBlackboard bb)
	{
		if (!string.IsNullOrWhiteSpace(bb.LookupString))
		{
			return EvaluatorBase_String.GetResult(ExpectedValue, Operation, ExpectedResult, bb.LookupString);
		}
		return false;
	}
}
