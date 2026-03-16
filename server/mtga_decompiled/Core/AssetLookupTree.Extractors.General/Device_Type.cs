using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class Device_Type : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.DeviceType;
		return true;
	}
}
