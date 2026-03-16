using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class AbilityType : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.AbilityType;
		return true;
	}
}
