using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class CardHolderType : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.CardHolderType;
		return true;
	}
}
