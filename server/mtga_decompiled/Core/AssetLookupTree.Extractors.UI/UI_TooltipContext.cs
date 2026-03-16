using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.UI;

public class UI_TooltipContext : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.TooltipContext;
		return true;
	}
}
