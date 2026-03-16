using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.RegionPair;

public class RegionPair_ToRegion : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.RegionPair.ToRegion;
		return true;
	}
}
