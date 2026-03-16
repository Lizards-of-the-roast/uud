using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace Assets.Core.Meta.MainNavigation.Store.Utils;

public static class PurchaseCostUtils
{
	[Serializable]
	public struct PurchaseButton
	{
		public CustomButton Button;

		public TextMeshProUGUI Label;

		public Localize LocalizedLabel;

		public Transform ButtonContainer;
	}

	public static IEnumerable<StoreItemBase.PurchaseButtonSpecification> TransformPurchaseOptions(StoreItem storeItem)
	{
		return storeItem.PurchaseOptions.Select(delegate(Client_PurchaseOption x)
		{
			string text = ((x.CurrencyType == Client_PurchaseCurrencyType.RMT) ? storeItem.LocalizedPrice : x.Price.ToString("N0"));
			bool enabled = true;
			if (x.CurrencyType == Client_PurchaseCurrencyType.Gold)
			{
				enabled = (WrapperController.Instance.InventoryManager.Inventory?.gold ?? 0) >= x.Price;
			}
			else if (x.CurrencyType == Client_PurchaseCurrencyType.CustomToken)
			{
				int value = 0;
				WrapperController.Instance.InventoryManager.Inventory?.CustomTokens.TryGetValue(x.CurrencyId, out value);
				enabled = value >= x.Price;
			}
			return new StoreItemBase.PurchaseButtonSpecification
			{
				CurrencyType = x.CurrencyType,
				Text = text,
				CurrencyId = x.CurrencyId,
				Enabled = enabled
			};
		});
	}

	public static void SetPurchaseButtons(StoreItemBase.PurchaseButtonSpecification spec, PurchaseButton blueButton, PurchaseButton orangeButton, PurchaseButton clearButton = default(PurchaseButton), PurchaseButton greenButton = default(PurchaseButton), int disabledAnimatorHash = 0)
	{
		PurchaseButton? purchaseButton = spec.CurrencyType switch
		{
			Client_PurchaseCurrencyType.Gem => blueButton, 
			Client_PurchaseCurrencyType.Gold => orangeButton, 
			Client_PurchaseCurrencyType.RMT => clearButton, 
			Client_PurchaseCurrencyType.None => greenButton, 
			Client_PurchaseCurrencyType.CustomToken => greenButton, 
			_ => null, 
		};
		if (purchaseButton.HasValue)
		{
			PurchaseButton valueOrDefault = purchaseButton.GetValueOrDefault();
			if (valueOrDefault.Button != null)
			{
				valueOrDefault.Button.gameObject.UpdateActive(active: true);
				valueOrDefault.Label.text = spec.Text;
				if (disabledAnimatorHash != 0)
				{
					valueOrDefault.Button.GetComponent<Animator>().SetBool(disabledAnimatorHash, !spec.Enabled);
				}
				valueOrDefault.Button.Interactable = spec.Enabled;
			}
		}
		else
		{
			Debug.LogError($"Did not expect CurrencyType == {spec.CurrencyType}");
		}
	}
}
