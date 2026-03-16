using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.ManaWheel;

public class ManaWheel_Color : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.ManaColor;
		return true;
	}
}
