using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.CardData;

public class CardData_BlockedByCount : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb.CardData?.Instance == null)
		{
			return false;
		}
		value = bb.CardData.Instance.BlockedByIds.Count;
		return true;
	}
}
