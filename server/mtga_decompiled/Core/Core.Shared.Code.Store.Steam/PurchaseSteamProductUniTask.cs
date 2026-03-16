using System;
using System.Threading.Tasks;
using Core.BI;
using Cysharp.Threading.Tasks;
using Steamworks;
using UnityEngine;
using UnityEngine.Purchasing;
using WAS;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using _3rdParty.Steam;

namespace Core.Shared.Code.Store.Steam;

public class PurchaseSteamProductUniTask
{
	private struct StructuredReceipt
	{
		public string Store;

		public string Payload;
	}

	private readonly IAccountClient _account;

	private readonly Product _product;

	private readonly IStorePurchaseCallback _purchaseCallback;

	private SteamReceipt _receipt;

	private Error _error;

	private string _language;

	private bool _gotSteamResponse;

	public PurchaseSteamProductUniTask(IStorePurchaseCallback purchaseCallback, IAccountClient account, Product product)
	{
		_purchaseCallback = purchaseCallback;
		_account = account;
		_product = product;
	}

	public async UniTaskVoid Load()
	{
		Task<AuthTicket> authTask = SteamUser.GetAuthSessionTicketAsync(SteamClient.SteamId);
		await authTask;
		string steamSessionTicket = _3rdParty.Steam.Steam.EncodeAuthTicket(authTask.Result);
		_language = _3rdParty.Steam.Steam.GetIsoLanguageCode();
		Enum.TryParse<TransactionType>(Pantry.CurrentEnvironment.xsollaTransactionType, out var result);
		SteamUser.OnMicroTxnAuthorizationResponse += OnMicroTxnAuthorizationResponse;
		PromiseExtensions.Logger.Info("[SteamPurchase] Initiating Steam purchase via Platform");
		await _account.InitSteamPurchase(steamSessionTicket, _language, SteamPurchaseController.Currency, _product.definition.storeSpecificId, result == TransactionType.Sandbox).IfSuccess(delegate(Promise<SteamReceipt> p)
		{
			_receipt = p.Result;
		}).IfError(delegate(Promise<SteamReceipt> p)
		{
			_error = p.Error;
		})
			.AsTask;
		if (_receipt != null && !ulong.TryParse(_receipt.orderid, out var _))
		{
			_error = new Error(-1, "Error deserializing receipt");
		}
		if (!_error.IsError)
		{
			PromiseExtensions.Logger.Info("[SteamPurchase] Got receipt from platform, waiting for Steam");
			await UniTask.WaitUntil(() => _gotSteamResponse);
		}
		else
		{
			_purchaseCallback?.OnPurchaseFailed(GetFailedOrder(PurchaseFailureReason.Unknown, _error.ToString()));
		}
		SteamUser.OnMicroTxnAuthorizationResponse -= OnMicroTxnAuthorizationResponse;
	}

	private void OnMicroTxnAuthorizationResponse(AppId appId, ulong orderId, bool authorized)
	{
		ulong.TryParse(_receipt.orderid, out var result);
		if (result != orderId)
		{
			SimpleLog.LogError($"[SteamPurchase] Received unexpected order id {orderId} from steam! Expected {result}");
			_purchaseCallback?.OnPurchaseFailed(GetFailedOrder(PurchaseFailureReason.Unknown, "Unexpected orderId"));
		}
		else if (!authorized)
		{
			_purchaseCallback?.OnPurchaseFailed(GetFailedOrder(PurchaseFailureReason.UserCancelled, "Purchase not authorized"));
			BIEventType.SteamPurchaseNotAuthorized.SendWithDefaults(("Sku", _product.definition.storeSpecificId), ("ExternalId", _product.definition.id), ("OrderNumber", orderId.ToString()), ("SteamCurrency", SteamPurchaseController.Currency), ("SteamLanguage", _language), ("LocalizedTitle", _product.metadata.localizedTitle), ("SteamPrice", _product.metadata.localizedPriceString));
		}
		else
		{
			StructuredReceipt obj = new StructuredReceipt
			{
				Store = "Steam",
				Payload = JsonUtility.ToJson(_receipt)
			};
			Cart cart = new Cart(_product);
			SteamOrderInfo info = new SteamOrderInfo(JsonUtility.ToJson(obj), orderId.ToString(), _product.definition.storeSpecificId);
			PendingOrder order = new PendingOrder(cart, info);
			_purchaseCallback?.OnPurchaseSucceeded(order);
		}
		_gotSteamResponse = true;
	}

	private FailedOrder GetFailedOrder(PurchaseFailureReason failureReason, string details)
	{
		return new FailedOrder(new Cart(_product), failureReason, details);
	}
}
