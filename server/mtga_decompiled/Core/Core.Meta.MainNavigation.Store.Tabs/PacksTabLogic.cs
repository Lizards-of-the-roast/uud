using System.Collections.Generic;
using System.Linq;
using TMPro;
using Wizards.Arena.Enums.Set;
using Wotc.Mtga.Client.Models.Mercantile;

namespace Core.Meta.MainNavigation.Store.Tabs;

public class PacksTabLogic : StoreTabLogic
{
	public override bool ShowPackTitle => true;

	public override bool ShowDropRatesLink => true;

	public override bool ShowUniversesBeyondLogo(StoreSetFilterToggles setFilters)
	{
		return setFilters.SelectedModel.Tags.Contains(GroupTag.UniversesBeyond);
	}

	public override List<StoreItem> GetItemsToDisplay(StoreSetFilterToggles setFilters, TMP_Dropdown storeFilterDropdown)
	{
		HashSet<string> selectedSets = setFilters.SelectedModel.HashSetSet;
		Dictionary<string, int> selectedSetsSortOrder = setFilters.SelectedModel.SelectedSetsSortOrder;
		return (from x in base.Store.Boosters
			where selectedSets.Contains(x.SubType)
			orderby selectedSetsSortOrder[x.SubType]
			select x).ThenBy((StoreItem b) => b.PackCount).ToList();
	}
}
