using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardBrowserData;

public class CardBrowserData_MinSelections : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.SelectCardBrowserMinMax.HasValue)
		{
			return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, bb.SelectCardBrowserMinMax.Value.min);
		}
		return false;
	}
}
