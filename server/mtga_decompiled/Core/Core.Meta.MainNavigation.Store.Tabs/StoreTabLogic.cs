using System;
using System.Collections.Generic;
using TMPro;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Extensions;

namespace Core.Meta.MainNavigation.Store.Tabs;

public abstract class StoreTabLogic
{
	private readonly Func<IStoreManager> _getStoreManager;

	private static readonly Dictionary<StoreTabType, StoreTabLogic> TabLogic = new Dictionary<StoreTabType, StoreTabLogic>
	{
		{
			StoreTabType.Featured,
			new FeaturedTabLogic()
		},
		{
			StoreTabType.Gems,
			new GemsTabLogic()
		},
		{
			StoreTabType.Packs,
			new PacksTabLogic()
		},
		{
			StoreTabType.Bundles,
			new BundlesTabLogic()
		},
		{
			StoreTabType.DailyDeals,
			new DailyDealsTabLogic()
		},
		{
			StoreTabType.Cosmetics,
			new CosmeticsTabLogic()
		},
		{
			StoreTabType.Decks,
			new DecksTabLogic()
		},
		{
			StoreTabType.PrizeWall,
			new PrizeWallTabLogic()
		}
	};

	private static readonly InvalidTabLogic InvalidTabLogic = new InvalidTabLogic();

	protected IStoreManager Store => _getStoreManager?.Invoke() ?? Pantry.Get<StoreManager>();

	public virtual string TitleText => null;

	public virtual bool ShowPackTitle => false;

	public virtual bool ShowTaxDisclaimer => false;

	public virtual bool ShowDropRatesLink => false;

	public virtual bool ShowPaymentInfoButton => true;

	protected StoreTabLogic(Func<IStoreManager> getStoreManager = null)
	{
		_getStoreManager = getStoreManager;
	}

	public virtual bool ShowUniversesBeyondLogo(StoreSetFilterToggles setFilters)
	{
		return false;
	}

	public virtual void UpdateSeenToCurrent()
	{
	}

	public virtual Promise<bool> GetIsHot()
	{
		return new SimplePromise<bool>(result: false);
	}

	public abstract List<StoreItem> GetItemsToDisplay(StoreSetFilterToggles setFilters, TMP_Dropdown storeFilterDropdown);

	public virtual void ActivateStoreFilterDropdown(TMP_Dropdown storeFilterDropdown, Action<int> onValueChanged)
	{
		storeFilterDropdown.onValueChanged.RemoveAllListeners();
		storeFilterDropdown.gameObject.UpdateActive(active: false);
	}

	public static StoreTabLogic LogicForTab(StoreTabType tabType)
	{
		return TabLogic.GetValueOrDefault(tabType, InvalidTabLogic);
	}
}
