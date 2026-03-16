using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Request;

public class Request_PromptId_List : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Request != null)
		{
			return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, (int)(bb.Request.Prompt?.PromptId ?? 0));
		}
		return false;
	}
}
