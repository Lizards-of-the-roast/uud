using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.CardData;

public class CardData_AttachmentCount : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb.CardData == null)
		{
			return false;
		}
		value = bb.CardData.Instance?.AttachedWithIds.Count ?? 0;
		return true;
	}
}
