using System;
using Assets.Core.Meta.MainNavigation.Store.Utils;
using UnityEngine;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Extensions;

public class PrizeWallUnlocker : MonoBehaviour
{
	[Header("Purchase Options")]
	[SerializeField]
	private PurchaseCostUtils.PurchaseButton BlueButton;

	[SerializeField]
	private PurchaseCostUtils.PurchaseButton OrangeButton;

	[SerializeField]
	private GameObject _unlockWidget;

	private StoreItem _prizeWallUnlockStoreItem;

	private event Action<StoreItem, Client_PurchaseCurrencyType> _purchaseOptionClicked;

	public event Action<StoreItem, Client_PurchaseCurrencyType> PurchaseOptionClicked
	{
		add
		{
			this._purchaseOptionClicked = null;
			_purchaseOptionClicked += value;
		}
		remove
		{
			_purchaseOptionClicked -= value;
		}
	}

	private void Awake()
	{
		if (BlueButton.Button != null)
		{
			BlueButton.Button.OnClick.AddListener(delegate
			{
				OnButtonClicked(Client_PurchaseCurrencyType.Gem);
			});
		}
		if (OrangeButton.Button != null)
		{
			OrangeButton.Button.OnClick.AddListener(delegate
			{
				OnButtonClicked(Client_PurchaseCurrencyType.Gold);
			});
		}
	}

	private void OnButtonClicked(Client_PurchaseCurrencyType currencyType)
	{
		this._purchaseOptionClicked?.Invoke(_prizeWallUnlockStoreItem, currencyType);
	}

	public void SetPrizeWallToUnlock(StoreItem item)
	{
		_prizeWallUnlockStoreItem = item;
		foreach (StoreItemBase.PurchaseButtonSpecification item2 in PurchaseCostUtils.TransformPurchaseOptions(_prizeWallUnlockStoreItem))
		{
			PurchaseCostUtils.SetPurchaseButtons(item2, BlueButton, OrangeButton);
		}
	}

	public void UpdateActiveUnlockWidget(bool active)
	{
		_unlockWidget.UpdateActive(active);
	}
}
