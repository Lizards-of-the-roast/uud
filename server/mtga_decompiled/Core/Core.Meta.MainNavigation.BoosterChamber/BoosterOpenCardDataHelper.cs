using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using SharedClientCore.SharedClientCore.Code.Providers;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;

namespace Core.Meta.MainNavigation.BoosterChamber;

public static class BoosterOpenCardDataHelper
{
	public static List<CardData> GetCardDataOverride(CardDatabase cardDatabase, CrackBoostersCardInfo[] originalData = null)
	{
		return null;
	}

	public static List<CardData> ConvertToCardDataList(CardDatabase cardDatabase, CrackBoostersCardInfo[] cards)
	{
		return cards.Select((CrackBoostersCardInfo c) => (!c.addedToInventory && c.gemsAwarded > 0) ? CardDataExtensions.CreateRewardsCard(cardDatabase, c.goldAwarded, c.gemsAwarded, c.set, "Booster") : new CardData(null, cardDatabase.CardDataProvider.GetCardPrintingById(c.grpId))).ToList();
	}

	public static List<CardData> SortCardsByRarity(List<CardData> cards)
	{
		return cards.OrderByDescending((CardData p) => (int)p.Rarity).ToList();
	}

	public static void AddTags(List<CardDataAndRevealStatus> cards, InventoryManager inventory, ISetMetadataProvider setMetadataProvider)
	{
		foreach (CardDataAndRevealStatus card in cards)
		{
			if (!card.CardData.IsWildcard && !IsGems(card.CardData))
			{
				if (inventory.Cards.TryGetValue(card.CardData.GrpId, out var value) && cards.Count((CardDataAndRevealStatus c) => c.CardData.GrpId == card.CardData.GrpId) == value)
				{
					card.Tags.Add(Languages.ActiveLocProvider.GetLocalizedText("MainNav/NewTags/First_label"));
				}
				if (setMetadataProvider.SetIsBonusSheet(string.IsNullOrEmpty(card.CardData.DigitalReleaseSet) ? card.CardData.ExpansionCode : card.CardData.DigitalReleaseSet))
				{
					card.Tags.Add(Languages.ActiveLocProvider.GetLocalizedText("MainNav/NewTags/BonusSheet_Label"));
				}
			}
		}
	}

	public static List<CardDataAndRevealStatus> AddRevealStatusAndRebalancedCardsToCardData(List<CardData> cardsToOpen, bool autoFlipEnabled = false, bool alwaysSkipAnimation = false)
	{
		List<CardDataAndRevealStatus> list = new List<CardDataAndRevealStatus>();
		foreach (CardData card in cardsToOpen)
		{
			if (!card.IsRebalanced)
			{
				list.Add(new CardDataAndRevealStatus
				{
					CardData = card,
					RebalancedCardData = ((card.RebalancedCardLink == 0) ? null : cardsToOpen.FirstOrDefault((CardData c) => c.GrpId == card.RebalancedCardLink)),
					InFinalPosition = alwaysSkipAnimation,
					Revealed = alwaysSkipAnimation,
					AutoReveal = (autoFlipEnabled || !IsRareOrMythic(card) || IsGems(card)),
					NeedsAnticipation = IsRareOrMythic(card)
				});
			}
		}
		return (from c in list
			where !c.CardData.IsRebalanced
			orderby c.AutoReveal
			select c).ToList();
	}

	private static bool IsRareOrMythic(CardData card)
	{
		CardRarity rarity = card.Rarity;
		return rarity == CardRarity.Rare || rarity == CardRarity.MythicRare;
	}

	private static bool IsGems(CardData card)
	{
		return card.GrpId == 0;
	}
}
