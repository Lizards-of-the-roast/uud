using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Extractors.CardData;

public class CardData_TitleId : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb.CardData == null)
		{
			return false;
		}
		if (bb.CardData.Printing != null && bb.CardData.Printing.IsRebalanced && bb.CardData.Printing.RebalancedCardLink != 0 && bb.CardDataProvider != null)
		{
			CardPrintingData cardPrintingById = bb.CardDataProvider.GetCardPrintingById(bb.CardData.Printing.RebalancedCardLink);
			if (cardPrintingById != null)
			{
				value = (int)cardPrintingById.TitleId;
				return true;
			}
		}
		value = (int)bb.CardData.TitleId;
		return true;
	}
}
