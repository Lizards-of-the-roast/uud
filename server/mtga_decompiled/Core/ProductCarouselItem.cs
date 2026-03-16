using System;
using UnityEngine;
using Wotc.Mtga.Client.Models.Mercantile;

[Serializable]
[CreateAssetMenu(menuName = "ScriptableObject/Carousel/Product", fileName = "ProductCarouselItem")]
public class ProductCarouselItem : CarouselItemBase
{
	[Space(20f)]
	public string Product;

	protected override bool OnIsVisibleToPlayer()
	{
		if (WrapperController.Instance.Store.StoreListings.TryGetValue(Product, out var value) && value.Enabled && value.HasRemainingPurchases)
		{
			return true;
		}
		return false;
	}

	public override void OnClick()
	{
		StoreManager store = WrapperController.Instance.Store;
		if (store.StoreListings.TryGetValue(Product, out var value))
		{
			PAPA.StartGlobalCoroutine(store.PurchaseItemYield(value, Client_PurchaseCurrencyType.RMT));
		}
	}
}
