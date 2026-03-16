using System.Collections.Generic;
using System.Linq;
using TMPro;
using Wotc.Mtga.Client.Models.Mercantile;

namespace Core.Meta.MainNavigation.Store.Tabs;

public class FeaturedTabLogic : StoreTabLogic
{
	public override string TitleText => "MainNav/Store/Featured";

	public override bool ShowTaxDisclaimer => true;

	public override bool ShowDropRatesLink => true;

	public override List<StoreItem> GetItemsToDisplay(StoreSetFilterToggles setFilters, TMP_Dropdown storeFilterDropdown)
	{
		StoreItem storeItem = (from x in base.Store.Sales
			where x.HasRemainingPurchases
			orderby x.SaleIndex descending
			select x).FirstOrDefault();
		List<StoreItem> collection = (from x in base.Store.Featured
			where x.HasRemainingPurchases
			orderby x.FeaturedIndex descending
			select x).ToList();
		List<StoreItem> list = new List<StoreItem>();
		if (storeItem != null)
		{
			list.Add(storeItem);
		}
		list.AddRange(collection);
		return list;
	}
}
