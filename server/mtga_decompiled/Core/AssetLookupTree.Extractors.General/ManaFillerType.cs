using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class ManaFillerType : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.ManaFillerType;
		return true;
	}
}
