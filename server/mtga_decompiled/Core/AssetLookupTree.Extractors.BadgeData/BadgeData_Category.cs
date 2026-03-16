using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.BadgeData;

public class BadgeData_Category : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb.BadgeData == null)
		{
			return false;
		}
		value = (int)bb.BadgeData.Category;
		return true;
	}
}
