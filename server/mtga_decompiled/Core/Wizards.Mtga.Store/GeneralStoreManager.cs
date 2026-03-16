using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Code.Promises;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using WAS;
using Wizards.Arena.Client.Logging;
using Wizards.Arena.Enums.System;
using Wizards.Arena.Promises;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga.BI;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Loc;

namespace Wizards.Mtga.Store;

public class GeneralStoreManager : StoreManager, IDisposable
{
	private IPurchaseController _controller;

	private readonly string _personaId;

	public GeneralStoreManager(IPurchaseController purchaser, IAccountClient accountClient, Wizards.Arena.Client.Logging.ILogger logger, IBILogger biLogger)
		: base(accountClient, logger, biLogger)
	{
		_controller = purchaser;
		_personaId = accountClient.AccountInformation?.PersonaID;
	}

	public void TestReceiptFile(string filename)
	{
		if (File.Exists(Path.Combine(Application.persistentDataPath, filename)))
		{
			string receipt = File.ReadAllText(Path.Combine(Application.persistentDataPath, filename));
			string[] array = filename.Split('!');
			string text = array[1];
			string itemId = array[2];
			_ = array[3];
			if (text != _personaId)
			{
				_logger.Warn("TestReceiptFile: Missmatch personaIds " + text + " == " + _personaId);
			}
			ValidateIAPReceipt(itemId, receipt).IfSuccess(delegate(Promise<(string, string)> p)
			{
				PromiseExtensions.Logger.Debug("TestValidate " + itemId + " Success - " + p.Result.Item1 + " " + p.Result.Item2);
				GetEntitlements();
			}).IfError(delegate(Error e)
			{
				PromiseExtensions.Logger.Debug($"TestValidate {itemId} Fail - ErrorCode:{e.Code} ErrorMessage:{e.Message}");
			});
		}
	}

	private void SaveReceiptFile(string itemId, string transactionID, string receipt)
	{
		File.WriteAllText(Path.Combine(Application.persistentDataPath, "receipt!" + _personaId + "!" + itemId + "!" + transactionID + ".txt"), receipt);
	}

	public void RestoreTransactions()
	{
		_controller.RestoreTransactions(OnTransactionsRestored);
	}

	private void OnTransactionsRestored(bool success, string internalMessage)
	{
		ShowRestoreTransactionsMessage(success, internalMessage);
		ProcessPending();
	}

	public void ShowRestoreTransactionsMessage(bool success, string internalMessage)
	{
		if (!success)
		{
			_logger.Error("RestoreTransactions returned failure message: " + internalMessage);
		}
		string localizedText = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Settings/RestorePurchasesTitle");
		string localizedText2 = Languages.ActiveLocProvider.GetLocalizedText(success ? "MainNav/Settings/RestorePurchasesSuccess" : "MainNav/Settings/RestorePurchasesFail");
		List<SystemMessageManager.SystemMessageButtonData> list = new List<SystemMessageManager.SystemMessageButtonData>();
		SystemMessageManager.SystemMessageButtonData item = new SystemMessageManager.SystemMessageButtonData
		{
			Text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Popups/Modal/ModalOptions_OK")
		};
		list.Add(item);
		SystemMessageManager.Instance.ShowMessage(localizedText, localizedText2, list);
	}

	public override IEnumerator PurchaseRMTItemYield(StoreItem item)
	{
		Client_PurchaseOption po = item.PurchaseOptions.FirstOrDefault((Client_PurchaseOption p) => p.CurrencyType == Client_PurchaseCurrencyType.RMT);
		PurchaseRequest request = _controller.RequestPurchase(po?.CurrencyId);
		yield return request;
		if (!request.Successful)
		{
			if (request.IsError)
			{
				BI_SendPurchaseFunnelError("Error PurchaseRmtItem: " + request.MessageKey + " - " + request.Receipt);
				_logger.Error("Error PurchaseRmtItem: " + request.MessageKey + " - " + request.Receipt);
				SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Store_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText(request.MessageKey));
			}
			else
			{
				_logger.Error("Unsuccessful PurchaseRmtItem: " + request.MessageKey + " - " + request.Receipt);
			}
		}
		else if (StoreManager.ForceCrashAfterPurchase)
		{
			StoreUtils.ForcedCrash(delegate(string m)
			{
				_logger.Debug(m);
			});
		}
		else if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			yield return ValidateRequestOnIOS(po.CurrencyId, request);
		}
		else
		{
			ValidateRequest(po.CurrencyId, request);
		}
	}

	private IEnumerator ValidateRequestOnIOS(string externalId, PurchaseRequest request)
	{
		SceneLoader sceneLoader = SceneLoader.GetSceneLoader();
		EnsureAppReceiptContainsTransactionUniTask ensureAppReceiptContainsTransactionUniTask = new EnsureAppReceiptContainsTransactionUniTask(((IAPPurchaseRequest)request).Order.Info.Apple?.OriginalTransactionID, UnityIAPServices.StoreController(), _logger);
		sceneLoader.EnableLoadingIndicator(shouldEnable: true);
		Exception error = null;
		yield return ensureAppReceiptContainsTransactionUniTask.Load().ToCoroutine(delegate(Exception ex)
		{
			error = ex;
		});
		if (error != null)
		{
			_logger.Error($"Refresh receipt failed to complete | {error}");
			SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Store_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Error_Text"));
		}
		else
		{
			ValidateRequest(externalId, request);
		}
		sceneLoader.EnableLoadingIndicator(shouldEnable: false);
	}

	private void ValidateRequest(string externalId, PurchaseRequest request)
	{
		_biLogger.Send(ClientBusinessEventType.PurchaseFunnel, PurchaseFunnel.Create(ClientPurchaseFunnelContext.PlayerBeginsPurchase));
		_biLogger.Send(ClientBusinessEventType.PurchaseFunnel, PurchaseFunnel.CreateCurrencyInfo(toggled: false, request.IsoCurrencyCode));
		_logger.Debug("ValidateRequest for " + externalId + " -- " + request.Receipt);
		ValidateIAPReceipt(externalId, request.Receipt).ThenOnMainThreadIfSuccess((Action<(string, string)>)delegate
		{
			if (StoreManager.ForceCrashBeforeEntitlements)
			{
				StoreUtils.ForcedCrash(delegate(string m)
				{
					_logger.Debug(m);
				});
			}
			else
			{
				_controller.ConfirmPurchase(request);
				BIEventTracker.TrackPurchaseEvent(request.ProductId);
				GetEntitlements();
			}
		}).IfError(delegate(Promise<(string, string)> p)
		{
			BI_SendErrorLogs(p, externalId);
			MainThreadDispatcher.Dispatch(delegate
			{
				DisplayPromiseErrorDialog(ServerErrors.InternalError, Client_PurchaseCurrencyType.RMT);
			});
		});
	}

	private Promise<(string, string)> ValidateIAPReceipt(string itemId, string receipt)
	{
		JObject jObject = JObject.Parse(receipt);
		string text = jObject["Store"].Value<string>();
		string text2 = jObject["Payload"].Value<string>();
		Promise<ValidateReceiptResponse> promise = null;
		switch (text)
		{
		case "GooglePlay":
		{
			string receipt2 = JObject.Parse(JObject.Parse(text2)["json"].Value<string>())["purchaseToken"].Value<string>();
			promise = _accountClient.TryValidateReceipt(itemId, AppStore.GooglePlay, Application.identifier, receipt2);
			break;
		}
		case "AppleAppStore":
			promise = _accountClient.TryValidateReceipt(itemId, AppStore.AppleAppStore, Application.identifier, text2);
			break;
		case "Steam":
			promise = _accountClient.TryValidateReceipt(itemId, AppStore.SteamStore, Application.identifier, text2);
			break;
		default:
			return new SimplePromise<(string, string)>(new Error(-1, "Bad Receipt:\n" + receipt));
		}
		return promise.Convert((ValidateReceiptResponse r) => (r.entitlementIDs.Length != 0) ? (itemId: itemId, r.entitlementIDs[0]) : (itemId: null, null));
	}

	public override IEnumerator ProcessRMTListingsYield()
	{
		_logger.Debug("GeneralStore Initializing");
		CatalogRequest listRequest = _controller.RequestCatalog(GetRmtSKUsFromMercantileList(MercantileCollections.StoreListings.Values));
		yield return listRequest;
		if (!listRequest.Successful)
		{
			BI_SendPurchaseFunnelError("RequestCatalog Error: " + listRequest.MessageKey);
			_logger.Error("RequestCatalog Error: " + listRequest.MessageKey);
		}
		else
		{
			ValidateRMTItems(listRequest.rmtProductInfos);
			yield return ProcessPending();
		}
	}

	public IEnumerator ProcessPending()
	{
		List<PurchaseRequest> list = new List<PurchaseRequest>(_controller?.PendingPurchases ?? Array.Empty<PurchaseRequest>());
		_logger.Debug($"Process pending purchases {list.Count} @ {Time.time}");
		foreach (PurchaseRequest item in list)
		{
			if (TryLookupExternalIdFromProductId(item, out var externalId))
			{
				if (Application.platform == RuntimePlatform.IPhonePlayer)
				{
					yield return ValidateRequestOnIOS(externalId, item);
				}
				else
				{
					ValidateRequest(externalId, item);
				}
			}
		}
	}

	private bool TryLookupExternalIdFromProductId(PurchaseRequest request, out string externalId)
	{
		foreach (StoreItem value in MercantileCollections.StoreListings.Values)
		{
			Client_PurchaseOption client_PurchaseOption = value.PurchaseOptions.FirstOrDefault((Client_PurchaseOption po) => po.CurrencyType == Client_PurchaseCurrencyType.RMT);
			if (client_PurchaseOption != null && client_PurchaseOption.CurrencyId == request.ProductId)
			{
				externalId = client_PurchaseOption.CurrencyId;
				return true;
			}
		}
		externalId = string.Empty;
		return false;
	}

	private static IEnumerable<string> GetRmtSKUsFromMercantileList(IEnumerable<StoreItem> mercantileItems)
	{
		foreach (StoreItem mercantileItem in mercantileItems)
		{
			Client_PurchaseOption client_PurchaseOption = mercantileItem.PurchaseOptions.FirstOrDefault((Client_PurchaseOption po) => po.CurrencyType == Client_PurchaseCurrencyType.RMT);
			if (!string.IsNullOrWhiteSpace(client_PurchaseOption?.CurrencyId))
			{
				yield return client_PurchaseOption.CurrencyId;
			}
		}
	}

	public void Dispose()
	{
		_controller.Dispose();
		_controller = null;
	}
}
