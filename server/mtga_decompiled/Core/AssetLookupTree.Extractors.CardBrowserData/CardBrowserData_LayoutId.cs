using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.CardBrowserData;

public class CardBrowserData_LayoutId : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = null;
		if (string.IsNullOrEmpty(bb.CardBrowserLayoutID))
		{
			return false;
		}
		value = bb.CardBrowserLayoutID;
		return true;
	}
}
