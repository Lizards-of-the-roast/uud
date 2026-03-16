using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class LoyaltyValence : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.LoyaltyValence;
		return true;
	}
}
