using System;
using System.Collections.Generic;
using System.Linq;
using SolidUtilities;
using TMPro;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wotc.Mtga.Client.Models.Mercantile;

namespace Core.Meta.MainNavigation.Store.Tabs;

public class DecksTabLogic : StoreTabLogic
{
	private string _seenDecksHash;

	private string _storeDecksHash;

	private static readonly string PlayerPrefsDecksSeenHashKey = "Store.DecksViewed";

	private readonly Func<string, Promise<string>> _getPref;

	public override string TitleText => "MainNav/Store/Purchase_Decks";

	public DecksTabLogic(Func<string, Promise<string>> getPref = null, Func<IStoreManager> getStoreManager = null)
		: base(getStoreManager)
	{
		_getPref = getPref ?? new Func<string, Promise<string>>(DefaultGetPref);
	}

	private static Promise<string> DefaultGetPref(string key)
	{
		return Pantry.Get<PlayerPrefsDataProvider>().GetPreference(key);
	}

	public override List<StoreItem> GetItemsToDisplay(StoreSetFilterToggles setFilters, TMP_Dropdown storeFilterDropdown)
	{
		return base.Store.Decks.OrderByDescending((StoreItem x) => x.SortIndex).ToList();
	}

	public override void UpdateSeenToCurrent()
	{
		_seenDecksHash = CalculateDecksHash(base.Store);
		Pantry.Get<PlayerPrefsDataProvider>().SetPreference(PlayerPrefsDecksSeenHashKey, _seenDecksHash);
	}

	public override Promise<bool> GetIsHot()
	{
		return _getPref(PlayerPrefsDecksSeenHashKey).Convert(delegate(string seenDecksHash)
		{
			if (_seenDecksHash == null)
			{
				_seenDecksHash = seenDecksHash;
			}
			if (_storeDecksHash == null)
			{
				_storeDecksHash = CalculateDecksHash(base.Store);
			}
			return _storeDecksHash != _seenDecksHash;
		});
	}

	public static string CalculateDecksHash(IStoreManager storeManager)
	{
		return Hash.SHA1(string.Join("", from x in storeManager.Decks
			orderby x.SortIndex descending, x.Id
			select x));
	}
}
