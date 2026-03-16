using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardBrowserData;

public class CardBrowserData_FiniteMinSelections : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.SelectCardBrowserMinMax.HasValue)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.SelectCardBrowserMinMax.Value.min < int.MaxValue);
		}
		return false;
	}
}
