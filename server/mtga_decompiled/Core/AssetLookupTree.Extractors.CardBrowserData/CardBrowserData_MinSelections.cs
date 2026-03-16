using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.CardBrowserData;

public class CardBrowserData_MinSelections : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		if (bb.SelectCardBrowserMinMax.HasValue)
		{
			value = bb.SelectCardBrowserMinMax.Value.min;
			return true;
		}
		value = 0;
		return false;
	}
}
