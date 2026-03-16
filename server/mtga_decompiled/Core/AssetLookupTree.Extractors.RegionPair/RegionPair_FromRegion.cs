using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.RegionPair;

public class RegionPair_FromRegion : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.RegionPair.FromRegion;
		return true;
	}
}
