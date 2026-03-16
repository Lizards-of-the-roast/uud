using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Browser;

public class Browser_ElementKey : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = bb.CardBrowserElementID;
		return !string.IsNullOrWhiteSpace(bb.CardBrowserElementID);
	}
}
