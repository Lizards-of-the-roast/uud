using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace Core.Meta.MainNavigation.Store.Tabs;

public class CosmeticsTabLogic : StoreTabLogic
{
	public override string TitleText => "MainNav/Store/Purchase_Cosmetics";

	public override void ActivateStoreFilterDropdown(TMP_Dropdown storeFilterDropdown, Action<int> onValueChanged)
	{
		List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>
		{
			new TMP_Dropdown.OptionData(Languages.ActiveLocProvider.GetLocalizedText("PlayBlade/Filters/Default/All")),
			new TMP_Dropdown.OptionData(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Store_2ndCol_Avatars_Bottom_Center")),
			new TMP_Dropdown.OptionData(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Sleeves_Tab")),
			new TMP_Dropdown.OptionData(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Profile/PetsTitle"))
		};
		if (storeFilterDropdown.options.Count != list.Count || !storeFilterDropdown.options.Except(list).Any())
		{
			storeFilterDropdown.options = list;
			storeFilterDropdown.value = 0;
		}
		storeFilterDropdown.onValueChanged.RemoveAllListeners();
		storeFilterDropdown.onValueChanged.AddListener(delegate(int idx)
		{
			onValueChanged(idx);
		});
		storeFilterDropdown.gameObject.UpdateActive(active: true);
	}

	public override List<StoreItem> GetItemsToDisplay(StoreSetFilterToggles setFilters, TMP_Dropdown storeFilterDropdown)
	{
		return (from x in storeFilterDropdown.value switch
			{
				0 => base.Store.Avatars.Concat(base.Store.Sleeves).Concat(base.Store.Pets), 
				1 => base.Store.Avatars, 
				2 => base.Store.Sleeves, 
				3 => base.Store.Pets, 
				_ => base.Store.Avatars.Concat(base.Store.Sleeves).Concat(base.Store.Pets), 
			}
			where x.HasRemainingPurchases
			orderby x.SortIndex descending
			select x).ToList();
	}
}
