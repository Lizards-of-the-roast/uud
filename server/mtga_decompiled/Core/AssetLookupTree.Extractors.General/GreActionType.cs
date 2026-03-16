using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class GreActionType : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.GreActionType;
		return true;
	}
}
