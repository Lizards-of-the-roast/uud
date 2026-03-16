using System;
using System.Collections.Generic;
using UnityEngine;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Wrapper;

[Serializable]
public class SkuDisplay
{
	public string catalogId;

	public string Sku;

	public UDateTime PreorderDate;

	public UDateTime EndDate;

	public string VoucherId;

	public CollationMapping Collation;

	public int UnitCountOverride;

	public bool HidePurchased;

	public MTGALocalizedString Label;

	public MTGALocalizedString FeatureCallout;

	public MTGALocalizedString Tooltip;

	public StoreItemDisplay Display;

	public Color BackgroundColor;

	public Sprite BackgroundSprite;

	public List<StoreCardData> CardViewDefinitions;

	public StoreItemBase customBase;

	public BoosterVoucherView PreorderDisplayPrefab;

	public SkuDisplay(SkuDisplay other)
	{
		catalogId = other.catalogId;
		Sku = other.Sku;
		PreorderDate = other.PreorderDate;
		VoucherId = other.VoucherId;
		Collation = other.Collation;
		UnitCountOverride = other.UnitCountOverride;
		HidePurchased = other.HidePurchased;
		Label = other.Label;
		FeatureCallout = other.FeatureCallout;
		Tooltip = other.Tooltip;
		Display = other.Display;
		BackgroundColor = other.BackgroundColor;
		BackgroundSprite = other.BackgroundSprite;
		CardViewDefinitions = new List<StoreCardData>(other.CardViewDefinitions);
		customBase = other.customBase;
		PreorderDisplayPrefab = other.PreorderDisplayPrefab;
	}
}
