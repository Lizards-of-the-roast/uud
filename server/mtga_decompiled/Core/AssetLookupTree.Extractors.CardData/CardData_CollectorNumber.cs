using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.CardData;

public class CardData_CollectorNumber : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = null;
		if (bb.CardData == null)
		{
			return false;
		}
		if (bb.CardData.Printing == null)
		{
			return false;
		}
		if (bb.CardData.Printing.CollectorNumber == null)
		{
			return false;
		}
		value = bb.CardData.Printing.CollectorNumber;
		return true;
	}
}
