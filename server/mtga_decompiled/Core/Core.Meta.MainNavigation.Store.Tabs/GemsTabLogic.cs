using System.Collections.Generic;
using System.Linq;
using TMPro;
using Wotc.Mtga.Client.Models.Mercantile;

namespace Core.Meta.MainNavigation.Store.Tabs;

public class GemsTabLogic : StoreTabLogic
{
	public override string TitleText => "MainNav/Store/Purchase_Gems";

	public override bool ShowTaxDisclaimer => true;

	public override List<StoreItem> GetItemsToDisplay(StoreSetFilterToggles setFilters, TMP_Dropdown storeFilterDropdown)
	{
		return base.Store.Gems.OrderByDescending((StoreItem x) => x.SortIndex).ToList();
	}
}
