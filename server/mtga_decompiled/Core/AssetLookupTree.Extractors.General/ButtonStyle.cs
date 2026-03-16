using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class ButtonStyle : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.ButtonStyle;
		return true;
	}
}
