using System;
using System.Collections.Generic;
using Wizards.Arena.Promises;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Code.Harnesses.OfflineHarnessServices;

public class HarnessMercantileServiceWrapper : IMercantileServiceWrapper
{
	public event Action<EmoteCatalog> OnEmoteCatalogUpdated;

	public Promise<MercantileCollections> GetMercantileCollections()
	{
		throw new NotImplementedException();
	}

	public Promise<ClientStoreStatus> GetStoreStatus()
	{
		throw new NotImplementedException();
	}

	public Promise<InventoryInfoShared> PurchaseProduct(StoreItem item, int quantity, Client_PurchaseCurrencyType paymentType, string customTokenId = null)
	{
		throw new NotImplementedException();
	}

	public Promise<Client_EntitlementsResponse> CheckEntitlements(bool shouldRetry)
	{
		throw new NotImplementedException();
	}

	public Promise<List<Client_VoucherDefinition>> GetVoucherDefinitions()
	{
		throw new NotImplementedException();
	}
}
