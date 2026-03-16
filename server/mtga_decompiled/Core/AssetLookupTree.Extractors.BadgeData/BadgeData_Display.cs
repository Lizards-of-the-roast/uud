using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.Ability.Metadata;

namespace AssetLookupTree.Extractors.BadgeData;

public class BadgeData_Display : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (!(bb.BadgeData is BadgeEntryData badgeEntryData))
		{
			return false;
		}
		value = (int)badgeEntryData.Display;
		return true;
	}
}
