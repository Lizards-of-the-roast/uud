using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_GrpId : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData == null)
		{
			return false;
		}
		return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, (int)GetGrpId(bb.CardData));
	}

	internal static uint GetGrpId(ICardDataAdapter cardData)
	{
		if (cardData.Printing != null && cardData.Printing.IsRebalanced && cardData.Printing.RebalancedCardLink != 0)
		{
			return cardData.Printing.RebalancedCardLink;
		}
		return cardData.GrpId;
	}
}
