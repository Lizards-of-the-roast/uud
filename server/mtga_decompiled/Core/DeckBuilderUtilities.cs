using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GreClient.CardData;
using SharedClientCore.SharedClientCore.Code.Providers;
using Wizards.Arena.DeckValidation.Core.Models;
using Wizards.Arena.Enums.Card;
using Wizards.Arena.Promises;
using Wizards.MDN;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public static class DeckBuilderUtilities
{
	public static void CraftAll(CardDatabase cardDb, Dictionary<uint, CardPrintingQuantity> cardsNeeded, InventoryManager inv, FormatManager formatManager, IGreLocProvider greLocProvider, SystemMessageManager systemMessageManager, DeckBuilderModel deckBuilderModel, ISetMetadataProvider setMetadataProvider, Action purchaseCallback = null)
	{
		Dictionary<uint, CardPrintingQuantity> dictionary = UncraftableCards(cardsNeeded, setMetadataProvider);
		if (dictionary.Count > 0)
		{
			NotifyUserUncraftableCards(dictionary, systemMessageManager, greLocProvider);
			return;
		}
		Dictionary<CardRarity, long> dictionary2 = CalculateCardRaritiesForCrafting(cardDb, cardsNeeded);
		if (dictionary2[CardRarity.Common] > inv.Inventory.wcCommon || dictionary2[CardRarity.Uncommon] > inv.Inventory.wcUncommon || dictionary2[CardRarity.Rare] > inv.Inventory.wcRare || dictionary2[CardRarity.MythicRare] > inv.Inventory.wcMythic)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckBuilder/BatchCrafting_NotEnoughWildcards_Message"));
			foreach (KeyValuePair<CardRarity, long> item in dictionary2)
			{
				AddWildCardNeededCount(item.Key, item.Value, inv, stringBuilder);
			}
			systemMessageManager.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckBuilder/BatchCrafting_NotEnoughWildcards_Title"), stringBuilder.ToString());
		}
		else
		{
			(string, string) tuple = BuildLocalizeCraftAllTitleAndText(dictionary2);
			NotifyUserCraftAllOption(cardsNeeded, tuple.Item1, tuple.Item2, inv, systemMessageManager, formatManager, deckBuilderModel, purchaseCallback);
		}
	}

	private static void AddWildCardNeededCount(CardRarity rarity, long neededCount, InventoryManager inv, StringBuilder stringBuilder)
	{
		int num;
		string key;
		switch (rarity)
		{
		default:
			return;
		case CardRarity.Common:
			num = inv.Inventory.wcCommon;
			key = "MainNav/General/CommonWildcard";
			break;
		case CardRarity.Uncommon:
			num = inv.Inventory.wcUncommon;
			key = "MainNav/General/UncommonWildcard";
			break;
		case CardRarity.Rare:
			num = inv.Inventory.wcRare;
			key = "MainNav/General/RareWildcard";
			break;
		case CardRarity.MythicRare:
			num = inv.Inventory.wcMythic;
			key = "MainNav/General/MythicRareWildcard";
			break;
		}
		if (neededCount > num)
		{
			stringBuilder.AppendLine();
			stringBuilder.Append(Languages.ActiveLocProvider.GetLocalizedText(key));
			stringBuilder.Append(": ");
			stringBuilder.Append(neededCount - num);
		}
	}

	private static Dictionary<uint, CardPrintingQuantity> UncraftableCards(Dictionary<uint, CardPrintingQuantity> cards, ISetMetadataProvider setMetadataProvider)
	{
		return cards.Where((KeyValuePair<uint, CardPrintingQuantity> c) => !CardUtilities.IsCardCraftable(c.Value.Printing) || !setMetadataProvider.IsSetPublished(c.Value.Printing.ExpansionCode)).ToDictionary((KeyValuePair<uint, CardPrintingQuantity> kv) => kv.Key, (KeyValuePair<uint, CardPrintingQuantity> kv) => kv.Value);
	}

	private static void NotifyUserUncraftableCards(Dictionary<uint, CardPrintingQuantity> cards, SystemMessageManager systemMessageManager, IGreLocProvider greLocProvider)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckBuilder/BatchCrafting_UnredeemableCards_Message"));
		foreach (string item in cards.Select((KeyValuePair<uint, CardPrintingQuantity> c) => greLocProvider.GetLocalizedText(c.Value.Printing.TitleId)).Distinct())
		{
			stringBuilder.AppendLine(item);
		}
		systemMessageManager.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckBuilder/BatchCrafting_UnredeemableCards_Title"), stringBuilder.ToString());
	}

	private static long CalculateCraftableNonBasicLands(Dictionary<uint, CardPrintingQuantity> cards)
	{
		return cards.Where((KeyValuePair<uint, CardPrintingQuantity> c) => c.Value.Printing.IsCraftableRarityLand).Sum((KeyValuePair<uint, CardPrintingQuantity> c) => c.Value.Quantity);
	}

	private static long CalculateCardRarityForCrafting(CardDatabase cardDb, Dictionary<uint, CardPrintingQuantity> cards, CardRarity rarity)
	{
		return cards.Where((KeyValuePair<uint, CardPrintingQuantity> c) => CraftingUtilities.GetWildCardRarityForCrafting(cardDb, c.Value.Printing.TitleId) == rarity).Sum((KeyValuePair<uint, CardPrintingQuantity> c) => c.Value.Quantity);
	}

	private static Dictionary<CardRarity, long> CalculateCardRaritiesForCrafting(CardDatabase cardDb, Dictionary<uint, CardPrintingQuantity> cards)
	{
		return new Dictionary<CardRarity, long>
		{
			{
				CardRarity.Common,
				CalculateCardRarityForCrafting(cardDb, cards, CardRarity.Common)
			},
			{
				CardRarity.Uncommon,
				CalculateCardRarityForCrafting(cardDb, cards, CardRarity.Uncommon)
			},
			{
				CardRarity.Rare,
				CalculateCardRarityForCrafting(cardDb, cards, CardRarity.Rare)
			},
			{
				CardRarity.MythicRare,
				CalculateCardRarityForCrafting(cardDb, cards, CardRarity.MythicRare)
			}
		};
	}

	private static WildcardBulkRequest GetWildcardBulkRequest(Dictionary<uint, CardPrintingQuantity> cards)
	{
		WildcardBulkRequest wildcardBulkRequest = new WildcardBulkRequest(cards.Count);
		foreach (CardPrintingQuantity value in cards.Values)
		{
			wildcardBulkRequest.bulkRequest.Add(new CardAndQuantity(value.Printing.GrpId, value.Quantity));
		}
		return wildcardBulkRequest;
	}

	private static void NotifyUserCraftAllOption(Dictionary<uint, CardPrintingQuantity> cardsNeeded, string title, string text, InventoryManager inv, SystemMessageManager systemMessageManager, FormatManager formatManager, DeckBuilderModel deckBuilderModel, Action PurchaseCallback = null)
	{
		WildcardBulkRequest bulkRequest = GetWildcardBulkRequest(cardsNeeded);
		if (!cardsNeeded.Any())
		{
			PromiseExtensions.Logger.Error("Craft all called on deck that has no craftable cards.");
			return;
		}
		SetAvailability availability = cardsNeeded.Min((KeyValuePair<uint, CardPrintingQuantity> c) => formatManager.GetCardTitleAvailability(c.Value.Printing.TitleId, formatManager.GetDefaultFormat()));
		bool isBannedInCurrentFormat = cardsNeeded.Exists((KeyValuePair<uint, CardPrintingQuantity> printingQuantity) => formatManager.GetSafeFormat(deckBuilderModel._deckFormat).IsCardBanned(printingQuantity.Value.Printing.TitleId));
		bool willBeOverRestrictedLimit = false;
		if (deckBuilderModel != null && !string.IsNullOrEmpty(deckBuilderModel._deckFormat))
		{
			DeckFormat safeFormat = formatManager.GetSafeFormat(deckBuilderModel._deckFormat);
			Dictionary<uint, Quota> restrictedTitleIds = safeFormat.RestrictedTitleIds;
			if (restrictedTitleIds != null && restrictedTitleIds.Count > 0)
			{
				foreach (KeyValuePair<uint, CardPrintingQuantity> item in cardsNeeded)
				{
					if (safeFormat.RestrictedTitleIds.TryGetValue(item.Value.Printing.TitleId, out var value))
					{
						willBeOverRestrictedLimit = deckBuilderModel.GetQuantityInWholeDeckByTitle(item.Value.Printing) > value.Max;
					}
				}
			}
		}
		Action Cancel = delegate
		{
		};
		systemMessageManager.ShowMessage(title, text, Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckBuilder/Cancel_Button"), null, Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckBuilder/BatchCrafting_OKButton"), OnOk);
		void OnOk()
		{
			inv.HandlePurchaseAvailabilityWarnings(RotationWarningContext.CraftAll, availability, isBannedInCurrentFormat, Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Popups/SetRotation/Warning_Craft"), Purchase, Cancel, willBeOverRestrictedLimit);
		}
		void Purchase()
		{
			PAPA.StartGlobalCoroutine(PurchaseFlow());
		}
		IEnumerator PurchaseFlow()
		{
			yield return inv.Coroutine_RedeemWildcards(bulkRequest);
			PurchaseCallback?.Invoke();
		}
	}

	private static string CardRarityKeyToLocKey(CardRarity rarity)
	{
		return rarity switch
		{
			CardRarity.Common => "Enum/Rarity/Common", 
			CardRarity.Uncommon => "Enum/Rarity/Uncommon", 
			CardRarity.Rare => "Enum/Rarity/Rare", 
			CardRarity.MythicRare => "Enum/Rarity/MythicRare", 
			_ => rarity.ToString(), 
		};
	}

	private static string CardRarityLocalization(Dictionary<CardRarity, long> rarities, CardRarity rarity)
	{
		if (rarities[rarity] > 0)
		{
			return $"{rarities[rarity]} {Languages.ActiveLocProvider.GetLocalizedText(CardRarityKeyToLocKey(rarity))}";
		}
		return "";
	}

	private static (string title, string text) BuildLocalizeCraftAllTitleAndText(Dictionary<CardRarity, long> rarities)
	{
		long num = rarities[CardRarity.Common] + rarities[CardRarity.Uncommon] + rarities[CardRarity.Rare] + rarities[CardRarity.MythicRare];
		(string, string) result = ("", "");
		if (num == 1)
		{
			result.Item1 = Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckBuilder/BatchCrafting_MessageTitle_OneItem");
			string rarityLoc = Utils.GetRarityLoc(rarities.First((KeyValuePair<CardRarity, long> x) => x.Value > 0).Key);
			result.Item2 = Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckBuilder/BatchCrafting_MessageContent_OneItem", ("0", rarityLoc));
		}
		else
		{
			List<string> list = new List<string>();
			list.Add(CardRarityLocalization(rarities, CardRarity.Common));
			list.Add(CardRarityLocalization(rarities, CardRarity.Uncommon));
			list.Add(CardRarityLocalization(rarities, CardRarity.Rare));
			list.Add(CardRarityLocalization(rarities, CardRarity.MythicRare));
			string text = string.Join(Environment.NewLine, list.Where((string s) => !string.IsNullOrEmpty(s)));
			result.Item1 = Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckBuilder/BatchCrafting_MessageTitle_Plural", ("0", num.ToString("N0")));
			result.Item2 = Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckBuilder/BatchCrafting_MessageContent_Plural", ("0", num.ToString("N0")));
			ref string item = ref result.Item2;
			item = item + Environment.NewLine + text;
		}
		return result;
	}
}
