using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData.Printing;

public class CardData_Printing_RebalancedCardLink : EvaluatorBase_List<uint>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Printing != null)
		{
			return EvaluatorBase_List<uint>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.Printing.RebalancedCardLink);
		}
		return false;
	}
}
