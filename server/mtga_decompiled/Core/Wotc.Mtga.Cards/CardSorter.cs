using System;
using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using Wizards.MDN.Store;
using Wizards.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.Cards;

public static class CardSorter
{
	public static SortType[] CardPoolNewCardsFirstSort = new SortType[5]
	{
		SortType.IsNew,
		SortType.ColorOrder,
		SortType.LandLast,
		SortType.CMCWithXLast,
		SortType.Title
	};

	public static CardCollection Sort(CardCollection unsortedData, ICardDatabaseAdapter cardDb, params SortType[] criteria)
	{
		return new CardCollection(cardDb, SortInternal(unsortedData, (ICardCollectionItem x) => x.Card.Printing, cardDb.GreLocProvider, cardsSortedFromDatabase: false, criteria));
	}

	public static IEnumerable<ListMetaCardViewDisplayInformation> Sort(IEnumerable<ListMetaCardViewDisplayInformation> unsortedData, ICardDatabaseAdapter cardDb, params SortType[] criteria)
	{
		return SortInternal(unsortedData, (ListMetaCardViewDisplayInformation x) => x.Card, cardDb.GreLocProvider, cardsSortedFromDatabase: false, criteria);
	}

	public static IEnumerable<CardPrintingData> Sort(IEnumerable<CardPrintingData> unsortedData, ICardDatabaseAdapter cardDb, bool cardsSortedFromDatabase, params SortType[] criteria)
	{
		return SortInternal(unsortedData, (CardPrintingData x) => x, cardDb.GreLocProvider, cardsSortedFromDatabase, criteria);
	}

	public static IEnumerable<CardPrintingQuantity> Sort(IEnumerable<CardPrintingQuantity> unsortedData, ICardDatabaseAdapter cardDb, params SortType[] criteria)
	{
		return SortInternal(unsortedData, (CardPrintingQuantity x) => x.Printing, cardDb.GreLocProvider, cardsSortedFromDatabase: false, criteria);
	}

	public static IEnumerable<CardDataForTile> Sort(IEnumerable<CardDataForTile> unsortedData, ICardDatabaseAdapter cardDb, params SortType[] criteria)
	{
		return SortInternal(unsortedData, (CardDataForTile x) => x.Card.Printing, cardDb.GreLocProvider, cardsSortedFromDatabase: false, criteria);
	}

	public static IOrderedEnumerable<T> SortInternal<T>(IEnumerable<T> cards, Func<T, CardPrintingData> mapper, IGreLocProvider greLocProvider, bool cardsSortedFromDatabase, params SortType[] criteria)
	{
		IOrderedEnumerable<T> orderedEnumerable = cards.OrderBy((T x) => 0);
		foreach (SortType sortType in criteria)
		{
			if (cardsSortedFromDatabase && CardSortHelpers.DatabaseSortTypes.Contains(sortType))
			{
				continue;
			}
			switch (sortType)
			{
			case SortType.Title:
				orderedEnumerable = orderedEnumerable.ThenBy((T c) => greLocProvider.GetLocalizedText(mapper(c).TitleId, null, formatted: false));
				break;
			case SortType.IsNew:
			{
				InventoryManager inventoryManager = Pantry.Get<InventoryManager>();
				orderedEnumerable = orderedEnumerable.ThenBy((T c) => (inventoryManager?.newCards?.ContainsKey(mapper(c).GrpId) != true) ? 1 : 0);
				break;
			}
			default:
			{
				Func<CardPrintingData, int> sortFunc = CardSortHelpers.GetDatabaseSortOrderFunc(sortType);
				orderedEnumerable = orderedEnumerable.ThenBy((T c) => sortFunc(mapper(c)));
				break;
			}
			}
		}
		return orderedEnumerable;
	}
}
