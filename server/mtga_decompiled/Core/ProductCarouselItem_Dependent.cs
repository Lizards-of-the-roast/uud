using System;
using UnityEngine;
using Wotc.Mtga.Client.Models.Mercantile;

[Serializable]
[CreateAssetMenu(menuName = "ScriptableObject/Carousel/Product_Dependent", fileName = "Product - Dependent - RENAME")]
public class ProductCarouselItem_Dependent : CarouselItemBase
{
	[Space(20f)]
	public string Product;

	[Space(20f)]
	public string ProductThisDependsOn;

	protected override bool OnIsVisibleToPlayer()
	{
		StoreManager store = WrapperController.Instance.Store;
		StoreItem value;
		bool flag = store.StoreListings.TryGetValue(Product, out value) && value.Enabled && value.HasRemainingPurchases;
		StoreItem value2;
		bool flag2 = store.StoreListings.TryGetValue(Product, out value2) && value2.Enabled && value2.HasRemainingPurchases;
		if (flag)
		{
			return !flag2;
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
