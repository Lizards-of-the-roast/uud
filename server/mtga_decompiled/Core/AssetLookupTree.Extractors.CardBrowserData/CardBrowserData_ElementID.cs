using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.CardBrowserData;

public class CardBrowserData_ElementID : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = null;
		if (string.IsNullOrEmpty(bb.CardBrowserElementID))
		{
			return false;
		}
		value = bb.CardBrowserElementID;
		return true;
	}
}
