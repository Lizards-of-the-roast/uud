using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.CardData;

public class CardData_ArtSize : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb.CardData == null)
		{
			return false;
		}
		if (bb.CardData.Printing == null)
		{
			return false;
		}
		value = (int)bb.CardData.Printing.ArtSize;
		return true;
	}
}
