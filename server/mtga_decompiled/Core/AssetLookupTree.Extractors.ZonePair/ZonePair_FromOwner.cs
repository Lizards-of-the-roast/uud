using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.ZonePair;

public class ZonePair_FromOwner : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.ZonePair.FromOwner;
		return true;
	}
}
