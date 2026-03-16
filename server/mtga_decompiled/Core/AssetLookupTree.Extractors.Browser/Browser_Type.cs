using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Browser;

public class Browser_Type : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.CardBrowserType;
		return true;
	}
}
