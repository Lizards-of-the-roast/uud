using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Prompt;

public class Prompt_Id : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Prompt != null)
		{
			return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, (int)bb.Prompt.PromptId);
		}
		return false;
	}
}
