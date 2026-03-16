using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardHolder;

public class CardHolder_CardViewCount : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		int valueOrDefault = (bb.CardHolder?.CardViews?.Count).GetValueOrDefault();
		return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, valueOrDefault);
	}
}
