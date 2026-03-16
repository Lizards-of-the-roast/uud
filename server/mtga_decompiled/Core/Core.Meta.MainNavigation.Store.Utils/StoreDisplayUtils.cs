using System;
using System.Collections;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Store;
using Pooling;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.Client.Logging;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Wrapper;

namespace Core.Meta.MainNavigation.Store.Utils;

public class StoreDisplayUtils
{
	private static readonly string[] AlchemyPrefixes = new string[5] { "Y22", "Y23", "Y24", "Y25", "Y26" };

	public static StoreItemDisplay BoosterForSetAndItemCount(string flavor, string setCode, int ItemCount, AssetLookupSystem assetLookupSystem, AssetTracker assetTracker, string trackingKey)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.UnitCount = ItemCount;
		assetLookupSystem.Blackboard.Flavor = flavor;
		assetLookupSystem.Blackboard.SetCode = setCode;
		return StoreItemDisplayFromAlt<PackPayload>(assetLookupSystem, assetTracker, trackingKey);
	}

	public static StoreItemDisplay GemsForUnitCount(int unitCount, AssetLookupSystem assetLookupSystem, AssetTracker assetTracker, string trackingKey)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.UnitCount = unitCount;
		return StoreItemDisplayFromAlt<GemPayload>(assetLookupSystem, assetTracker, trackingKey);
	}

	public static StoreItemDisplay StoreItemDisplayFromAlt<T>(AssetLookupSystem assetLookupSystem, AssetTracker assetTracker, string trackingKey) where T : StorePayload
	{
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<T> loadedTree))
		{
			T payload = loadedTree.GetPayload(assetLookupSystem.Blackboard);
			if (payload != null)
			{
				return AssetLoader.AcquireAndTrackAsset(assetTracker, trackingKey, payload.StoreDataRef);
			}
		}
		return null;
	}

	public static T StorePayloadForFlavor<T>(string flavor, AssetLookupSystem assetLookupSystem) where T : StorePayload
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.Flavor = flavor;
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<T> loadedTree))
		{
			return loadedTree.GetPayload(assetLookupSystem.Blackboard);
		}
		return null;
	}

	public static PetPayload PetPayloadForPetId(string petId, string petVariantId, AssetLookupSystem assetLookupSystem)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.PetId = petId;
		assetLookupSystem.Blackboard.PetVariantId = petVariantId;
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<PetPayload> loadedTree))
		{
			return loadedTree.GetPayload(assetLookupSystem.Blackboard);
		}
		return null;
	}

	public static MTGALocalizedString GetLocStringForPacks(StoreItem item)
	{
		if (item.LocKey != string.Empty)
		{
			return LocForKeyAndCount(item.LocKey, item.PackCount);
		}
		if (!string.IsNullOrEmpty(item.LocData?.LocKey))
		{
			return LocForKeyAndCount(item.LocData.LocKey, item.PackCount);
		}
		if (item.PackCount == 1)
		{
			return "MainNav/Store/BoosterCount/Gold-Packs-001";
		}
		return LocForKeyAndCount("MainNav/Store/BoosterCount/PacksCount", item.PackCount);
	}

	private static MTGALocalizedString LocForKeyAndCount(string locKey, int unitCount)
	{
		MTGALocalizedString mTGALocalizedString = locKey;
		mTGALocalizedString.Parameters = new Dictionary<string, string> { 
		{
			"count",
			unitCount.ToString()
		} };
		return mTGALocalizedString;
	}

	public static List<CollationMapping> CollationMappingsForItem(StoreItem item)
	{
		IEnumerable<string> enumerable = ReferenceIdsForItem(item);
		List<CollationMapping> list = new List<CollationMapping>();
		foreach (string item2 in enumerable)
		{
			CollationMapping collationMapping = CollationMappingUtils.FromString(item2);
			if (collationMapping != CollationMapping.None)
			{
				list.Add(collationMapping);
			}
		}
		return list;
	}

	private static IEnumerable<string> ReferenceIdsForItem(StoreItem item)
	{
		foreach (Sku sku in item.Skus)
		{
			foreach (string item2 in ReferenceIdsForTreasureItem(sku.TreasureItem))
			{
				yield return item2;
			}
		}
	}

	private static IEnumerable<string> ReferenceIdsForTreasureItem(TreasureItem treasureItem)
	{
		for (int i = 0; i < treasureItem.Quantity; i++)
		{
			yield return treasureItem.ReferenceId;
		}
		if (treasureItem.Contents == null)
		{
			yield break;
		}
		foreach (TreasureItem content in treasureItem.Contents)
		{
			foreach (string item in ReferenceIdsForTreasureItem(content))
			{
				yield return item;
			}
		}
	}

	public static void RefreshPooledPack(StoreItem item, StoreItemBase itemBase, Transform container, AssetLookupSystem assetLookupSystem, Wizards.Arena.Client.Logging.Logger logger)
	{
		List<CollationMapping> list = CollationMappingsForItem(item);
		itemBase.transform.SetParent(container, worldPositionStays: false);
		if (list.Count > 0)
		{
			itemBase.gameObject.UpdateActive(active: true);
			itemBase.SetPurchaseButtons(item, assetLookupSystem);
			RefreshStoreItemDisplay(itemBase.ItemDisplay, item, assetLookupSystem, logger, list);
			if ((bool)itemBase.ItemConfirmationDisplay)
			{
				RefreshStoreItemDisplay(itemBase.ItemConfirmationDisplay, item, assetLookupSystem, logger, list);
			}
			itemBase.SetLimitText(null);
			itemBase.SetItem(item);
		}
	}

	private static void RefreshStoreItemDisplay(StoreItemDisplay itemDisplay, StoreItem item, AssetLookupSystem assetLookupSystem, Wizards.Arena.Client.Logging.Logger logger, List<CollationMapping> collationIds)
	{
		itemDisplay.SetCollationIds(collationIds);
		itemDisplay.RefreshBoosterMaterials();
		if (itemDisplay is StoreDisplayCardViewBundle storeDisplayCardViewBundle)
		{
			storeDisplayCardViewBundle.SetCardViews(item.BundleData?.CardViewDefinitions);
		}
		if (itemDisplay is StoreDisplayPhraseBundle storeDisplayPhraseBundle)
		{
			storeDisplayPhraseBundle.SetEmotes(item.Skus, assetLookupSystem, logger);
		}
	}

	public static void DespawnPackWidget(IUnityObjectPool StoreObjectPool, StoreItemBase itemWidget, List<StoreItemBase> spawnedItems = null)
	{
		itemWidget.SetDailyDealOverride();
		itemWidget.SetFeatureCalloutText(null);
		itemWidget.HideBrowseButton();
		StoreItemDisplay itemDisplay = itemWidget.ItemDisplay;
		itemWidget.DetachItemDisplay();
		StoreObjectPool.PushObject(itemWidget.gameObject);
		StoreObjectPool.PushObject(itemDisplay.gameObject);
		spawnedItems?.Remove(itemWidget);
	}

	public static void UpdateBackgroundSprite(AltAssetReference<Sprite> spriteDataRef, StoreItemDisplay itemDisplay)
	{
		if (spriteDataRef != null)
		{
			itemDisplay.SetBackgroundSprite(spriteDataRef.RelativePath);
		}
	}

	public static void UpdateCardViews(StoreItemDisplay itemDisplay, StoreItem item)
	{
		if (item.BundleData != null && itemDisplay is StoreDisplayCardViewBundle storeDisplayCardViewBundle)
		{
			storeDisplayCardViewBundle.SetCardViews(item.BundleData.CardViewDefinitions, item);
		}
	}

	public static void SetBackgroundColor(IBackgroundColorPayload backgroundColorPayload, StoreItemBase newButton)
	{
		if (backgroundColorPayload != null)
		{
			SetBackgroundColor(backgroundColorPayload.BackgroundColor, newButton);
		}
	}

	private static void SetBackgroundColor(Color backgroundColor, StoreItemBase newButton)
	{
		if (backgroundColor != default(Color))
		{
			newButton.SetBackgroundColor(backgroundColor);
		}
	}

	public static void UpdateItemLoc(StoreItem item, StoreItemBase itemDisplay, AssetLookupSystem assetLookupSystem, Wizards.Arena.Client.Logging.Logger logger)
	{
		string text = ((item.LocData?.FeatureCalloutLocKey != null) ? item.LocData.FeatureCalloutLocKey : null);
		itemDisplay.SetFeatureCalloutText(text);
		itemDisplay.SetHeaderText(null);
		if (item.LocData != null)
		{
			itemDisplay.SetLabelText(item.LocData.LocKey);
			itemDisplay.SetTooltipText(item.LocData.TooltipLocKey);
		}
		else
		{
			itemDisplay.SetLabelText(null);
		}
		if (itemDisplay.ItemDisplay is StoreDisplayPhraseBundle storeDisplayPhraseBundle)
		{
			storeDisplayPhraseBundle.SetEmotes(item.Skus, assetLookupSystem, logger);
		}
		if ((bool)itemDisplay.ItemConfirmationDisplay && itemDisplay.ItemConfirmationDisplay is StoreDisplayPhraseBundle storeDisplayPhraseBundle2)
		{
			storeDisplayPhraseBundle2.SetEmotes(item.Skus, assetLookupSystem, logger);
		}
	}

	public static CollationMapping CollationMappingFromStoreItemSubType(string itemSubType)
	{
		return CollationMappingFromStoreItemSubType(itemSubType, AlchemyPrefixes);
	}

	private static CollationMapping CollationMappingFromStoreItemSubType(string itemSubType, IEnumerable<string> prefixes)
	{
		if (AlchemyPrefixes.Exists((string x) => itemSubType.StartsWith(x)))
		{
			string text = itemSubType.Substring(0, 3);
			string text2 = itemSubType.Substring(3);
			itemSubType = text + "_" + text2;
		}
		return CollationMappingUtils.FromString(itemSubType);
	}

	public static void UpdatePurchaseButton(StoreItem item, StoreItemBase itemBase, AssetLookupSystem assetLookupSystem)
	{
		if (!item.HasRemainingPurchases)
		{
			itemBase.SetHeaderText("MainNav/Store/Purchased");
			itemBase.SetPurchaseButtons(item, assetLookupSystem);
		}
	}

	public static void UpdatePreOrderItem(StoreItem item, StoreItemBase itemBase, AssetLookupSystem assetLookupSystem)
	{
		if (!item.HasRemainingPurchases)
		{
			UpdatePurchaseButton(item, itemBase, assetLookupSystem);
			itemBase.SetFeatureCalloutText(StoreManager.GetPreorderAvailableString(item.PreorderData.EndTime, isPurchased: true));
		}
		else
		{
			itemBase.SetHeaderText("MainNav/Store/PreorderTitle");
			itemBase.SetFeatureCalloutText(StoreManager.GetPreorderAvailableString(item.PreorderData.EndTime));
			itemBase.SetLabelText(item.LocData.LocKey);
		}
		itemBase.SetTooltipText(item.LocData.TooltipLocKey);
	}

	public static bool IsPreorder(StoreItem storeItem)
	{
		if (storeItem.PreorderData != null)
		{
			return storeItem.PreorderData.StartTime != DateTime.MinValue;
		}
		return false;
	}

	public static IEnumerator Coroutine_HighlightItem(StoreItemBase highlightItem, HorizontalLayoutGroup storeButtonLayoutGroup, string highlightItemId)
	{
		highlightItem.Highlight();
		yield return null;
		RectTransform viewport = (RectTransform)storeButtonLayoutGroup.transform.parent.parent;
		RectTransform layoutRect = (RectTransform)storeButtonLayoutGroup.transform;
		viewport.ForceUpdateRectTransforms();
		layoutRect.ForceUpdateRectTransforms();
		yield return null;
		float width = viewport.rect.width;
		float x = highlightItem.transform.localPosition.x;
		Vector3 vector = layoutRect.anchoredPosition;
		vector.x = 0f - x + width * 0.5f;
		layoutRect.anchoredPosition = vector;
		highlightItemId = null;
	}
}
