using System.Collections.Generic;
using Wotc.Mtga.Client.Models.Mercantile;

namespace Core.Meta.MainNavigation.Store.Utils;

public static class StoreSortUtils
{
	public static void ResyncSiblingOrder(StoreItemBase item, IReadOnlyList<StoreItem> targetOrder)
	{
		if (targetOrder.Count < 1)
		{
			return;
		}
		for (int i = 0; i < targetOrder.Count; i++)
		{
			if (item._storeItem == targetOrder[i])
			{
				item.transform.SetSiblingIndex(i);
				break;
			}
		}
	}
}
