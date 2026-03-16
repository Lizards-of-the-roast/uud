using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData.Printing;

public class CardData_Printing_IndicatorColorCount : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Printing?.IndicatorColors != null)
		{
			return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, bb.CardData.Printing.IndicatorColors.Count);
		}
		return false;
	}
}
