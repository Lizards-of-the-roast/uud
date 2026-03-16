using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardBrowserData;

public class CardBrowserData_FiniteMaxSelections : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.SelectCardBrowserMinMax.HasValue)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.SelectCardBrowserMinMax.Value.max < int.MaxValue);
		}
		return false;
	}
}
