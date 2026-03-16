using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Evaluators.CardData.Printing;

public class CardData_Printing_TitleIdList : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Printing == null)
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
				goto IL_008b;
			}
		}
		num = bb.CardData.Printing.TitleId;
		goto IL_008b;
		IL_008b:
		return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, (int)num);
	}
}
