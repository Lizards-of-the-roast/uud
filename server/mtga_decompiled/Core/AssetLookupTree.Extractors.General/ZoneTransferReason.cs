using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class ZoneTransferReason : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.ZoneTransferReason;
		return true;
	}
}
