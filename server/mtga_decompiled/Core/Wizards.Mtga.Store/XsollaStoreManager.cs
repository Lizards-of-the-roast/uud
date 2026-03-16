using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Meta.MainNavigation.SystemMessage;
using Core.Shared.Code.Store;
using Cysharp.Threading.Tasks;
using WAS;
using Wizards.Arena.Client.Logging;
using Wizards.Arena.Promises;
using Wizards.Models.ClientBusinessEvents;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Loc;

namespace Wizards.Mtga.Store;

public class XsollaStoreManager : StoreManager
{
	private XsollaConnection _xsolla;

	private EntitlementPoll _entitlementPoll;

	private CancellationTokenSource _cts;

	private TransactionType _transactionType;

	private readonly PurchaseFlow _purchaseFlow;

	public XsollaStoreManager(IAccountClient accountClient, ILogger logger, IBILogger biLogger)
		: base(accountClient, logger, biLogger)
	{
		_purchaseFlow = PurchaseFlow.XsollaEmbedded;
	}

	public override IEnumerator PurchaseRMTItemYield(StoreItem item)
	{
		WrapperController.EnableLoadingIndicator(enabled: true);
		int attemptCount = 0;
		bool flag = false;
		Promise<PurchaseTokenResponse> getTokenPromise = null;
		while (attemptCount < 3 && !flag)
		{
			attemptCount++;
			string currencyId = item.PurchaseOptions.FirstOrDefault((Client_PurchaseOption po) => po.CurrencyType == Client_PurchaseCurrencyType.RMT).CurrencyId;
			getTokenPromise = XsollaUtils.GetPurchaseToken(currencyId, _accountClient, _transactionType);
			yield return getTokenPromise.AsCoroutine();
			flag = getTokenPromise.Successful;
		}
		if (flag)
		{
			_biLogger.Send(ClientBusinessEventType.PurchaseFunnel, PurchaseFunnel.Create(ClientPurchaseFunnelContext.PlayerBeginsPurchase));
			_xsolla.OpenBrowser(getTokenPromise.Result.token, getTokenPromise.Result.redirect);
		}
		else
		{
			BI_SendPurchaseFunnelError($"Xsolla Error GetPurchaseToken: {getTokenPromise.Error.Code} - {getTokenPromise.Error.Message}");
			SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Store_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Purchase_Error_Unknown_Title"));
		}
		WrapperController.EnableLoadingIndicator(enabled: false);
	}

	public override void OnLoad()
	{
		if (!Enum.TryParse<TransactionType>(Pantry.CurrentEnvironment.xsollaTransactionType, out var result))
		{
			_transactionType = TransactionType.Production;
			_logger.Error($"!!! unknown transaction type ({Pantry.CurrentEnvironment.xsollaTransactionType}) in environment settings, using {result} as default");
		}
		else
		{
			_transactionType = result;
		}
		_cts = new CancellationTokenSource();
		if (_purchaseFlow == PurchaseFlow.XsollaExternal)
		{
			_xsolla = new ExternalXsollaConnection(result);
			_entitlementPoll = new EntitlementPoll(_mercantile, Pantry.Get<ISystemMessageManager>(), _cts.Token, 1f, 120f, 600f, 1800f);
		}
		else
		{
			_xsolla = new EmbeddedXsollaConnection(result);
			_entitlementPoll = new EntitlementPoll(_mercantile, Pantry.Get<ISystemMessageManager>(), _cts.Token, 1f, 10f, 30f, 120f, 0.1f, 1u);
		}
		_xsolla.OnPurchaseCompleted += OnXsollaPurchaseCompleted;
	}

	public override void OnDestroy()
	{
		_cts?.Cancel();
		_cts?.Dispose();
		_cts = null;
		_xsolla.Dispose();
	}

	private void OnXsollaPurchaseCompleted()
	{
		if (_purchaseFlow == PurchaseFlow.XsollaExternal)
		{
			_entitlementPoll.StartPolling();
		}
		else
		{
			WaitForEntitlements().Forget();
		}
	}

	private async UniTaskVoid WaitForEntitlements()
	{
		WrapperController.EnableLoadingIndicator(enabled: true);
		_entitlementPoll.StartPolling();
		await UniTask.WaitUntil(() => !_entitlementPoll.IsPolling);
		WrapperController.EnableLoadingIndicator(enabled: false);
	}

	public override IEnumerator ProcessRMTListingsYield()
	{
		List<RmtProductInfo> rmtProductInfos = new List<RmtProductInfo>();
		ItemsResponse xsollaResp = null;
		AccountError xsollaError = null;
		int attemptsLeft = 3;
		while (attemptsLeft > 0)
		{
			xsollaResp = null;
			xsollaError = null;
			attemptsLeft--;
			yield return XsollaUtils.TryGetStoreItems(_accountClient, delegate(ItemsResponse x)
			{
				xsollaResp = x;
			}, delegate(AccountError x)
			{
				xsollaError = x;
			}).ToCoroutine();
			if (xsollaError == null)
			{
				break;
			}
			_logger.Error($"Xsolla Error: {xsollaError.ErrorCode} - {xsollaError.ErrorMessage}");
			if (xsollaError.ErrorCode != 503)
			{
				break;
			}
		}
		if (xsollaResp != null)
		{
			string storeCurrencySelection = _accountClient.GetStoreCurrencySelection();
			_logger.Debug("Default currency for SKUs: " + storeCurrencySelection);
			Item[] items = xsollaResp.items;
			foreach (Item item in items)
			{
				string currency = StoreUtils.ValidatedIsoCurrencyCode(item.currency) ?? storeCurrencySelection;
				rmtProductInfos.Add(new RmtProductInfo
				{
					Price = item.price,
					SkuId = item.sku,
					CurrencyCode = item.currency,
					LocalizedPriceString = StoreUtils.GetLocalizedRmtPriceString(item.price, currency)
				});
			}
		}
		else
		{
			BI_SendPurchaseFunnelError($"Xsolla Error GetItems: {xsollaError?.ErrorCode} - {xsollaError?.ErrorMessage}");
			_logger.Error("Failed to get items from Xsolla");
		}
		ValidateRMTItems(rmtProductInfos);
	}

	public override void OpenPaymentSetup()
	{
		PAPA.StartGlobalCoroutine(Coroutine_OpenEditInfoPaymentWebpage());
	}

	private IEnumerator Coroutine_OpenEditInfoPaymentWebpage()
	{
		WrapperController.EnableLoadingIndicator(enabled: true);
		Promise<ProfileToken> getTokenPromise = _accountClient.GetProfileToken();
		yield return getTokenPromise.AsCoroutine();
		if (getTokenPromise.Successful)
		{
			_xsolla.OpenBrowser(getTokenPromise.Result.token, getTokenPromise.Result.redirect);
		}
		else
		{
			BI_SendPurchaseFunnelError($"Xsolla Error GetProfileToken: {getTokenPromise.Error.Code} - {getTokenPromise.Error.Message}");
			SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Purchase_Error_Unknown_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Purchase_Error_Unknown_Body"));
		}
		WrapperController.EnableLoadingIndicator(enabled: false);
	}
}
