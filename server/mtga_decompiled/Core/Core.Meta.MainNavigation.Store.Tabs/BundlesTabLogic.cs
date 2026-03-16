using System.Collections.Generic;
using System.Linq;
using TMPro;
using Wotc.Mtga.Client.Models.Mercantile;

namespace Core.Meta.MainNavigation.Store.Tabs;

public class BundlesTabLogic : StoreTabLogic
{
	public override string TitleText => "MainNav/Store/Purchase_Bundles";

	public override bool ShowTaxDisclaimer => true;

	public override bool ShowDropRatesLink => true;

	public override List<StoreItem> GetItemsToDisplay(StoreSetFilterToggles setFilters, TMP_Dropdown storeFilterDropdown)
	{
		return base.Store.Bundles.OrderByDescending((StoreItem x) => x.SortIndex).ToList();
	}
}
