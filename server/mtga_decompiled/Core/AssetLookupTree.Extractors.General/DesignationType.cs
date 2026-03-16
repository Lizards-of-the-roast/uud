using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class DesignationType : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.Designation.Value;
		return bb.Designation.HasValue;
	}
}
