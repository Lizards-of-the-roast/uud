using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.RegionPair;

public class RegionPair_ToOwner : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.RegionPair.ToOwner;
		return true;
	}
}
