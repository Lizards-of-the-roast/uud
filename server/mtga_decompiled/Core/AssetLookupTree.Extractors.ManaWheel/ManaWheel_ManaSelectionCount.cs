using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.ManaWheel;

public class ManaWheel_ManaSelectionCount : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = bb.ManaSelectionCount;
		return true;
	}
}
