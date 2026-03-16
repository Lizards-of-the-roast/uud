using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Core.Code.Promises;
using UnityEngine;
using Wizards.Arena.Client.Logging;
using Wizards.Arena.Enums.System;
using Wizards.Arena.Promises;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Store;
using Wizards.Unification.Models.Mercantile;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;

public abstract class StoreManager : IStoreManager
{
	public static bool ForceCrashAfterPurchase;

	public static bool ForceCrashBeforeEntitlements;

	public static bool DebugAddFakeListingToCatalogRequest;

	protected IAccountClient _accountClient;

	private const string SaleTag2 = "PriceSlash_1";

	private const string SaleTag3 = "Ribbon_2";

	private const string SaleTag3Payload = "MainNav/Store/Tags/PercentOff;{percent}";

	protected Wizards.Arena.Client.Logging.ILogger _logger;

	protected IBILogger _biLogger;

	protected MercantileCollections MercantileCollections = new MercantileCollections();

	private bool _storeEnabled;

	private Coroutine _entitlePollingCoroutine;

	protected IMercantileServiceWrapper _mercantile;

	protected ListingsProvider _listingsProvider;

	protected ICosmeticsServiceWrapper _cosmetics;

	public SaleInfo SaleInfo { get; set; }

	public ClientStoreStatus StoreStatus { get; private set; }

	public string ListingsHash { get; private set; }

	public AvatarCatalog AvatarCatalog => MercantileCollections.AvatarCatalog;

	public CardBackCatalog CardbackCatalog => MercantileCollections.CardBackCatalog;

	public PetCatalog PetCatalog => MercantileCollections.PetCatalog;

	public CardSkinCatalog CardSkinCatalog => MercantileCollections.CardSkinCatalog;

	public EmoteCatalog EmoteCatalog => MercantileCollections.EmoteCatalog;

	public BundleCatalog BundleCatalog => MercantileCollections.BundleCatalog;

	public Dictionary<string, StoreItem> StoreListings => MercantileCollections.StoreListings;

	public IReadOnlyList<StoreItem> Avatars => MercantileCollections.StoreListings.Values.Where((StoreItem p) => p.StoreSection == EStoreSection.Avatars).ToList();

	public IReadOnlyList<StoreItem> Sleeves => MercantileCollections.StoreListings.Values.Where((StoreItem p) => p.StoreSection == EStoreSection.CardSleeves).ToList();

	public IReadOnlyList<StoreItem> CardSkins => MercantileCollections.StoreListings.Values.Where((StoreItem p) => p.StoreSection == EStoreSection.CardSkins).ToList();

	public IReadOnlyList<StoreItem> Bundles => MercantileCollections.StoreListings.Values.Where((StoreItem p) => p.StoreSection == EStoreSection.Bundles).ToList();

	public IReadOnlyList<StoreItem> Boosters => MercantileCollections.StoreListings.Values.Where((StoreItem p) => p.StoreSection == EStoreSection.Packs).ToList();

	public IReadOnlyList<StoreItem> Gems => MercantileCollections.StoreListings.Values.Where((StoreItem p) => p.StoreSection == EStoreSection.Gems).ToList();

	public IReadOnlyList<StoreItem> ProgressionTracks => MercantileCollections.StoreListings.Values.Where((StoreItem p) => p.StoreSection == EStoreSection.ProgressionTracks).ToList();

	public IReadOnlyList<StoreItem> Featured => MercantileCollections.StoreListings.Values.Where((StoreItem p) => p.FeaturedIndex > 0).ToList();

	public IReadOnlyList<StoreItem> Pets => MercantileCollections.StoreListings.Values.Where((StoreItem p) => p.StoreSection == EStoreSection.Pets).ToList();

	public IReadOnlyList<StoreItem> Sales => MercantileCollections.StoreListings.Values.Where((StoreItem p) => p.StoreSection == EStoreSection.Sale).ToList();

	public IReadOnlyList<StoreItem> Decks => MercantileCollections.StoreListings.Values.Where((StoreItem p) => p.StoreSection == EStoreSection.Decks).ToList();

	public IReadOnlyList<StoreItem> PrizeWall => MercantileCollections.StoreListings.Values.Where((StoreItem p) => p.StoreSection == EStoreSection.PrizeWall).ToList();

	public bool StoreEnabled
	{
		get
		{
			return _storeEnabled;
		}
		set
		{
			_storeEnabled = value;
			this.OnStoreEnabledSet?.Invoke(value);
		}
	}

	public event Action OnDataRefreshed;

	public event Action<bool> OnStoreEnabledSet;

	public Promise<Client_EntitlementsResponse> GetEntitlements(bool shouldRetry = false)
	{
		return _mercantile.CheckEntitlements(shouldRetry).IfError(delegate(Promise<Client_EntitlementsResponse> x)
		{
			if (x.State == PromiseState.Timeout)
			{
				MainThreadDispatcher.Dispatch(delegate
				{
					SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Store_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Payment_Removal_Error_Text"));
				});
			}
		});
	}

	public StoreManager(IAccountClient accountClient, Wizards.Arena.Client.Logging.ILogger logger, IBILogger biLogger)
	{
		_logger = logger;
		_biLogger = biLogger;
		_accountClient = accountClient;
		_listingsProvider = Pantry.Get<ListingsProvider>();
		_mercantile = Pantry.Get<IMercantileServiceWrapper>();
		SaleInfo = new SaleInfo
		{
			saleTag2 = "PriceSlash_1",
			saleTag3 = "Ribbon_2",
			saleTag3Payload = "MainNav/Store/Tags/PercentOff;{percent}"
		};
	}

	protected void BI_SendPurchaseFunnelError(string error)
	{
		_biLogger.Send(ClientBusinessEventType.PurchaseFunnel, PurchaseFunnel.CreateErrorInfo(error));
	}

	public void BI_SendErrorLogs<T>(Promise<T> handle, string productId = null)
	{
		if (productId != null)
		{
			BI_SendPurchaseFunnelError("Error ValidateRequest: " + handle.Error.Code + " Message: " + handle.Error.Message);
			_logger.Error("Error ValidateRequest: " + handle.Error.Code + " Message: " + handle.Error.Message);
		}
		if (handle.State != PromiseState.Timeout)
		{
			_biLogger.Send(ClientBusinessEventType.StoreError, new StoreError
			{
				EventTime = DateTime.UtcNow,
				Code = (ServerErrors)handle.Error.Code,
				ErrorMessage = handle.Error.Message,
				ElapsedMilliseconds = handle.ElapsedMilliseconds,
				PromiseState = handle.State.ToString(),
				ProductId = productId
			});
		}
	}

	public virtual void OnLoad()
	{
	}

	public virtual void OnDestroy()
	{
	}

	public void ClearSaleTimer()
	{
		SaleInfo.nextSaleRefreshTime = null;
	}

	public bool AllTrackContentEnabled()
	{
		foreach (StoreItem progressionTrack in ProgressionTracks)
		{
			if (!progressionTrack.Enabled)
			{
				return false;
			}
		}
		return true;
	}

	public IEnumerator RefreshStoreDataYield(Action<bool> onComplete)
	{
		Promise<ClientStoreStatus> statusPromise = _mercantile.GetStoreStatus();
		yield return statusPromise.AsCoroutine();
		if (!statusPromise.Successful || !statusPromise.Result.StoreEnabled)
		{
			StoreEnabled = false;
			if (!statusPromise.Successful)
			{
				StoreStatus = new ClientStoreStatus
				{
					StoreEnabled = false,
					CodeRedemptionEnabled = false
				};
			}
			onComplete?.Invoke(obj: false);
			yield break;
		}
		StoreStatus = statusPromise.Result;
		_biLogger.Send(ClientBusinessEventType.PurchaseFunnel, PurchaseFunnel.Create(ClientPurchaseFunnelContext.PlayerUpdateStoreSku));
		Promise<MercantileCollections> collectionsPromise = _mercantile.GetMercantileCollections();
		while (!collectionsPromise.IsDone)
		{
			yield return null;
		}
		if (!collectionsPromise.Successful)
		{
			MercantileCollections = new MercantileCollections();
			StoreEnabled = false;
			StoreStatus.StoreEnabled = false;
			BI_SendErrorLogs(collectionsPromise);
			onComplete?.Invoke(obj: false);
		}
		else
		{
			MercantileCollections = collectionsPromise.Result;
			SaleInfo.nextSaleRefreshTime = ComputeNextSaleRefreshTime(Sales);
			yield return ProcessRMTListingsYield();
			ListingsHash = ComputeProductsHash();
			RefreshEnabledStates();
			StoreEnabled = true;
			onComplete?.Invoke(obj: true);
			this.OnDataRefreshed?.Invoke();
		}
	}

	public static DateTime? ComputeNextSaleRefreshTime(IEnumerable<StoreItem> saleItems)
	{
		DateTime? result = null;
		foreach (StoreItem saleItem in saleItems)
		{
			if (!result.HasValue || result.Value < saleItem.ExpireTime)
			{
				result = saleItem.ExpireTime;
			}
		}
		return result;
	}

	public abstract IEnumerator ProcessRMTListingsYield();

	public string ComputeProductsHash()
	{
		if (MercantileCollections.StoreListings.Values == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (StoreItem value in MercantileCollections.StoreListings.Values)
		{
			stringBuilder.Append(value.Id);
		}
		StringBuilder stringBuilder2 = new StringBuilder();
		using (SHA256 sHA = SHA256.Create())
		{
			byte[] array = sHA.ComputeHash(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder2.Append(array[i].ToString("x2"));
			}
		}
		return stringBuilder2.ToString();
	}

	protected void ValidateRMTItems(List<RmtProductInfo> rmtProductInfos)
	{
		List<string> list = new List<string>();
		foreach (StoreItem value in MercantileCollections.StoreListings.Values)
		{
			List<Client_PurchaseOption> purchaseOptions = value.PurchaseOptions;
			if (purchaseOptions == null || purchaseOptions.Count == 0)
			{
				continue;
			}
			Client_PurchaseOption rmtPurchaseOption = value.PurchaseOptions.FirstOrDefault((Client_PurchaseOption po) => po.CurrencyType == Client_PurchaseCurrencyType.RMT);
			if (rmtPurchaseOption != null)
			{
				RmtProductInfo rmtProductInfo = rmtProductInfos.FirstOrDefault((RmtProductInfo x) => x.SkuId == rmtPurchaseOption.CurrencyId);
				if (rmtProductInfo != null)
				{
					value.LocalizedPrice = rmtProductInfo.LocalizedPriceString;
					continue;
				}
				_logger.Warn("[StoreManager] Could not find rmt info for " + value.Id + ". Item will not display.");
				list.Add(value.Id);
			}
		}
		foreach (string item in list)
		{
			MercantileCollections.StoreListings.Remove(item);
		}
	}

	private void RefreshEnabledStates()
	{
		bool flag = _accountClient.AccountInformation != null && _accountClient.AccountInformation.HasRole_WotCAccess();
		foreach (StoreItem storeItem in MercantileCollections.StoreListings.Values)
		{
			if (flag)
			{
				storeItem.Enabled = true;
				continue;
			}
			bool flag2 = StoreStatus.DisabledListings?.Contains(storeItem.Id) ?? false;
			bool flag3 = StoreStatus.DisabledTags?.Exists((EProductTag t) => storeItem.ProductTags.Contains(t)) ?? false;
			storeItem.Enabled = !flag3 && !flag2;
		}
	}

	public void DisplayPromiseErrorDialog(ServerErrors errorCode, Client_PurchaseCurrencyType currencyType, StoreItem item = null)
	{
		string empty = string.Empty;
		switch (errorCode)
		{
		case ServerErrors.Store_StoreNotEnabled:
			empty = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Store_Not_Enabled");
			break;
		case ServerErrors.Store_InvalidQuantity:
			if (item != null)
			{
				empty = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/PurchaseResult_BuyLimit", (item.Id, item.LimitRemaining.ToString()));
				break;
			}
			goto default;
		case ServerErrors.Store_DisabledListingError:
			empty = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/PurchaseResult_ProductNotActive");
			break;
		case ServerErrors.Store_MaxQuantityError:
			empty = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/PurchaseResult_QtyOverAccountMax");
			break;
		case ServerErrors.Store_InvalidListingError:
			empty = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/PurchaseResult_ProductNotFound");
			break;
		case ServerErrors.Store_IncorrectCurrency:
			empty = currencyType switch
			{
				Client_PurchaseCurrencyType.Gold => Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Order_Error_NotEnoughGold"), 
				Client_PurchaseCurrencyType.Gem => Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Order_Error_NotEnoughTotalGems"), 
				_ => Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Order_Error_MustUseGemsOrGold"), 
			};
			break;
		default:
			empty = Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/Internal_Error");
			break;
		}
		SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Store_Error_Title"), empty);
	}

	public static MTGALocalizedString GetPreorderAvailableString(DateTime preorderDate, bool isPurchased = false)
	{
		return GetPreorderAvailableString(isPurchased ? "MainNav/Store/PreorderAvailableMonth" : "MainNav/Store/Bundles/PreOrder_Available_Purchasable", preorderDate, isPurchased);
	}

	public static MTGALocalizedString GetPreorderAvailableString(MTGALocalizedString locKey, DateTime preorderDate, bool isPurchased = false)
	{
		locKey.Parameters = new Dictionary<string, string>
		{
			{
				"month",
				preorderDate.ToShortDateString()
			},
			{
				"dateTime",
				preorderDate.ToShortDateString()
			}
		};
		return locKey;
	}

	public IEnumerator PurchaseItemYield(StoreItem item, Client_PurchaseCurrencyType currencyType, Action<StoreItem> onPurchaseSuccessful = null, int quantity = 1, string customTokenId = null)
	{
		if (currencyType == Client_PurchaseCurrencyType.RMT)
		{
			yield return PAPA.StartGlobalCoroutine(PurchaseRMTItemYield(item));
			yield break;
		}
		WrapperController.EnableLoadingIndicator(enabled: true);
		Promise<InventoryInfoShared> promise = _mercantile.PurchaseProduct(item, quantity, currencyType, customTokenId);
		yield return promise.AsCoroutine();
		WrapperController.EnableLoadingIndicator(enabled: false);
		if (promise.Successful)
		{
			onPurchaseSuccessful?.Invoke(item);
			yield break;
		}
		BI_SendErrorLogs(promise, item.Id);
		DisplayPromiseErrorDialog((ServerErrors)promise.Error.Code, currencyType, item);
	}

	public abstract IEnumerator PurchaseRMTItemYield(StoreItem item);

	public virtual void OpenPaymentSetup()
	{
	}
}
