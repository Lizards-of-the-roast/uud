using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardBrowserData;

public class CardBrowserData_CurrentSelectionLessThanMaxSelections : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.SelectCardBrowserMinMax.HasValue)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.SelectCardBrowserCurrentSelectionCount < bb.SelectCardBrowserMinMax.Value.max);
		}
		return false;
	}
}
