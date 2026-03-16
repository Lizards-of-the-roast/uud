using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;

namespace Core.Meta.MainNavigation.Store;

public readonly struct PrecalculatedRewardUpdateInfo
{
	public readonly List<CardAdded> UnpairedCards;

	public readonly List<CardAddedWithBonus> PairedCards;

	public readonly List<string> SetsToGrant;

	public readonly int GoldLessCards;

	public readonly int GemsLessCards;

	private PrecalculatedRewardUpdateInfo(List<CardAdded> unpairedCards, List<CardAddedWithBonus> pairedCards, List<string> setsToGrant, int goldLessCards, int gemsLessCards)
	{
		UnpairedCards = unpairedCards;
		PairedCards = pairedCards;
		SetsToGrant = setsToGrant;
		GoldLessCards = goldLessCards;
		GemsLessCards = gemsLessCards;
	}

	public static PrecalculatedRewardUpdateInfo Create(ICardDatabaseAdapter cardDatabase, List<string> setsOfInterest, ClientInventoryUpdateReportItem updateMessage)
	{
		List<AetherizedCardInformation> aetherizedCards = updateMessage.aetherizedCards;
		object obj;
		if (aetherizedCards != null && aetherizedCards.Count == 0)
		{
			InventoryDelta delta = updateMessage.delta;
			if (delta != null && delta.cardsAdded?.Length > 0)
			{
				obj = updateMessage.delta?.cardsAdded?.Select((int grpId) => new AetherizedCardInformation
				{
					grpId = grpId
				}).ToList() ?? new List<AetherizedCardInformation>();
				goto IL_00c2;
			}
		}
		obj = updateMessage.aetherizedCards?.ToList() ?? new List<AetherizedCardInformation>();
		goto IL_00c2;
		IL_00c2:
		List<AetherizedCardInformation> list = (List<AetherizedCardInformation>)obj;
		List<string> list2 = new List<string>();
		if (list.Count > 40)
		{
			IEnumerable<int> second = list.Select((AetherizedCardInformation x) => x.grpId).ToList();
			foreach (string item in setsOfInterest)
			{
				HashSet<int> printingsInSet = (from x in cardDatabase.DatabaseUtilities.GetPrintingsByExpansion(item)
					where x.Rarity != CardRarity.Land && x.Rarity != CardRarity.None && CardUtilities.IsCardCraftable(x)
					select (int)x.GrpId).ToHashSet();
				if (!printingsInSet.Except(second).Any())
				{
					list2.Add(item);
					list = list.Where((AetherizedCardInformation x) => !printingsInSet.Contains(x.grpId)).ToList();
				}
			}
		}
		int num = updateMessage.delta?.goldDelta ?? 0;
		int num2 = updateMessage.delta?.gemsDelta ?? 0;
		foreach (AetherizedCardInformation item2 in list)
		{
			num -= item2.goldAwarded;
			num2 -= item2.gemsAwarded;
		}
		var (unpairedCards, pairedCards) = FindAlchemyCardRewardPairs(cardDatabase.CardDataProvider, list);
		return new PrecalculatedRewardUpdateInfo(unpairedCards, pairedCards, list2, num, num2);
	}

	private static (List<CardAdded>, List<CardAddedWithBonus>) FindAlchemyCardRewardPairs(ICardDataProvider cardDatabase, List<AetherizedCardInformation> uncategorizedCards)
	{
		Dictionary<uint, CardAdded> dictionary = new Dictionary<uint, CardAdded>();
		List<CardAddedWithBonus> list = new List<CardAddedWithBonus>();
		foreach (IGrouping<int, AetherizedCardInformation> item2 in from _ in uncategorizedCards
			group _ by _.grpId)
		{
			item2.Deconstruct(out var key, out var grouped);
			int grpID = key;
			AetherizedCardInformation[] source = grouped.ToArray();
			AetherizedCardInformation aetherizedInfo = source.First();
			CardAdded cardAdded = new CardAdded
			{
				GrpID = (uint)grpID,
				ExpectedRarity = CardRarity.None,
				AetherizedInfo = aetherizedInfo,
				count = (uint)source.Count()
			};
			dictionary.Add(cardAdded.GrpID, cardAdded);
		}
		foreach (KeyValuePair<uint, CardAdded> item3 in dictionary)
		{
			item3.Deconstruct(out var key2, out var value);
			uint id = key2;
			CardAdded cardAdded2 = value;
			CardPrintingData cardPrintingById = cardDatabase.GetCardPrintingById(id);
			if (cardPrintingById == null || !cardPrintingById.IsRebalanced || cardAdded2.IsPartOfAlchemyPair)
			{
				continue;
			}
			dictionary.TryGetValue(cardPrintingById.RebalancedCardLink, out var value2);
			if (value2 != null && !value2.IsPartOfAlchemyPair)
			{
				uint count;
				if (cardAdded2.count == value2.count)
				{
					count = value2.count;
					value2.count = 0u;
					cardAdded2.count = 0u;
				}
				else if (value2.count < cardAdded2.count)
				{
					count = value2.count;
					value2.count = 0u;
					cardAdded2.count -= count;
				}
				else
				{
					count = cardAdded2.count;
					cardAdded2.count = 0u;
					value2.count -= count;
				}
				CardAddedWithBonus item = new CardAddedWithBonus
				{
					card = new CardAdded
					{
						GrpID = cardAdded2.GrpID,
						ExpectedRarity = cardAdded2.ExpectedRarity,
						AetherizedInfo = cardAdded2.AetherizedInfo,
						count = count
					},
					bonusCard = new CardAdded
					{
						GrpID = value2.GrpID,
						ExpectedRarity = value2.ExpectedRarity,
						AetherizedInfo = value2.AetherizedInfo,
						count = count
					}
				};
				list.Add(item);
			}
		}
		return (dictionary.Values.Where((CardAdded _) => !_.IsPartOfAlchemyPair).ToList(), list);
	}
}
