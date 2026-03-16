using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Store;
using Core.Code.PrizeWall;
using Core.Meta.MainNavigation.Cosmetics;
using Core.Meta.Shared;
using Core.Shared.Code.Utilities;
using GreClient.CardData;
using Pooling;
using SharedClientCore.SharedClientCore.Code.Providers;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.Client.Logging;
using Wizards.Arena.Enums.Card;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Wrapper;

namespace Core.Meta.MainNavigation.Store.Utils;

public static class StoreWidgetUtils
{
	public static StoreItemBase CreatePetWidget(StoreItem item, Action<StoreItem, Client_PurchaseCurrencyType> executePurchase, Action<StoreItem, Client_PurchaseCurrencyType, Action<StoreItem, Client_PurchaseCurrencyType>> onStoreItemPurchaseOptionClicked, AssetLookupSystem assetLookupSystem, StoreItemBase storeItemBasePrefab, StoreDisplayPet petItemModel, IClientLocProvider clientLocProvider)
	{
		WrapperController.Instance.Store.PetCatalog.TryGetValue(item.PrefabIdentifier, out var value);
		StoreItemBase storeItemBase = null;
		if (value == null)
		{
			return null;
		}
		storeItemBase = UnityEngine.Object.Instantiate(storeItemBasePrefab);
		storeItemBase.SetPurchaseButtons(item, assetLookupSystem);
		storeItemBase.SetItem(item);
		string text = PetUtils.KeyForPetDetails(value, clientLocProvider);
		storeItemBase.SetLabelText(text);
		StoreDisplayPet storeDisplayPet = UnityEngine.Object.Instantiate(petItemModel);
		storeDisplayPet.CreatePet(value.Name, value.Variant, assetLookupSystem);
		storeItemBase.PurchaseOptionClicked += delegate(StoreItem i, Client_PurchaseCurrencyType c)
		{
			onStoreItemPurchaseOptionClicked(i, c, executePurchase);
		};
		PetPayload petPayload = StoreDisplayUtils.PetPayloadForPetId(value.Name, value.Variant, assetLookupSystem);
		StoreDisplayUtils.SetBackgroundColor(petPayload, storeItemBase);
		StoreDisplayUtils.UpdateBackgroundSprite(petPayload?.SpriteDataRef, storeDisplayPet);
		storeItemBase.AttachItemDisplay(storeDisplayPet);
		return storeItemBase;
	}

	public static StoreItemBase CreateBoosterPackWidget(StoreItem item, Action<StoreItem, Client_PurchaseCurrencyType> executePurchase, Action<StoreItem, Client_PurchaseCurrencyType, Action<StoreItem, Client_PurchaseCurrencyType>> onStoreItemPurchaseOptionClicked, AssetLookupSystem assetLookupSystem, AssetTracker assetTracker, IUnityObjectPool storeObjectPool, StoreItemBase storeItemBaseWidePrefab, StoreItemBase storeItemBasePrefab, ISetMetadataProvider setMetadataProvider, List<StoreItemBase> spawnedPackStoreItems)
	{
		MTGALocalizedString locStringForPacks = StoreDisplayUtils.GetLocStringForPacks(item);
		CollationMapping itemCollationMapping = StoreDisplayUtils.CollationMappingFromStoreItemSubType(item.SubType);
		string text = setMetadataProvider.FlavorForCollation(itemCollationMapping);
		StoreItemDisplay storeItemDisplay = StoreDisplayUtils.BoosterForSetAndItemCount(text, item.SubType, item.PackCount, assetLookupSystem, assetTracker, "BoosterPackWidget - " + item.Id);
		SimpleLogUtils.LogWarningIfNull(storeItemDisplay, $"Prefab not found for flavor:{text}, subtype:{item.SubType}, pack count:{item.PackCount}, id:{item.Id}");
		StoreItemDisplay component = storeObjectPool.PopObject(storeItemDisplay.gameObject).GetComponent<StoreItemDisplay>();
		component.transform.localRotation = Quaternion.identity;
		component.transform.localScale = Vector3.one;
		component.transform.localPosition = Vector3.zero;
		StoreItemBase component2 = storeObjectPool.PopObject(component.WideBase ? storeItemBaseWidePrefab.gameObject : storeItemBasePrefab.gameObject).GetComponent<StoreItemBase>();
		component2.transform.localScale = Vector3.one;
		spawnedPackStoreItems.Add(component2);
		component2.AttachItemDisplay(component);
		component2.SetLabelText(locStringForPacks);
		component2.SetRolloverSound(WwiseEvents.sfx_ui_main_rewards_pack_rollover);
		component2.PurchaseOptionClicked += FormatWarning;
		return component2;
		void FormatWarning(StoreItem i, Client_PurchaseCurrencyType c)
		{
			SetAvailability availability = (setMetadataProvider.CollationForMapping(itemCollationMapping) ?? throw new Exception($"Set Data not found for mapping {itemCollationMapping}. Is the set metadata correctly set and capitalized?")).Set.Availability;
			WrapperController.Instance.InventoryManager.HandlePurchaseAvailabilityWarnings(RotationWarningContext.Booster, availability, hasCardsBannedInDeckFormat: false, Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Popups/SetRotation/Warning_Purchase"), delegate
			{
				onStoreItemPurchaseOptionClicked(i, c, executePurchase);
			}, delegate
			{
			});
		}
	}

	private static (StoreItemDisplay storeDisplay, StoreItemDisplay confirmDisplay) PrepStoreItemDisplay(StoreItem item, BundlePayload bundlePayload, AssetTracker assetTracker)
	{
		StoreItemDisplay storeItemDisplay = UnityEngine.Object.Instantiate(AssetLoader.AcquireAndTrackAsset(assetTracker, "BundleWidget - " + item.Id, bundlePayload.StoreDataRef));
		StoreDisplayUtils.UpdateCardViews(storeItemDisplay, item);
		StoreDisplayUtils.UpdateBackgroundSprite(bundlePayload.SpriteDataRef, storeItemDisplay);
		List<int> collationIds = (from x in StoreDisplayUtils.CollationMappingsForItem(item)
			select (int)x).ToList();
		storeItemDisplay.SetCollationIds(collationIds);
		StoreItemDisplay storeItemDisplay2 = null;
		if (!string.IsNullOrEmpty(bundlePayload.StoreConfirmDataRef?.RelativePath))
		{
			storeItemDisplay2 = UnityEngine.Object.Instantiate(AssetLoader.AcquireAndTrackAsset(assetTracker, "BundleConfirmWidget - " + item.Id, bundlePayload.StoreConfirmDataRef));
			StoreDisplayUtils.UpdateCardViews(storeItemDisplay2, item);
			StoreDisplayUtils.UpdateCardViews(storeItemDisplay2, item);
			StoreDisplayUtils.UpdateBackgroundSprite(bundlePayload.SpriteDataRef, storeItemDisplay2);
			storeItemDisplay2.SetCollationIds(collationIds);
			storeItemDisplay2.gameObject.SetActive(value: false);
		}
		return (storeDisplay: storeItemDisplay, confirmDisplay: storeItemDisplay2);
	}

	public static StoreItemBase CreateBundleWidget(StoreItem item, Action<StoreItem, Client_PurchaseCurrencyType> executePurchase, Action<StoreItem, Client_PurchaseCurrencyType, Action<StoreItem, Client_PurchaseCurrencyType>> onStoreItemPurchaseOptionClicked, AssetLookupSystem assetLookupSystem, AssetTracker assetTracker, StoreItemBase storeItemBaseWidePrefab, StoreItemBase storeItemBasePrefab, HorizontalLayoutGroup storeButtonLayoutGroup, Wizards.Arena.Client.Logging.Logger logger)
	{
		BundlePayload bundlePayload = StoreDisplayUtils.StorePayloadForFlavor<BundlePayload>(item.PrefabIdentifier, assetLookupSystem);
		if (bundlePayload == null)
		{
			logger.Error("Could not find payload for bundle '" + item.PrefabIdentifier + "'");
			return null;
		}
		(StoreItemDisplay, StoreItemDisplay) tuple = PrepStoreItemDisplay(item, bundlePayload, assetTracker);
		StoreItemDisplay item2 = tuple.Item1;
		StoreItemBase storeItemBase = UnityEngine.Object.Instantiate(item2.WideBase ? storeItemBaseWidePrefab : storeItemBasePrefab);
		storeItemBase.SetPurchaseButtons(item, assetLookupSystem);
		storeItemBase.SetItem(item);
		storeItemBase.AttachItemDisplay(item2);
		StoreDisplayUtils.SetBackgroundColor(bundlePayload, storeItemBase);
		if ((bool)tuple.Item2)
		{
			storeItemBase.AttachConfirmationItemDisplay(tuple.Item2);
		}
		storeItemBase.PurchaseOptionClicked += delegate(StoreItem i, Client_PurchaseCurrencyType c)
		{
			onStoreItemPurchaseOptionClicked(i, c, executePurchase);
		};
		if (StoreDisplayUtils.IsPreorder(item))
		{
			storeItemBase._forceToolTipDescriptionOnConfirmation = true;
			StoreDisplayUtils.UpdatePreOrderItem(item, storeItemBase, assetLookupSystem);
			if (item.PreorderData.EndTime > DateTime.MinValue)
			{
				storeItemBase.SetTimer(item.PreorderData.EndTime);
			}
		}
		else
		{
			StoreDisplayUtils.UpdateItemLoc(item, storeItemBase, assetLookupSystem, logger);
			storeItemBase.SetTimer(item.ExpireTime);
			StoreDisplayUtils.RefreshPooledPack(item, storeItemBase, storeButtonLayoutGroup.transform, assetLookupSystem, logger);
		}
		return storeItemBase;
	}

	public static StoreItemBase CreateCardWidget(StoreItem item, Action<StoreItem, Client_PurchaseCurrencyType> executePurchase, Action<StoreItem, Client_PurchaseCurrencyType, Action<StoreItem, Client_PurchaseCurrencyType>> onStoreItemPurchaseOptionClicked, AssetLookupSystem assetLookupSystem, StoreItemDisplay cardStoreItemModel, StoreItemBase storeItemBaseWidePrefab, StoreItemBase storeItemBasePrefab, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, Wizards.Arena.Client.Logging.Logger logger, bool allowStoreTags)
	{
		if (!item.HasRemainingPurchases)
		{
			return null;
		}
		if (!uint.TryParse(item.PrefabIdentifier, out var result))
		{
			logger.Error("Treasure Item for listing " + item.Id + " had an unexpected PrefabIdentifier " + item.PrefabIdentifier + ".");
			return null;
		}
		StoreItemDisplay storeItemDisplay = UnityEngine.Object.Instantiate(cardStoreItemModel);
		storeItemDisplay.SetBackgroundSprite(string.Empty);
		StoreCardView componentInChildren = storeItemDisplay.gameObject.GetComponentInChildren<StoreCardView>();
		CardPrintingData cardPrintingById = cardDatabase.CardDataProvider.GetCardPrintingById(result);
		if (cardPrintingById == null)
		{
			logger.Error("Failed to get printing data for " + item.Id + ". Card is possibly not collectible.");
			return null;
		}
		CardData cardData = cardPrintingById.ConvertToCardModel();
		componentInChildren.InitWithData(cardData, cardDatabase, cardViewBuilder);
		componentInChildren.gameObject.SetActive(value: true);
		StoreItemBase storeItemBase = UnityEngine.Object.Instantiate(storeItemDisplay.WideBase ? storeItemBaseWidePrefab : storeItemBasePrefab);
		storeItemBase.SetLabelText(new MTGALocalizedString
		{
			Key = "MainNav/General/JustText",
			Parameters = new Dictionary<string, string> { 
			{
				"text",
				cardDatabase.GreLocProvider.GetLocalizedText(cardPrintingById.TitleId)
			} }
		});
		storeItemBase.SetPurchaseButtons(item, assetLookupSystem);
		storeItemBase.SetItem(item, allowStoreTags);
		storeItemBase.AttachItemDisplay(storeItemDisplay);
		storeItemBase.PurchaseOptionClicked += delegate(StoreItem storeItem, Client_PurchaseCurrencyType type)
		{
			DeckFormat defaultFormat = WrapperController.Instance.FormatManager.GetDefaultFormat();
			SetAvailability cardTitleAvailability = WrapperController.Instance.FormatManager.GetCardTitleAvailability(cardData.TitleId, defaultFormat);
			bool hasCardsBannedInDeckFormat = defaultFormat.IsCardBanned(cardData.TitleId);
			WrapperController.Instance.InventoryManager.HandlePurchaseAvailabilityWarnings(RotationWarningContext.CardStore, cardTitleAvailability, hasCardsBannedInDeckFormat, Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Popups/SetRotation/Warning_Purchase"), delegate
			{
				onStoreItemPurchaseOptionClicked(storeItem, type, executePurchase);
			}, delegate
			{
			});
		};
		return storeItemBase;
	}

	public static StoreItemBase CreateCardStyleWidget(StoreItem item, Action<StoreItem, Client_PurchaseCurrencyType> executePurchase, Action<StoreItem, Client_PurchaseCurrencyType, Action<StoreItem, Client_PurchaseCurrencyType>> onStoreItemPurchaseOptionClicked, AssetLookupSystem assetLookupSystem, StoreItemDisplay cardStyleStoreItemModel, StoreItemBase storeItemBaseWidePrefab, StoreItemBase storeItemBasePrefab, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, Wizards.Arena.Client.Logging.Logger logger, bool allowStoreTags)
	{
		if (RewardDisplayData.TryParseCard(item.PrefabIdentifier, out var artId, out var styleName))
		{
			Action<StoreItem, Client_PurchaseCurrencyType> formatWarning = delegate(StoreItem i, Client_PurchaseCurrencyType c)
			{
				RewardDisplayData.TryParseCard(item.PrefabIdentifier, out var grpId, out var _);
				FormatManager formatManager = WrapperController.Instance.FormatManager;
				SetAvailability cardArtAvailability = formatManager.GetCardArtAvailability(grpId, formatManager.GetDefaultFormat());
				bool hasCardsBannedInDeckFormat = cardDatabase.DatabaseUtilities.GetPrintingsByArtId(artId).Exists(formatManager, (CardPrintingData printing, FormatManager fm) => fm.GetDefaultFormat().IsCardBanned(printing.TitleId));
				WrapperController.Instance.InventoryManager.HandlePurchaseAvailabilityWarnings(RotationWarningContext.StyleStore, cardArtAvailability, hasCardsBannedInDeckFormat, Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Popups/SetRotation/Warning_Purchase"), delegate
				{
					onStoreItemPurchaseOptionClicked(i, c, executePurchase);
				}, delegate
				{
				});
			};
			StoreItemDisplay storeItemDisplay = UnityEngine.Object.Instantiate(cardStyleStoreItemModel);
			storeItemDisplay.SetBackgroundSprite(string.Empty);
			StoreCardView componentInChildren = storeItemDisplay.GetComponentInChildren<StoreCardView>();
			CardPrintingData cardPrintingData = cardDatabase.DatabaseUtilities.GetPrintingsByArtId(artId).LastOrDefault((CardPrintingData x) => CardUtilities.IsCardCollectible(x) && x.IsPrimaryCard);
			if (cardPrintingData == null)
			{
				logger.Error("Failed to get printing data for " + item.Id + ". Card is possibly not collectible.");
				return null;
			}
			CardData cardData = CardDataExtensions.CreateSkinCard(cardPrintingData.GrpId, cardDatabase, styleName);
			componentInChildren.Init(cardDatabase, cardViewBuilder);
			componentInChildren.gameObject.SetActive(value: true);
			cardData.IsFakeStyleCard = true;
			componentInChildren.SetData(cardData);
			StoreItemBase storeItemBase = UnityEngine.Object.Instantiate(storeItemDisplay.WideBase ? storeItemBaseWidePrefab : storeItemBasePrefab);
			MTGALocalizedString labelText = new MTGALocalizedString
			{
				Key = "BattlePass/Rewards/CardStyle",
				Parameters = new Dictionary<string, string> { 
				{
					"CardName",
					cardDatabase.GreLocProvider.GetLocalizedText(cardPrintingData.TitleId)
				} }
			};
			storeItemBase.SetLabelText(labelText);
			storeItemBase.PurchaseOptionClicked += delegate(StoreItem i, Client_PurchaseCurrencyType c)
			{
				formatWarning(i, c);
			};
			storeItemBase.SetPurchaseButtons(item, assetLookupSystem);
			storeItemBase.SetItem(item, allowStoreTags);
			storeItemBase.AttachItemDisplay(storeItemDisplay);
			return storeItemBase;
		}
		logger.Error("Missing Card Style asset for " + item.Id + ".");
		return null;
	}

	public static StoreItemBase CreateSleeveWidget(StoreItem item, Action<StoreItem, Client_PurchaseCurrencyType> executePurchase, Action<StoreItem, Client_PurchaseCurrencyType, Action<StoreItem, Client_PurchaseCurrencyType>> onStoreItemPurchaseOptionClicked, AssetLookupSystem assetLookupSystem, StoreItemDisplay sleeveStoreItemModel, StoreItemBase storeItemBaseWidePrefab, StoreItemBase storeItemBasePrefab, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, bool allowStoreTags)
	{
		MTGALocalizedString labelText = CosmeticsUtils.LocKeyForSleeveId(item.PrefabIdentifier);
		StoreItemDisplay storeItemDisplay = UnityEngine.Object.Instantiate(sleeveStoreItemModel);
		StoreCardView componentInChildren = storeItemDisplay.GetComponentInChildren<StoreCardView>();
		CardData data = CardDataExtensions.CreateSkinCard(0u, cardDatabase, "", item.PrefabIdentifier);
		componentInChildren.gameObject.SetActive(value: true);
		StoreCardView item2 = componentInChildren.CreateCard(data, cardDatabase, cardViewBuilder, flipSleeved: true);
		storeItemDisplay.CardViews.Add(componentInChildren);
		storeItemDisplay.CardViews.Add(item2);
		StoreItemBase storeItemBase = UnityEngine.Object.Instantiate(storeItemDisplay.WideBase ? storeItemBaseWidePrefab : storeItemBasePrefab);
		storeItemBase.SetPurchaseButtons(item, assetLookupSystem);
		storeItemBase.SetItem(item, allowStoreTags);
		storeItemBase.SetLabelText(labelText);
		storeItemBase.AttachItemDisplay(storeItemDisplay);
		storeItemBase.PurchaseOptionClicked += delegate(StoreItem i, Client_PurchaseCurrencyType c)
		{
			onStoreItemPurchaseOptionClicked(i, c, executePurchase);
		};
		return storeItemBase;
	}

	public static StoreItemBase CreatePrizeWallWidget(StoreItem item, Action<StoreItem, Client_PurchaseCurrencyType> executePurchase, Action<StoreItem, Client_PurchaseCurrencyType, Action<StoreItem, Client_PurchaseCurrencyType>> onStoreItemPurchaseOptionClicked, AssetLookupSystem assetLookupSystem, StoreItemDisplay prizeWallStoreItemModel, StoreItemBase storeItemBasePrefab, Client_PrizeWall prizeWall, bool allowStoreTags, Wizards.Arena.Client.Logging.Logger logger)
	{
		StoreItemDisplay itemDisplay = UnityEngine.Object.Instantiate(prizeWallStoreItemModel);
		StoreItemBase storeItemBase = UnityEngine.Object.Instantiate(storeItemBasePrefab);
		storeItemBase.SetPurchaseButtons(item, assetLookupSystem);
		storeItemBase.SetItem(item, allowStoreTags);
		storeItemBase.SetLabelText(item.LocKey);
		storeItemBase.AttachItemDisplay(itemDisplay);
		storeItemBase.PurchaseOptionClicked += delegate(StoreItem i, Client_PurchaseCurrencyType c)
		{
			onStoreItemPurchaseOptionClicked(i, c, executePurchase);
		};
		StoreDisplayUtils.UpdateItemLoc(item, storeItemBase, assetLookupSystem, logger);
		return storeItemBase;
	}

	public static StoreItemBase CreateAvatarWidget(StoreItem item, Action<StoreItem, Client_PurchaseCurrencyType> executePurchase, Action<StoreItem, Client_PurchaseCurrencyType, Action<StoreItem, Client_PurchaseCurrencyType>> onStoreItemPurchaseOptionClicked, AssetLookupSystem assetLookupSystem, StoreItemDisplay avatarStoreItemModel, StoreItemBase storeItemBaseWidePrefab, StoreItemBase storeItemBasePrefab, bool allowStoreTags)
	{
		MTGALocalizedString labelText = ProfileUtilities.GetAvatarLocKey(item.PrefabIdentifier);
		StoreItemDisplay storeItemDisplay = UnityEngine.Object.Instantiate(avatarStoreItemModel);
		storeItemDisplay.SetBackgroundSprite(ProfileUtilities.GetAvatarStoreImagePath(assetLookupSystem, item.PrefabIdentifier));
		StoreItemBase storeItemBase = UnityEngine.Object.Instantiate(storeItemDisplay.WideBase ? storeItemBaseWidePrefab : storeItemBasePrefab);
		storeItemBase.SetPurchaseButtons(item, assetLookupSystem);
		storeItemBase.SetItem(item, allowStoreTags);
		storeItemBase.SetLabelText(labelText);
		storeItemBase.AttachItemDisplay(storeItemDisplay);
		storeItemBase.PurchaseOptionClicked += delegate(StoreItem i, Client_PurchaseCurrencyType c)
		{
			onStoreItemPurchaseOptionClicked(i, c, executePurchase);
		};
		return storeItemBase;
	}

	public static StoreItemBase CreateGemWidget(StoreItem item, Action<StoreItem, Client_PurchaseCurrencyType> executePurchase, Action<StoreItem, Client_PurchaseCurrencyType, Action<StoreItem, Client_PurchaseCurrencyType>> onStoreItemPurchaseOptionClicked, AssetLookupSystem assetLookupSystem, AssetTracker assetTracker, StoreItemBase storeItemBaseWidePrefab, StoreItemBase storeItemBasePrefab)
	{
		MTGALocalizedString mTGALocalizedString = "MainNav/Store/Gem_Count";
		mTGALocalizedString.Parameters = new Dictionary<string, string> { 
		{
			"count",
			item.GemsCount.ToString("N0")
		} };
		StoreItemDisplay storeItemDisplay = UnityEngine.Object.Instantiate(StoreDisplayUtils.GemsForUnitCount(item.GemsCount, assetLookupSystem, assetTracker, "Gem Widget - " + item.Id));
		StoreItemBase storeItemBase = UnityEngine.Object.Instantiate(storeItemDisplay.WideBase ? storeItemBaseWidePrefab : storeItemBasePrefab);
		storeItemBase.SetPurchaseButtons(item, assetLookupSystem);
		storeItemBase.SetItem(item);
		storeItemBase.SetLabelText(mTGALocalizedString);
		storeItemBase.AttachItemDisplay(storeItemDisplay);
		storeItemBase.PurchaseOptionClicked += delegate(StoreItem i, Client_PurchaseCurrencyType c)
		{
			onStoreItemPurchaseOptionClicked(i, c, executePurchase);
		};
		storeItemBase.SetRolloverSound(WwiseEvents.sfx_ui_main_rewards_gem_rollover);
		return storeItemBase;
	}
}
