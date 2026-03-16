using System;
using System.Collections.Generic;
using Wizards.Arena.Promises;
using Wizards.MDN;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Inventory;
using Wizards.Unification.Models.Player;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Code.Harnesses.OfflineHarnessServices;

public class HarnessInventoryServiceWrapper : IInventoryServiceWrapper
{
	public InventoryInfoShared ExposedInventoryInfoShared;

	public Action OnInventoryUpdated { get; set; }

	public Action OnCardsUpdated { get; set; }

	public Action PublishEvents { get; set; }

	public ClientPlayerInventory Inventory
	{
		get
		{
			return new ClientPlayerInventory();
		}
		set
		{
		}
	}

	public Dictionary<uint, int> Cards { get; set; }

	public Dictionary<uint, int> newCards { get; set; }

	public Dictionary<uint, int> CardsToTagNew { get; set; }

	public List<ClientInventoryUpdateReportItem> Updates { get; }

	public event Action<int> GemsChanged;

	public event Action<int> GoldChanged;

	public event Action<List<ClientBoosterInfo>> BoostersChanged;

	public void OnReconnect()
	{
		throw new NotImplementedException();
	}

	public void OnInventoryInfoUpdated_AWS(InventoryInfoShared obj)
	{
		throw new NotImplementedException();
	}

	public Promise<CardsAndCacheVersion> GetPlayerCards(int playerCardsVersion)
	{
		throw new NotImplementedException();
	}

	public Promise<InventoryInfo> RedeemWildcards(WildcardBulkRequest request)
	{
		throw new NotImplementedException();
	}

	public Promise<string> RedeemVoucher(string voucherId)
	{
		throw new NotImplementedException();
	}

	public Promise<InventoryInfoShared> CrackBooster(string boosterCollationId, int numBoostersToOpen)
	{
		return new SimplePromise<InventoryInfoShared>(ExposedInventoryInfoShared);
	}

	public Promise<bool> CompleteVault()
	{
		throw new NotImplementedException();
	}
}
