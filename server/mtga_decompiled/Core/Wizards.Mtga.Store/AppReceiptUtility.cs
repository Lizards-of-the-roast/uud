using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

namespace Wizards.Mtga.Store;

public static class AppReceiptUtility
{
	public struct Result
	{
		public bool IsSuccess;

		public string Receipt;
	}

	public static async Task<Result> RefreshReceiptViaUnityIAP(StoreController storeController)
	{
		TaskCompletionSource<Result> tcs = new TaskCompletionSource<Result>();
		storeController.AppleStoreExtendedPurchaseService.RefreshAppReceipt(SuccessCallback, FailureCallback);
		await tcs.Task;
		Debug.Log("New receipt in cache: " + storeController.AppleStoreExtendedPurchaseService.appReceipt);
		return tcs.Task.Result;
		void FailureCallback(string message)
		{
			Debug.Log("[RefreshReceiptViaUnityIAP] Receipt failed to refresh " + message);
			Result result = new Result
			{
				IsSuccess = false,
				Receipt = message
			};
			tcs.TrySetResult(result);
		}
		void SuccessCallback(string message)
		{
			Debug.Log("[RefreshReceiptViaUnityIAP] Receipt refreshed successfully " + message);
			Result result = new Result
			{
				IsSuccess = true,
				Receipt = message
			};
			tcs.TrySetResult(result);
		}
	}

	public static AppleReceipt ParseAppleReceipt(string receipt)
	{
		return new AppleReceiptParser().Parse(Convert.FromBase64String(receipt));
	}
}
