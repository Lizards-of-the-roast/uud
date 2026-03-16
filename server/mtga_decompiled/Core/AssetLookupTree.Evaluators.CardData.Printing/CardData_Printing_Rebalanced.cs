using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData.Printing;

public class CardData_Printing_Rebalanced : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Printing != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.CardData.Printing.IsRebalanced);
		}
		return false;
	}
}
