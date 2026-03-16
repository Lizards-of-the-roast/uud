using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Prompt;

public class PromptParameter_Id : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, (int)bb.PromptParameterId);
	}
}
