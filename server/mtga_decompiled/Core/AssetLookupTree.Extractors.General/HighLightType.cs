using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class HighLightType : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.HighlightType;
		return true;
	}
}
