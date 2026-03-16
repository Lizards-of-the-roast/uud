using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.CardBrowserData;

public class CardBrowserData_MaxSelections : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		if (bb.SelectCardBrowserMinMax.HasValue)
		{
			value = (int)bb.SelectCardBrowserMinMax.Value.max;
			return true;
		}
		value = 0;
		return false;
	}
}
