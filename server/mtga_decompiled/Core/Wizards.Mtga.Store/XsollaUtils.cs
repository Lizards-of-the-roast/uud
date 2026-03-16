using System;
using Cysharp.Threading.Tasks;
using WAS;
using Wizards.Arena.Promises;

namespace Wizards.Mtga.Store;

public static class XsollaUtils
{
	public static async UniTask TryGetStoreItems(IAccountClient account, Action<ItemsResponse> onComplete, Action<AccountError> onError)
	{
		await account.GetStoreItems(account.GetStoreCurrencySelection()).IfSuccess(delegate(Promise<ItemsResponse> p)
		{
			onComplete(p.Result);
		}).IfError(delegate(Error e)
		{
			onError(WASUtils.ToAccountError(e));
		})
			.AsTask;
	}

	public static Promise<PurchaseTokenResponse> GetPurchaseToken(string sku, IAccountClient account, TransactionType transactionType)
	{
		return account.GetPurchaseToken(account.GetStoreCurrencySelection(), sku, transactionType);
	}
}
