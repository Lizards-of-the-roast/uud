using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Request;

public class Request_Type : EvaluatorBase_String
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Request == null)
		{
			return false;
		}
		return EvaluatorBase_String.GetResult(ExpectedValue, Operation, ExpectedResult, bb.Request.GetType().Name);
	}
}
