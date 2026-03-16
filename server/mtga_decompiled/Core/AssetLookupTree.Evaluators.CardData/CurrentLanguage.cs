using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class CurrentLanguage : EvaluatorBase_List<string>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<string>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.Language);
	}
}
