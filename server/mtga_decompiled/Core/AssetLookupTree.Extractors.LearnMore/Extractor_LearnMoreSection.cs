using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.LearnMore;

public class Extractor_LearnMoreSection
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
