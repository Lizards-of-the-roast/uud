using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wizards.Arena.DeckValidation.Client;
using Wizards.Arena.Enums.Card;
using Wotc.Mtga.Cards;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Wizards.Mtga.Decks;

public static class DeckViewUtilities
{
	private static readonly string ColorChallengeStringHeader = "Events/Event_Campaign_Title_";

	public static string GetToolTipForDeck(DeckDisplayInfo deckDisplayInfo, bool isEditable = true)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (deckDisplayInfo.ValidationResult.CardTitlesOverRestrictedListQuota.Count > 0)
		{
			MTGALocalizedString arg = "MainNav/ConstructedDeckSelect/Tooltip_InvalidDeck_RestrictedCount";
			stringBuilder.AppendLine($"{arg} {deckDisplayInfo.ValidationResult.CardTitlesOverRestrictedListQuota.Count}");
		}
		List<Client_DeckCard> value;
		if (!string.IsNullOrWhiteSpace(deckDisplayInfo.ColorChallengeEventLock))
		{
			MTGALocalizedString mTGALocalizedString = "MainNav/DeckManager/Tooltip_PlayMoreColorChallenges";
			MTGALocalizedString mTGALocalizedString2 = ColorChallengeStringHeader + deckDisplayInfo.ColorChallengeEventLock;
			mTGALocalizedString.Parameters = new Dictionary<string, string> { { "color", mTGALocalizedString2 } };
			stringBuilder.AppendLine(mTGALocalizedString);
		}
		else if (deckDisplayInfo.IsMalformed)
		{
			if (deckDisplayInfo.ValidationResult.NumberOfInvalidCards == 0)
			{
				MTGALocalizedString mTGALocalizedString3 = "MainNav/DeckBuilder/DeckBuilder_MessageTitle_InvalidDeck";
				stringBuilder.AppendLine(mTGALocalizedString3);
			}
			else
			{
				MTGALocalizedString arg2 = "MainNav/ConstructedDeckSelect/Tooltip_InvalidCards";
				stringBuilder.AppendLine($"{arg2} {deckDisplayInfo.ValidationResult.NumberOfInvalidCards}");
			}
		}
		else if (deckDisplayInfo.IsWarning)
		{
			ClientSideDeckValidationResult validationResult = deckDisplayInfo.ValidationResult;
			uint num = validationResult.NumberBannedCards + validationResult.NumberEmergencyBannedCards;
			if (num != 0)
			{
				MTGALocalizedString arg3 = "MainNav/ConstructedDeckSelect/Tooltip_BannedCards";
				stringBuilder.AppendLine($"{arg3} {num}");
			}
			if (deckDisplayInfo.ValidationResult.NumberNonFormatCard != 0)
			{
				MTGALocalizedString arg4 = "MainNav/ConstructedDeckSelect/Tooltip_NonFormatCards";
				stringBuilder.AppendLine($"{arg4} {deckDisplayInfo.ValidationResult.NumberNonFormatCard}");
			}
		}
		else if (deckDisplayInfo.IsUnowned)
		{
			if (deckDisplayInfo.IsUncraftable && deckDisplayInfo.ValidationResult.NumberOfUnownedUncraftableCards != 0)
			{
				MTGALocalizedString mTGALocalizedString4 = "MainNav/DeckBuilder/BatchCrafting_MessageContent_HasUncraftableCards";
				stringBuilder.AppendLine(mTGALocalizedString4);
			}
			else
			{
				stringBuilder.Append(BuildWildCardsNeededToolTip(deckDisplayInfo.ValidationResult.GetUnownedCardCountsByRarity()));
			}
		}
		else if (!deckDisplayInfo.ValidationResult.CompanionIsValid && deckDisplayInfo.Deck.Contents.Piles.TryGetValue(EDeckPile.Companions, out value))
		{
			Client_DeckCard client_DeckCard = value.FirstOrDefault();
			if (Pantry.Get<ICardDataProvider>().TryGetCardPrintingById(client_DeckCard.Id, out var card))
			{
				MTGALocalizedString mTGALocalizedString5 = "MainNav/DeckBuilder/CompanionInvalid_Title";
				stringBuilder.AppendLine(mTGALocalizedString5);
				MTGALocalizedString mTGALocalizedString6 = "MainNav/DeckBuilder/CompanionSelection_Body";
				string abilityText = CompanionUtil.GetAbilityText(card, Pantry.Get<IGreLocProvider>());
				mTGALocalizedString6.Parameters = new Dictionary<string, string> { { "conditionText", abilityText } };
				stringBuilder.AppendLine(mTGALocalizedString6);
			}
		}
		if (isEditable)
		{
			MTGALocalizedString mTGALocalizedString7 = "DuelScene/Browsers/Select_Deck_Tooltip";
			stringBuilder.AppendLine(mTGALocalizedString7);
		}
		return stringBuilder.ToString();
	}

	private static StringBuilder BuildWildCardsNeededToolTip(Dictionary<Rarity, int> unownedCards)
	{
		StringBuilder stringBuilder = new StringBuilder();
		MTGALocalizedString mTGALocalizedString = "MainNav/ConstructedDeckSelect/Tooltip_WildcardsNeeded";
		stringBuilder.AppendLine(mTGALocalizedString);
		stringBuilder.Append(ConstructToolTipFromUnownedBasedOffRarity(Rarity.Common, unownedCards));
		stringBuilder.Append(ConstructToolTipFromUnownedBasedOffRarity(Rarity.Uncommon, unownedCards));
		stringBuilder.Append(ConstructToolTipFromUnownedBasedOffRarity(Rarity.Rare, unownedCards));
		stringBuilder.Append(ConstructToolTipFromUnownedBasedOffRarity(Rarity.Mythic, unownedCards));
		return stringBuilder;
	}

	private static StringBuilder ConstructToolTipFromUnownedBasedOffRarity(Rarity rarity, Dictionary<Rarity, int> unownedCards)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (unownedCards.TryGetValue(rarity, out var value))
		{
			MTGALocalizedString arg = GetWildCardLocalizationKeyForRarity(rarity);
			stringBuilder.AppendLine($"{arg} {value}");
		}
		return stringBuilder;
	}

	private static string GetWildCardLocalizationKeyForRarity(Rarity rarity)
	{
		return rarity switch
		{
			Rarity.Common => "MainNav/ConstructedDeckSelect/Tooltip_Common", 
			Rarity.Uncommon => "MainNav/ConstructedDeckSelect/Tooltip_Uncommon", 
			Rarity.Rare => "MainNav/ConstructedDeckSelect/Tooltip_Rare", 
			Rarity.Mythic => "MainNav/ConstructedDeckSelect/Tooltip_Mythic", 
			_ => "", 
		};
	}

	public static void ClearNewCardSortedCacheIfRequired()
	{
		if (Pantry.Get<IInventoryServiceWrapper>().newCards.Count > 0)
		{
			BaseSortedCardCache.ClearCachedList(ignoreIsCollectible: false, allowSpecializeFacets: false, CardSorter.CardPoolNewCardsFirstSort);
		}
	}

	public static void ClearNewCardSortedCache()
	{
		BaseSortedCardCache.ClearCachedList(ignoreIsCollectible: false, allowSpecializeFacets: false, CardSorter.CardPoolNewCardsFirstSort);
	}

	public static List<DeckViewInfo> SortDeckViewInfo(List<DeckViewInfo> deckViewInfos, string formatForSort, DeckViewSortType sortType)
	{
		switch (sortType)
		{
		case DeckViewSortType.Invalid:
			SimpleLog.LogError("Tried Sorting DeckViewInfo for Invalid Type, returning unsorted");
			break;
		case DeckViewSortType.LastPlayed:
			deckViewInfos = (from di in deckViewInfos
				orderby di.isFavorite descending, di.GetValidationForFormat(formatForSort).IsMalformed, di.GetValidationForFormat(formatForSort).IsUncraftable, di.GetValidationForFormat(formatForSort).ValidationResult.IsValid descending, di.LastPlayed descending, di.deckName
				select di).ToList();
			break;
		case DeckViewSortType.LastModified:
			deckViewInfos = (from di in deckViewInfos
				orderby di.isFavorite descending, di.GetValidationForFormat(formatForSort).IsMalformed, di.GetValidationForFormat(formatForSort).IsUncraftable, di.GetValidationForFormat(formatForSort).ValidationResult.IsValid descending, di.LastUpdated descending, di.deckName
				select di).ToList();
			break;
		case DeckViewSortType.Alphabetical:
			deckViewInfos = (from di in deckViewInfos
				orderby di.isFavorite descending, di.GetValidationForFormat(formatForSort).IsMalformed, di.GetValidationForFormat(formatForSort).IsUncraftable, di.GetValidationForFormat(formatForSort).ValidationResult.IsValid descending, di.deckName, di.LastUpdated descending
				select di).ToList();
			break;
		default:
			deckViewInfos = (from di in deckViewInfos
				orderby di.GetValidationForFormat(formatForSort).IsMalformed, di.GetValidationForFormat(formatForSort).ValidationResult.IsCraftable(Pantry.Get<InventoryManager>().Inventory.CombinedWildcardInventory()), di.GetValidationForFormat(formatForSort).ValidationResult.IsValid descending, di.isFavorite descending, (!(di.LastUpdated > di.LastPlayed)) ? di.LastPlayed : di.LastUpdated descending, di.deckName
				select di).ToList();
			break;
		}
		return deckViewInfos;
	}

	public static int NumInvalidCards(uint proposedInvalidCards, uint bannedCards, bool isCraftableNow, bool isCraftableWithMoreWildCards)
	{
		if (!InvalidCardsAreCraftable(bannedCards, isCraftableNow, isCraftableWithMoreWildCards))
		{
			return (int)proposedInvalidCards;
		}
		return 0;
	}

	private static bool InvalidCardsAreCraftable(uint bannedCards, bool isCraftableNow, bool isCraftableWithMoreWildCards)
	{
		if (bannedCards == 0)
		{
			return isCraftableNow || isCraftableWithMoreWildCards;
		}
		return false;
	}
}
