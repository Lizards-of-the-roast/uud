using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.ZonePair;

public class ZonePair_ToZone : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.ZonePair.ToZone;
		return true;
	}
}
