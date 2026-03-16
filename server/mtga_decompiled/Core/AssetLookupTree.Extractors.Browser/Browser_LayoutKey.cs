using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Browser;

public class Browser_LayoutKey : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = bb.CardBrowserLayoutID;
		return !string.IsNullOrWhiteSpace(bb.CardBrowserLayoutID);
	}
}
