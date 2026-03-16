using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.CardData;

public class CardData_GrpId : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb.CardData == null)
		{
			return false;
		}
		if (bb.CardData.Printing != null && bb.CardData.Printing.IsRebalanced && bb.CardData.Printing.RebalancedCardLink != 0)
		{
			value = (int)bb.CardData.Printing.RebalancedCardLink;
			return true;
		}
		value = (int)bb.CardData.GrpId;
		return true;
	}
}
