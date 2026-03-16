using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.ZonePair;

public class ZonePair_ToHolder : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.ZonePair.ToHolder;
		return true;
	}
}
