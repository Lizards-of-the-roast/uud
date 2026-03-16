using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.CardBrowserData;

public class CardBrowserData_BrowserType : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.CardBrowserType;
		return true;
	}
}
