using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class FieldFillerType : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.FieldFillerType;
		return true;
	}
}
