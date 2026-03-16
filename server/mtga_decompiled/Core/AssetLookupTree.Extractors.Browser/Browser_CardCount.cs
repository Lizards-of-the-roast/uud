using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Browser;

public class Browser_CardCount : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.CardBrowserCardCount.Value;
		return bb.CardBrowserCardCount.HasValue;
	}
}
