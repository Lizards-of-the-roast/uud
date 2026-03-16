using System;
using Cysharp.Threading.Tasks;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using Wizards.Arena.Client.Logging;
using Wotc.Mtga.Extensions;

namespace Wizards.Mtga.Store;

public class EnsureAppReceiptContainsTransactionUniTask
{
	private const int MaxRetryAttempts = 7;

	private const float SecondsBetweenRetries = 5f;

	private readonly ILogger _logger;

	private readonly StoreController _storeController;

	private readonly string _targetTransactionId;

	public EnsureAppReceiptContainsTransactionUniTask(string targetTransactionId, StoreController storeController, ILogger logger)
	{
		_targetTransactionId = targetTransactionId;
		_storeController = storeController;
		_logger = logger;
	}

	public async UniTask Load()
	{
		_logger.Info("[EnsureAppReceiptIsCurrentFiber] Starting, looking for {transactionId}...");
		string appReceipt = _storeController.AppleStoreExtendedPurchaseService.appReceipt;
		if (IsReceiptValidAndContainsTransaction(appReceipt))
		{
			_logger.Info("[EnsureAppReceiptIsCurrentFiber] Existing receipt already contains " + _targetTransactionId);
			return;
		}
		int currentAttempt = 0;
		while (true)
		{
			currentAttempt++;
			if (currentAttempt > 7)
			{
				throw new TimeoutException("[EnsureAppReceiptIsCurrentFiber] Unable to fetch updated receipt with transactionId: " + _targetTransactionId);
			}
			if (IsReceiptValidAndContainsTransaction((await AppReceiptUtility.RefreshReceiptViaUnityIAP(_storeController)).Receipt))
			{
				break;
			}
			await UniTask.Delay(TimeSpan.FromSeconds(5.0), DelayType.DeltaTime);
		}
	}

	private bool IsReceiptValidAndContainsTransaction(string receipt)
	{
		if (receipt == null)
		{
			return false;
		}
		AppleReceipt appleReceipt = AppReceiptUtility.ParseAppleReceipt(receipt);
		if (appleReceipt == null)
		{
			_logger.Error("[EnsureAppReceiptIsCurrentFiber] Returned null receipt");
		}
		else if (appleReceipt.inAppPurchaseReceipts.Exists((AppleInAppPurchaseReceipt purchaseReceipt) => purchaseReceipt.transactionID == _targetTransactionId))
		{
			_logger.Info("[EnsureAppReceiptIsCurrentFiber] Updated receipt correctly contains " + _targetTransactionId);
			return true;
		}
		return false;
	}
}
