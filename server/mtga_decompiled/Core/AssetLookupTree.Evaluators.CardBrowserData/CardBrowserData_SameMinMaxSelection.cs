using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardBrowserData;

public class CardBrowserData_SameMinMaxSelection : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.SelectCardBrowserMinMax.HasValue)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.SelectCardBrowserMinMax.Value.min == bb.SelectCardBrowserMinMax.Value.max);
		}
		return false;
	}
}
