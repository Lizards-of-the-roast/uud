using System;
using System.Collections.Generic;
using TMPro;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Extensions;

namespace Core.Meta.MainNavigation.Store.Tabs;

public class InvalidTabLogic : StoreTabLogic
{
	public override List<StoreItem> GetItemsToDisplay(StoreSetFilterToggles setFilters, TMP_Dropdown storeFilterDropdown)
	{
		return new List<StoreItem>();
	}

	public override void ActivateStoreFilterDropdown(TMP_Dropdown storeFilterDropdown, Action<int> onValueChanged)
	{
		storeFilterDropdown.onValueChanged.RemoveAllListeners();
		storeFilterDropdown.gameObject.UpdateActive(active: false);
	}
}
