using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Avatar;

public class Avatar_PhaseIconType : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.PhaseIconType;
		return true;
	}
}
