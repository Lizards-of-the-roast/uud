using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.ZonePair;

public class ZonePair_FromZone : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.ZonePair.FromZone;
		return true;
	}
}
