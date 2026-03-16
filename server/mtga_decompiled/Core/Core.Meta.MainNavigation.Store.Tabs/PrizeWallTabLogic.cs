using System;
using System.Collections.Generic;
using System.Linq;
using Core.Code.PrizeWall;
using TMPro;
using Wizards.Mtga;
using Wizards.Unification.Models.Mercantile;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace Core.Meta.MainNavigation.Store.Tabs;

public class PrizeWallTabLogic : StoreTabLogic
{
	private PrizeWallDataProvider _prizeWallDataProvider;

	public override bool ShowPaymentInfoButton => false;

	public override void ActivateStoreFilterDropdown(TMP_Dropdown storeFilterDropdown, Action<int> onValueChanged)
	{
		List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>
		{
			new TMP_Dropdown.OptionData(Languages.ActiveLocProvider.GetLocalizedText("PlayBlade/Filters/Default/All")),
			new TMP_Dropdown.OptionData(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Store_2ndCol_Avatars_Bottom_Center")),
			new TMP_Dropdown.OptionData(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Sleeves_Tab")),
			new TMP_Dropdown.OptionData(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/CardStyles_Tab")),
			new TMP_Dropdown.OptionData(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Filters_Packs_And_More"))
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
		_prizeWallDataProvider = Pantry.Get<PrizeWallDataProvider>();
		Client_PrizeWall storePrizeWall = _prizeWallDataProvider.GetStoreTabPrizeWall();
		EListingType[] filterListingTypes = storeFilterDropdown.value switch
		{
			0 => new EListingType[10]
			{
				EListingType.Avatar,
				EListingType.Sleeve,
				EListingType.ArtStyle,
				EListingType.Card,
				EListingType.Booster,
				EListingType.Bundle,
				EListingType.Economic,
				EListingType.Emote,
				EListingType.Pet,
				EListingType.PreconDeck
			}, 
			1 => new EListingType[1] { EListingType.Avatar }, 
			2 => new EListingType[1] { EListingType.Sleeve }, 
			3 => new EListingType[1] { EListingType.ArtStyle }, 
			4 => new EListingType[7]
			{
				EListingType.Card,
				EListingType.Booster,
				EListingType.Bundle,
				EListingType.Economic,
				EListingType.Emote,
				EListingType.Pet,
				EListingType.PreconDeck
			}, 
			_ => new EListingType[10]
			{
				EListingType.Avatar,
				EListingType.Sleeve,
				EListingType.ArtStyle,
				EListingType.Card,
				EListingType.Booster,
				EListingType.Bundle,
				EListingType.Economic,
				EListingType.Emote,
				EListingType.Pet,
				EListingType.PreconDeck
			}, 
		};
		return base.Store.PrizeWall.OrderByDescending((StoreItem x) => x.SortIndex).Where(delegate(StoreItem item)
		{
			if (item?.PrizeWallData?.AssociatedPrizeWall != storePrizeWall.Id)
			{
				return false;
			}
			if (item != null && item.ListingType == EListingType.PrizeWall)
			{
				return false;
			}
			return ((IReadOnlyCollection<EListingType>)(object)filterListingTypes).Contains(item.ListingType) ? true : false;
		}).ToList();
	}
}
