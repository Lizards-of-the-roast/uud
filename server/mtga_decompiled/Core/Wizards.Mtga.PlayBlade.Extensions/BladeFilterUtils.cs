using System.Collections.Generic;

namespace Wizards.Mtga.PlayBlade.Extensions;

public static class BladeFilterUtils
{
	public static HashSet<string> RemoveSingleUseBladeFilters(List<BladeEventInfo> events)
	{
		HashSet<string> hashSet = new HashSet<string>();
		if (events == null)
		{
			return hashSet;
		}
		HashSet<string> hashSet2 = new HashSet<string>();
		foreach (BladeEventInfo @event in events)
		{
			List<string> dynamicFilterTagIds = @event.DynamicFilterTagIds;
			if (dynamicFilterTagIds == null || dynamicFilterTagIds.Count <= 0)
			{
				continue;
			}
			foreach (string dynamicFilterTagId in @event.DynamicFilterTagIds)
			{
				if (!hashSet2.Add(dynamicFilterTagId))
				{
					hashSet.Add(dynamicFilterTagId);
				}
			}
		}
		return hashSet;
	}
}
