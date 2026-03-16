using System;
using System.Collections.Generic;
using System.Linq;
using Core.Code.Promises;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using WAS;
using Wizards.Arena.Client.Logging;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Mtga.Store;

namespace Core.Shared.Code.Store.Steam;

public class SteamCustomStore : UnityEngine.Purchasing.Extension.Store
{
	private readonly ILogger _logger;

	public ConnectionState ConnectionState { get; private set; }

	public SteamCustomStore(ILogger logger)
	{
		ConnectionState = ConnectionState.Disconnected;
		_logger = logger;
	}

	public override void Connect()
	{
		if (!Enum.TryParse<TransactionType>(Pantry.CurrentEnvironment.xsollaTransactionType, out var result))
		{
			result = TransactionType.Production;
			_logger.Error($"!!! unknown transaction type ({Pantry.CurrentEnvironment.xsollaTransactionType}) in environment settings, using {result} as default");
		}
		ConnectionState = ConnectionState.Connected;
		ConnectCallback?.OnStoreConnectionSucceeded();
	}

	public override void FetchProducts(IReadOnlyCollection<ProductDefinition> products)
	{
		Pantry.Get<IAccountClient>().GetStoreItems(SteamPurchaseController.Currency).ThenOnMainThread(delegate(Promise<ItemsResponse> p)
		{
			_logger.Info("Fetching products for " + p.Name);
			IEnumerable<ProductDescription> source = p.Result?.items.Select(ToProductDescription) ?? Array.Empty<ProductDescription>();
			ProductsCallback?.OnProductsFetched(source.ToList().AsReadOnly());
		});
	}

	public override void FetchPurchases()
	{
		PurchaseFetchCallback?.OnAllPurchasesRetrieved(Array.Empty<Order>());
	}

	public override void Purchase(ICart cart)
	{
		Product product = cart.Items().FirstOrDefault().Product;
		_logger.Info($"Purchasing product {product}");
		new PurchaseSteamProductUniTask(PurchaseCallback, Pantry.Get<IAccountClient>(), product).Load().Forget();
	}

	public override void FinishTransaction(PendingOrder pendingOrder)
	{
		_logger.Info("Finishing transaction " + pendingOrder.CartOrdered.Items().FirstOrDefault().Product.definition.id);
		ConfirmCallback?.OnConfirmOrderSucceeded(pendingOrder.Info.TransactionID);
	}

	public override void CheckEntitlement(ProductDefinition product)
	{
		throw new NotImplementedException();
	}

	private static ProductDescription ToProductDescription(Item item)
	{
		string text = StoreUtils.ValidatedIsoCurrencyCode(item.currency) ?? SteamPurchaseController.Currency;
		return new ProductDescription(item.sku, new ProductMetadata(StoreUtils.GetLocalizedRmtPriceString(item.price, text), item.localized_name, item.localized_description, text, (decimal)item.price));
	}
}
