using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.CardData;

public class CardData_LeftColor : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb.CardData == null)
		{
			return false;
		}
		switch (bb.CardData.GetFrameColors.Count)
		{
		case 0:
			value = 0;
			return true;
		case 1:
		case 2:
			value = (int)bb.CardData.GetFrameColors[0];
			return true;
		default:
			return false;
		}
	}
}
