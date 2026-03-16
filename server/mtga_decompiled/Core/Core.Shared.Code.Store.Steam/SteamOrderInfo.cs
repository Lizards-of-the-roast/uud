using System;
using System.Collections.Generic;
using UnityEngine.Purchasing;

namespace Core.Shared.Code.Store.Steam;

public class SteamOrderInfo : IOrderInfo
{
	public IAppleOrderInfo Apple
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public IGoogleOrderInfo Google
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public List<IPurchasedProductInfo> PurchasedProductInfo { get; set; }

	public string Receipt { get; }

	public string TransactionID { get; }

	public SteamOrderInfo(string receipt, string transactionId, string product)
	{
		Receipt = receipt;
		TransactionID = transactionId;
		PurchasedProductInfo = new List<IPurchasedProductInfo>
		{
			new SteamPurchaseInfo(product)
		};
	}
}
