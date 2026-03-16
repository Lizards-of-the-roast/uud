using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class CardReaction : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.CardReactionType;
		return true;
	}
}
