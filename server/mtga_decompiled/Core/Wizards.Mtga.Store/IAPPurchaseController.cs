using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Purchasing;
using Wizards.Arena.Client.Logging;

namespace Wizards.Mtga.Store;

public class IAPPurchaseController : IPurchaseController, IDisposable
{
	protected StoreController Controller;

	private readonly Dictionary<string, IAPPurchaseRequest> _purchaseRequests = new Dictionary<string, IAPPurchaseRequest>();

	private IAPCatalogRequest _activeCatalogRequest;

	protected readonly ILogger _logger;

	private bool _firstTimeSetupRequired = true;

	private bool _disposed;

	public IEnumerable<PurchaseRequest> PendingPurchases => _purchaseRequests.Values.Where((IAPPurchaseRequest request) => request.Order is PendingOrder);

	public IAPPurchaseController(ILogger logger)
	{
		_logger = logger;
	}

	~IAPPurchaseController()
	{
		if (Controller != null)
		{
			Controller.OnPurchasePending -= OnPurchasePending;
			Controller.OnPurchaseDeferred -= OnPurchaseDeferred;
			Controller.OnPurchaseFailed -= OnPurchaseFailed;
			Controller.OnProductsFetched -= OnProductsFetched;
			Controller.OnPurchasesFetched -= OnPurchasesFetched;
			Controller.OnStoreDisconnected -= OnStoreDisconnected;
			Controller.OnProductsFetchFailed -= OnProductsFetchFailed;
			Controller.OnPurchasesFetchFailed -= OnPurchasesFetchFailed;
		}
	}

	public void Initialize(IEnumerable<string> itemListings)
	{
		List<ProductDefinition> list = new List<ProductDefinition>(itemListings.Select((string s) => new ProductDefinition(s, ProductType.Consumable)));
		if (StoreManager.DebugAddFakeListingToCatalogRequest)
		{
			list.Add(new ProductDefinition("fake_bundle", ProductType.Consumable));
		}
		_logger.Debug("[IAPPurchaseController] Initializing");
		InitializeStoreController(list);
	}

	private async Task InitializeStoreController(List<ProductDefinition> productDefinitions)
	{
		if (_firstTimeSetupRequired)
		{
			Controller = UnityIAPServices.StoreController();
			Controller.ProcessPendingOrdersOnPurchasesFetched(shouldProcess: false);
			Controller.OnPurchasePending += OnPurchasePending;
			Controller.OnPurchaseDeferred += OnPurchaseDeferred;
			Controller.OnPurchaseFailed += OnPurchaseFailed;
			Controller.OnPurchaseConfirmed += OnPurchaseConfirmed;
			Controller.OnProductsFetched += OnProductsFetched;
			Controller.OnPurchasesFetched += OnPurchasesFetched;
			Controller.OnStoreDisconnected += OnStoreDisconnected;
			Controller.OnProductsFetchFailed += OnProductsFetchFailed;
			Controller.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
		}
		await Controller.Connect();
		_logger.Debug("[IAPPurchaseController] Connected successfully");
		Controller.FetchProducts(productDefinitions);
	}

	private void OnProductsFetched(List<Product> obj)
	{
		FillCatalogRequest(_activeCatalogRequest, obj);
		if (_firstTimeSetupRequired)
		{
			Controller.FetchPurchases();
		}
		else
		{
			FinalizeCatalogRequest();
		}
	}

	private void OnPurchasesFetched(Orders orders)
	{
		_logger.DebugFormat("[IAPPurchaseController] Purchases fetched from store, {0} Pending", orders.PendingOrders.Count);
		foreach (PendingOrder pendingOrder in orders.PendingOrders)
		{
			Product productFromOrder = GetProductFromOrder(pendingOrder);
			if (_purchaseRequests.TryGetValue(productFromOrder.definition.id, out var value))
			{
				value.SetOrder(pendingOrder);
			}
			else
			{
				value = new IAPPurchaseRequest(productFromOrder, pendingOrder);
				_purchaseRequests.Add(productFromOrder.definition.id, value);
			}
			_logger.DebugFormat("Fetched request for pending purchase constructed\nSKU: {0}\nReceipt: {1}", value.ProductId, value.Receipt);
		}
		FinalizeCatalogRequest();
	}

	private void FillCatalogRequest(IAPCatalogRequest request, IEnumerable<Product> productsFromCatalog)
	{
		request.rmtProductInfos = new List<RmtProductInfo>();
		if (productsFromCatalog == null)
		{
			_logger.Debug("[IAPPurchaseController] FillCatalogRequest called without productsFromCatalog");
			productsFromCatalog = Controller.GetProducts();
		}
		List<Product> list = new List<Product>(productsFromCatalog);
		_logger.DebugFormat("[IAPPurchaseController] Filling catalog with {0} products from catalog", list.Count);
		foreach (Product item in list)
		{
			if (item.availableToPurchase)
			{
				request.rmtProductInfos.Add(new RmtProductInfo
				{
					Price = (float)item.metadata.localizedPrice,
					SkuId = item.definition.id,
					CurrencyCode = item.metadata.isoCurrencyCode,
					LocalizedPriceString = item.metadata.localizedPriceString
				});
			}
		}
		request.products = list;
	}

	private void FinalizeCatalogRequest()
	{
		if (_activeCatalogRequest != null)
		{
			_logger.Debug("[IAPPurchaseController] Catalog finalized and finishing initialization");
			_activeCatalogRequest.SetSuccess("Success");
			_activeCatalogRequest = null;
			_firstTimeSetupRequired = false;
		}
	}

	private void FailCatalogRequest(string message)
	{
		if (_activeCatalogRequest != null)
		{
			_activeCatalogRequest.SetFail(message);
			_activeCatalogRequest = null;
		}
	}

	private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription obj)
	{
		_logger.ErrorFormat("[IAPPurchaseController] Failed to fetch purchases with result {0}: {1}", obj.FailureReason, obj.Message);
		FailCatalogRequest("MainNav/Store/InitializationFailureReason/Unknown");
	}

	private void OnProductsFetchFailed(ProductFetchFailed obj)
	{
		if (obj.FailureReason.Contains("Retrieve Products succeeded"))
		{
			_logger.ErrorFormat("[IAPPurchaseController] Failed to fetch the following products:{0}\nReason:{1}", BuildFailedProductsFetchedText(obj.FailedFetchProducts), obj.FailureReason);
		}
		else
		{
			_logger.ErrorFormat("[IAPPurchaseController] Failed to fetch products with reason {0}", obj.FailureReason);
			FailCatalogRequest("MainNav/Store/InitializationFailureReason/NoProductsAvailable");
		}
		static string BuildFailedProductsFetchedText(List<ProductDefinition> productDefinitions)
		{
			StringBuilder stringBuilder = new StringBuilder(string.Empty);
			foreach (ProductDefinition productDefinition in productDefinitions)
			{
				stringBuilder.AppendFormat("\n{0}", productDefinition.id);
			}
			return stringBuilder.ToString();
		}
	}

	private void OnStoreDisconnected(StoreConnectionFailureDescription obj)
	{
		_logger.ErrorFormat("[IAPPurchaseController] Connection to Store failed with result: {0}", obj.Message);
		FailCatalogRequest("MainNav/Store/InitializationFailureReason/Unknown");
	}

	public CatalogRequest RequestCatalog(IEnumerable<string> itemList)
	{
		IAPCatalogRequest result = (_activeCatalogRequest = new IAPCatalogRequest());
		Initialize(itemList);
		return result;
	}

	public void ConfirmPurchase(PurchaseRequest request)
	{
		IAPPurchaseRequest iAPPurchaseRequest = (IAPPurchaseRequest)request;
		_logger.Debug("[IAPPurchaseController] Confirming purchase of " + iAPPurchaseRequest.Product.definition.id);
		if (iAPPurchaseRequest.Order is PendingOrder order)
		{
			Controller.ConfirmPurchase(order);
		}
		else
		{
			_logger.ErrorFormat("Error confirming purchase of " + iAPPurchaseRequest.ProductId + ", Order is not recognized as Pending");
		}
		_purchaseRequests.Remove(iAPPurchaseRequest.ProductId);
	}

	private void OnPurchasePending(PendingOrder pendingOrder)
	{
		Product productFromOrder = GetProductFromOrder(pendingOrder);
		if (productFromOrder == null)
		{
			_logger.Debug("[IAPPurchaseController] OnPurchasePending called without associated product, bailing");
			return;
		}
		IAPPurchaseRequest valueOrDefault = _purchaseRequests.GetValueOrDefault(productFromOrder.definition.id);
		string text = "(" + productFromOrder.definition.id + ", " + pendingOrder.Info.TransactionID + ")";
		string text2 = ((valueOrDefault == null) ? "None" : ("(" + productFromOrder.definition.id + ")"));
		_logger.Debug("[IAPPurchaseController] Processing purchase:\n\tPurchase " + text + "\n\tRequest " + text2);
		if (valueOrDefault == null)
		{
			valueOrDefault = new IAPPurchaseRequest(productFromOrder, pendingOrder);
			_purchaseRequests.Add(productFromOrder.definition.id, valueOrDefault);
		}
		else
		{
			valueOrDefault.SetOrder(pendingOrder);
		}
	}

	private void OnPurchaseConfirmed(Order order)
	{
		Product productFromOrder = GetProductFromOrder(order);
		IAPPurchaseRequest value;
		if (productFromOrder == null)
		{
			_logger.Debug("[IAPPurchaseController] OnPurchaseConfirmed called without associated product, bailing");
		}
		else if (_purchaseRequests.TryGetValue(productFromOrder.definition.id, out value))
		{
			_logger.Debug("[IAPPurchaseController] Purchase confirmed for:\n\tPurchase " + value.ProductId + "\nReceipt " + value.Receipt);
			value.SetOrder(order);
			_purchaseRequests.Remove(productFromOrder.definition.id);
		}
		else
		{
			_logger.Error("[IAPPurchaseController] OnPurchaseConfirmed called without an existing purchase request for product: " + productFromOrder.definition.id);
		}
	}

	private void OnPurchaseDeferred(DeferredOrder deferredOrder)
	{
		Product productFromOrder = GetProductFromOrder(deferredOrder);
		if (_purchaseRequests.TryGetValue(productFromOrder.definition.id, out var value))
		{
			value.SetOrder(deferredOrder);
		}
		else
		{
			value = new IAPPurchaseRequest(productFromOrder, deferredOrder);
			_purchaseRequests.Add(productFromOrder.definition.id, value);
		}
		_logger.Debug("[IAPPurchaseController] Deferred purchase request " + value.ProductId);
	}

	private void OnPurchaseFailed(FailedOrder failedOrder)
	{
		Product productFromOrder = GetProductFromOrder(failedOrder);
		if (productFromOrder == null)
		{
			_logger.Debug("[IAPPurchaseController] OnPurchaseFailed called without associated product, bailing");
			return;
		}
		IAPPurchaseRequest valueOrDefault = _purchaseRequests.GetValueOrDefault(productFromOrder.definition.id);
		if (valueOrDefault == null)
		{
			_logger.Warn("[IAPPurchaseController] Request not found on for failed purchase " + productFromOrder.definition.id);
			return;
		}
		_purchaseRequests.Remove(productFromOrder.definition.id);
		string text = PurchaseFailureMessage(failedOrder.FailureReason);
		_logger.Debug("Purchase Failed " + productFromOrder.definition.id + " - " + text + " : " + failedOrder.Details);
		valueOrDefault.SetFail(text, ShouldShowError(failedOrder.FailureReason));
	}

	private static bool ShouldShowError(PurchaseFailureReason reason)
	{
		return reason != PurchaseFailureReason.UserCancelled;
	}

	private static string PurchaseFailureMessage(PurchaseFailureReason reason)
	{
		return reason switch
		{
			PurchaseFailureReason.PurchasingUnavailable => "MainNav/Store/PurchaseFailureReason/PurchasingUnavailable", 
			PurchaseFailureReason.ExistingPurchasePending => "MainNav/Store/PurchaseFailureReason/ExistingPurchasePending", 
			PurchaseFailureReason.ProductUnavailable => "MainNav/Store/PurchaseFailureReason/ProductUnavailable", 
			PurchaseFailureReason.SignatureInvalid => "MainNav/Store/PurchaseFailureReason/SignatureInvalid", 
			PurchaseFailureReason.UserCancelled => "MainNav/Store/PurchaseFailureReason/UserCancelled", 
			PurchaseFailureReason.PaymentDeclined => "MainNav/Store/PurchaseFailureReason/PaymentDeclined", 
			PurchaseFailureReason.DuplicateTransaction => "MainNav/Store/PurchaseFailureReason/DuplicateTransaction", 
			_ => "MainNav/Store/PurchaseFailureReason/Unknown", 
		};
	}

	public PurchaseRequest RequestPurchase(string productId)
	{
		if (string.IsNullOrWhiteSpace(productId))
		{
			_logger.Warn("Purchase request with null sku");
			return IAPPurchaseRequest.Fail("Purchase Request null SKU", isError: false);
		}
		if (_purchaseRequests.ContainsKey(productId))
		{
			_logger.Warn("Purchase request with the same sku being processed");
			return IAPPurchaseRequest.Fail("Purchase Request SKU Duplicate", isError: false);
		}
		if (!StoreUtils.MultiPurchasing && _purchaseRequests.Count >= 1)
		{
			_logger.Warn("MainNav/Store/PurchaseInProgress");
			return IAPPurchaseRequest.Fail("MainNav/Store/PurchaseInProgress", isError: true);
		}
		if (Controller == null)
		{
			_logger.Warn("MainNav/Store/PurchasingNotInitialized");
			return IAPPurchaseRequest.Fail("MainNav/Store/PurchasingNotInitialized", isError: true);
		}
		Product productById = Controller.GetProductById(productId);
		if (productById == null)
		{
			_logger.Warn("MainNav/Store/CannotFindProduct " + productId);
			return IAPPurchaseRequest.Fail("MainNav/Store/CannotFindProduct", isError: true);
		}
		IAPPurchaseRequest iAPPurchaseRequest = new IAPPurchaseRequest(productById);
		_purchaseRequests.Add(productId, iAPPurchaseRequest);
		_logger.Debug("Request made for " + productId);
		Controller.PurchaseProduct(productById);
		return iAPPurchaseRequest;
	}

	public void RestoreTransactions(Action<bool, string> callback)
	{
		Controller.RestoreTransactions(callback);
	}

	private Product GetProductFromOrder(Order order)
	{
		CartItem cartItem = order.CartOrdered.Items().FirstOrDefault();
		if (cartItem?.Product == null)
		{
			_logger.Error("[IAPPurchaseController] Failed to fetch product from Order!");
		}
		return cartItem?.Product;
	}

	public void Dispose()
	{
		if (!_disposed && Controller != null)
		{
			Controller.OnPurchasePending -= OnPurchasePending;
			Controller.OnPurchaseDeferred -= OnPurchaseDeferred;
			Controller.OnPurchaseFailed -= OnPurchaseFailed;
			Controller.OnPurchaseConfirmed -= OnPurchaseConfirmed;
			Controller.OnProductsFetched -= OnProductsFetched;
			Controller.OnPurchasesFetched -= OnPurchasesFetched;
			Controller.OnStoreDisconnected -= OnStoreDisconnected;
			Controller.OnProductsFetchFailed -= OnProductsFetchFailed;
			Controller.OnPurchasesFetchFailed -= OnPurchasesFetchFailed;
			_disposed = true;
			Controller = null;
		}
	}
}
