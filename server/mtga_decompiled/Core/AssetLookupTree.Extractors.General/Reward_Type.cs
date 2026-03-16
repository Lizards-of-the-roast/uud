using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class Reward_Type : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.RewardType;
		return true;
	}
}
