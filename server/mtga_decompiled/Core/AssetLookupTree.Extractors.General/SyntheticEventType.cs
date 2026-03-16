using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class SyntheticEventType : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.SyntheticEvent;
		return true;
	}
}
