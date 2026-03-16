using UnityEngine.Purchasing;

namespace Core.Shared.Code.Store.Steam;

public class SteamPurchaseInfo : IPurchasedProductInfo
{
	public string productId { get; }

	public SubscriptionInfo subscriptionInfo { get; }

	public SteamPurchaseInfo(string product)
	{
		productId = product;
	}
}
