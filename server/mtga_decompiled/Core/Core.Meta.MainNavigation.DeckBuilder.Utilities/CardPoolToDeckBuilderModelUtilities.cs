using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using SharedClientCore.SharedClientCore.Code.Providers;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Cards.Database;

namespace Core.Meta.MainNavigation.DeckBuilder.Utilities;

public static class CardPoolToDeckBuilderModelUtilities
{
	public static Dictionary<uint, uint> GetCardPoolFromContext(DeckBuilderContext context, IFormatManager formatManager, IInventoryManager inventoryManager, ICardDatabaseAdapter cardDatabase)
	{
		if (context == null)
		{
			return new Dictionary<uint, uint>();
		}
		Dictionary<uint, uint> dictionary = new Dictionary<uint, uint>();
		if (context.OnlyShowPoolCards)
		{
			if (context.StartingMode == DeckBuilderMode.ReadOnlyCollection)
			{
				if (context.CardPoolOverride != null)
				{
					dictionary = context.CardPoolOverride;
				}
				else if (context.Format != null)
				{
					AddCardsLegalInFormatToPool(cardDatabase.DatabaseUtilities.GetPrimaryPrintings(), dictionary, context.Format);
				}
				else if (context.Event?.PlayerEvent?.EventUXInfo?.DeckSelectFormat != null)
				{
					DeckFormat safeFormat = formatManager.GetSafeFormat(context.Event.PlayerEvent.EventUXInfo.DeckSelectFormat);
					AddCardsLegalInFormatToPool(cardDatabase.DatabaseUtilities.GetPrimaryPrintings(), dictionary, safeFormat);
				}
			}
			else if (context.IsSideboarding)
			{
				AddCardsInDeckToPool(context.Deck, dictionary);
				context.Deck.sideboard.Clear();
				if (context.IsLimited && inventoryManager != null)
				{
					AddUnlimitedBasicLandsToPool(inventoryManager.Cards, cardDatabase.CardDataProvider, dictionary);
				}
			}
			else if (context.IsEvent)
			{
				List<uint> list = context.Event.PlayerEvent.CourseData?.CardPool;
				if (list != null)
				{
					if (context.Event.PlayerEvent.EventInfo.FormatType == MDNEFormatType.Draft && context.CardPoolOverride != null)
					{
						dictionary = context.CardPoolOverride;
					}
					else
					{
						AddCardsFromRawPoolToPool(list, dictionary);
					}
				}
				else
				{
					AddCardsInDeckToPool(context.Deck, dictionary);
				}
				context.Deck.sideboard.Clear();
				if (context.IsLimited && inventoryManager != null)
				{
					AddUnlimitedBasicLandsToPool(inventoryManager.Cards, cardDatabase.CardDataProvider, dictionary);
				}
			}
			else
			{
				AddAllCardsFromInventoryToPool(inventoryManager.Cards, dictionary);
			}
		}
		else if (inventoryManager != null)
		{
			AddAllCardsFromInventoryToPool(inventoryManager.Cards, dictionary);
		}
		return dictionary;
	}

	public static Dictionary<uint, string> GetCardSkinOverridesFromEventData(ICardDatabaseAdapter cardDb, Dictionary<uint, string> existingCardSkinOverrides, List<string> eventCardStyles, Dictionary<uint, uint> eventCardPool)
	{
		Dictionary<uint, string> dictionary = existingCardSkinOverrides ?? new Dictionary<uint, string>();
		foreach (string eventCardStyle in eventCardStyles)
		{
			string[] array = eventCardStyle.Split('.');
			uint artId = uint.Parse(array[0]);
			foreach (uint item in from p in cardDb.DatabaseUtilities.GetPrintingsByArtId(artId)
				select p.GrpId)
			{
				if (eventCardPool.ContainsKey(item))
				{
					dictionary[item] = array[1];
				}
			}
		}
		return dictionary;
	}

	private static void AddCardsInDeckToPool(DeckInfo deckInfo, Dictionary<uint, uint> cardPool)
	{
		foreach (CardInDeck item in deckInfo.mainDeck.Concat(deckInfo.sideboard).Concat(deckInfo.commandZone))
		{
			cardPool.TryGetValue(item.Id, out var value);
			value += item.Quantity;
			cardPool[item.Id] = value;
		}
	}

	private static void AddCardsLegalInFormatToPool(IReadOnlyCollection<CardPrintingData> printings, Dictionary<uint, uint> cardPool, DeckFormat deckFormat)
	{
		foreach (uint item in from pair in printings
			where deckFormat.IsCardLegal(pair.TitleId)
			select pair.GrpId)
		{
			cardPool[item] = 4u;
		}
	}

	private static void AddAllCardsFromInventoryToPool(Dictionary<uint, int> userInventory, Dictionary<uint, uint> cardPool)
	{
		foreach (var (key, num3) in userInventory)
		{
			cardPool.TryGetValue(key, out var value);
			value = (cardPool[key] = value + (uint)num3);
		}
	}

	private static void AddCardsFromRawPoolToPool(IEnumerable<uint> rawPool, Dictionary<uint, uint> cardPool)
	{
		foreach (uint item in rawPool)
		{
			cardPool.TryGetValue(item, out var value);
			value = (cardPool[item] = value + 1);
		}
	}

	private static void AddUnlimitedBasicLandsToPool(Dictionary<uint, int> usersCardInventory, ICardDataProvider cardDatabase, Dictionary<uint, uint> cardPool)
	{
		foreach (KeyValuePair<uint, int> item in usersCardInventory)
		{
			item.Deconstruct(out var key, out var value);
			uint num = key;
			int num2 = value;
			CardPrintingData cardPrintingById = cardDatabase.GetCardPrintingById(num);
			if (cardPrintingById != null && !cardPool.ContainsKey(num) && cardPrintingById.IsBasicLandUnlimited && num2 >= cardPrintingById.MaxCollected)
			{
				cardPool.Add(num, cardPrintingById.MaxCollected);
			}
		}
	}
}
