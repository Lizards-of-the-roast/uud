using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.LinkInfo;

public class LinkedInfoText_TypeCategory : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.LinkedInfoText.Category;
		return true;
	}
}
