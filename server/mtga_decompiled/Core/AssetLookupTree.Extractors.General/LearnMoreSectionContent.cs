using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class LearnMoreSectionContent : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = null;
		if (string.IsNullOrEmpty(bb.LearnMoreSectionContentName))
		{
			return false;
		}
		value = bb.LearnMoreSectionContentName;
		return true;
	}
}
