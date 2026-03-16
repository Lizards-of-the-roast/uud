using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Browser;

public class Browser_CardCount : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardBrowserCardCount.HasValue)
		{
			return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, (int)bb.CardBrowserCardCount.Value);
		}
		return false;
	}
}
