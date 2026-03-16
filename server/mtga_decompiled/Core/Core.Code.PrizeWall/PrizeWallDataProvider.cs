using System;
using System.Collections.Generic;
using System.Linq;
using Core.Meta.MainNavigation.Store.Utils;
using Core.Shared.Code.Network;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Unification.Models.Mercantile;
using Wotc.Mtga.Client.Models.Mercantile;

namespace Core.Code.PrizeWall;

public class PrizeWallDataProvider
{
	private readonly IPrizeWallServiceWrapper _prizeWallServiceWrapper;

	private Dictionary<string, Client_PrizeWall> _prizeWalls;

	private bool _initialized;

	private bool _enabled;

	private InventoryManager _inventoryManager;

	private StoreManager Store => WrapperController.Instance.Store;

	public PrizeWallDataProvider(IPrizeWallServiceWrapper prizeWallServiceWrapper, InventoryManager inventoryManager)
	{
		_prizeWallServiceWrapper = prizeWallServiceWrapper;
		_inventoryManager = inventoryManager;
	}

	public static PrizeWallDataProvider Create()
	{
		return new PrizeWallDataProvider(Pantry.Get<IPrizeWallServiceWrapper>(), Pantry.Get<InventoryManager>());
	}

	public Dictionary<string, Client_PrizeWall> GetAllActivePrizeWalls()
	{
		if (!_enabled)
		{
			return new Dictionary<string, Client_PrizeWall>();
		}
		if (!_initialized)
		{
			SimpleLog.LogError("Attempting to get Prize Walls before PrizeWallDataProvider is initialized.");
		}
		return _prizeWalls;
	}

	public Client_PrizeWall GetStoreTabPrizeWall()
	{
		if (!_enabled)
		{
			return null;
		}
		if (!_initialized)
		{
			SimpleLog.LogError("Attempting to get Prize Walls before PrizeWallDataProvider is initialized.");
			return null;
		}
		return (from pw in _prizeWalls.Values
			where pw.AppearsAsStoreTab
			orderby pw.SpendStopDate
			select pw).FirstOrDefault();
	}

	public Client_PrizeWall GetPrizeWallById(string prizeWallId)
	{
		if (!_enabled)
		{
			return null;
		}
		if (!_initialized)
		{
			SimpleLog.LogError("Attempting to get Prize Walls before PrizeWallDataProvider is initialized.");
			return null;
		}
		if (string.IsNullOrEmpty(prizeWallId))
		{
			return null;
		}
		if (!_prizeWalls.TryGetValue(prizeWallId, out var value))
		{
			SimpleLog.LogError("Unknown prize wall id.");
			return null;
		}
		return value;
	}

	public bool IsPrizeWallUnlocked(string prizeWallId)
	{
		if (_prizeWalls.TryGetValue(prizeWallId, out var value))
		{
			if (value.IsPrizeWallFree)
			{
				return true;
			}
			return _inventoryManager.Inventory.prizeWallsUnlocked.Contains(prizeWallId);
		}
		return false;
	}

	public bool WasPrizeWallRefunded(string prizeWallId)
	{
		if (_prizeWalls.TryGetValue(prizeWallId, out var value) && !value.IsPrizeWallFree && !IsPrizeWallUnlocked(prizeWallId) && GetPrizeWallUnlockListing(prizeWallId) == null)
		{
			return true;
		}
		return false;
	}

	public StoreItem GetPrizeWallUnlockListing(string prizeWallId)
	{
		return Store.PrizeWall.FirstOrDefault((StoreItem item) => item.ListingType == EListingType.PrizeWall && item.PrizeWallData != null && item.PrizeWallData.AssociatedPrizeWall == prizeWallId);
	}

	public bool IsPrizeWallBeyondEarnStopDate(string prizeWallId)
	{
		if (!_enabled)
		{
			return false;
		}
		if (!_initialized)
		{
			SimpleLog.LogError("Attempting to get information about Prize Walls before PrizeWallDataProvider is initialized.");
			return false;
		}
		if (!_prizeWalls.TryGetValue(prizeWallId, out var value))
		{
			SimpleLog.LogError("Unknown prize wall id.");
			return false;
		}
		return DateTime.UtcNow >= value.EarnStopDate;
	}

	public int GetPrizeWallCurrencyQuantity(string prizeWallId)
	{
		if (_prizeWalls.TryGetValue(prizeWallId, out var value) && !string.IsNullOrEmpty(value.CurrencyCustomTokenId) && _inventoryManager.Inventory.CustomTokens.TryGetValue(value.CurrencyCustomTokenId, out var value2))
		{
			return value2;
		}
		return 0;
	}

	public List<Client_PrizeWall> GetPrizeWallsByCurrencyId(string customTokenId)
	{
		List<Client_PrizeWall> list = new List<Client_PrizeWall>();
		if (_prizeWalls == null || !_prizeWalls.Any())
		{
			return list;
		}
		foreach (Client_PrizeWall item in _prizeWalls.Values.OrderBy((Client_PrizeWall pw) => pw?.SpendStopDate))
		{
			if (item?.CurrencyCustomTokenId == customTokenId)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static bool PrizeWallHasAffordableItems(StoreManager store, string prizeWallId, int ownedCurrencyAmt)
	{
		foreach (StoreItem item in store.PrizeWall)
		{
			if (item != null && item.PrizeWallData?.AssociatedPrizeWall == prizeWallId && item.ListingType != EListingType.PrizeWall && item.Enabled && item.HasRemainingPurchases && !StoreDisplayUtils.IsPreorder(item) && item.PurchaseOptions.Exists((Client_PurchaseOption po) => po.Price <= ownedCurrencyAmt))
			{
				return true;
			}
		}
		return false;
	}

	public Promise<List<Client_PrizeWall>> Initialize(bool enabled)
	{
		_enabled = enabled;
		if (!_enabled)
		{
			return new SimplePromise<List<Client_PrizeWall>>(null);
		}
		return _prizeWallServiceWrapper.GetAllPrizeWalls().Then(delegate(Promise<List<Client_PrizeWall>> promise)
		{
			if (promise.Successful)
			{
				_prizeWalls = promise.Result.ToDictionary((Client_PrizeWall prizeWallDef) => prizeWallDef.Id);
				_initialized = true;
			}
			else
			{
				PromiseExtensions.Logger.Error($"Failed to get Prize Wall Definitions: {promise.Error}");
			}
		});
	}

	public void Dispose()
	{
		_prizeWalls = null;
	}
}
