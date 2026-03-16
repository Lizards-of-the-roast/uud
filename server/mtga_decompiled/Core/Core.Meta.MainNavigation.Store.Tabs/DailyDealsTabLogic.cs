using System.Collections.Generic;
using System.Linq;
using TMPro;
using Wotc.Mtga.Client.Models.Mercantile;

namespace Core.Meta.MainNavigation.Store.Tabs;

public class DailyDealsTabLogic : StoreTabLogic
{
	public override string TitleText => "MainNav/Store/Purchase_CardStyles";

	public override List<StoreItem> GetItemsToDisplay(StoreSetFilterToggles setFilters, TMP_Dropdown storeFilterDropdown)
	{
		return base.Store.Sales.OrderByDescending((StoreItem x) => x.SaleIndex).ToList();
	}
}
