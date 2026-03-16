using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Request;

public class Request_PromptId : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Request != null)
		{
			return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, (int)(bb.Request.Prompt?.PromptId ?? 0));
		}
		return false;
	}
}
