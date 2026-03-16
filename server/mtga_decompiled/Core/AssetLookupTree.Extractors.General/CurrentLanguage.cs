using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class CurrentLanguage : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = null;
		if (bb.Language == null)
		{
			return false;
		}
		value = bb.Language;
		return true;
	}
}
