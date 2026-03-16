using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_TitleIdList : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData == null)
		{
			return false;
		}
		uint num = 0u;
		if (bb.CardData.Printing != null && bb.CardData.Printing.IsRebalanced && bb.CardData.Printing.RebalancedCardLink != 0 && bb.CardDataProvider != null)
		{
			CardPrintingData cardPrintingById = bb.CardDataProvider.GetCardPrintingById(bb.CardData.Printing.RebalancedCardLink);
			if (cardPrintingById != null)
			{
				num = cardPrintingById.TitleId;
				goto IL_007a;
			}
		}
		num = bb.CardData.TitleId;
		goto IL_007a;
		IL_007a:
		return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, (int)num);
	}
}
