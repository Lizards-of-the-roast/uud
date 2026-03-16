using System;
using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using Wotc.Mtga.Cards;
using Wotc.Mtga.Cards.Database;

public static class BaseSortedCardCache
{
	private static readonly Dictionary<int, List<CardPrintingData>> _baseSortedCache = new Dictionary<int, List<CardPrintingData>>();

	private static int GenerateHashCode(bool ignoreIsCollectible, bool allowSpecializeFacets, SortType[] sortCriteria)
	{
		HashCode hashCode = default(HashCode);
		foreach (SortType value in sortCriteria)
		{
			hashCode.Add((int)value);
		}
		hashCode.Add(ignoreIsCollectible);
		hashCode.Add(allowSpecializeFacets);
		return hashCode.ToHashCode();
	}

	public static bool TryGetList(bool ignoreIsCollectible, bool allowSpecializeFacets, SortType[] sortCriteria, out List<CardPrintingData> list)
	{
		return _baseSortedCache.TryGetValue(GenerateHashCode(ignoreIsCollectible, allowSpecializeFacets, sortCriteria), out list);
	}

	public static void CacheList(bool ignoreIsCollectible, bool allowSpecializeFacets, SortType[] sortCriteria, List<CardPrintingData> list)
	{
		_baseSortedCache[GenerateHashCode(ignoreIsCollectible, allowSpecializeFacets, sortCriteria)] = list;
	}

	public static void ClearCachedList(bool ignoreIsCollectible, bool allowSpecializeFacets, SortType[] sortCriteria)
	{
		_baseSortedCache.Remove(GenerateHashCode(ignoreIsCollectible, allowSpecializeFacets, sortCriteria));
	}

	public static void ClearAllCachedLists()
	{
		_baseSortedCache.Clear();
	}

	private static IEnumerable<CardPrintingData> GenerateBaseList(IDatabaseUtilities cardDatabaseUtils, bool ignoreIsCollectible, bool allowSpecializeFacets, SortType[] sortCriteria)
	{
		foreach (CardPrintingData printing in cardDatabaseUtils.GetPrimaryPrintings(sortCriteria))
		{
			if (!ignoreIsCollectible && !CardUtilities.IsCardCollectible(printing))
			{
				continue;
			}
			yield return printing;
			if (!allowSpecializeFacets || !SpecializeUtilities.IsSpecializeBaseCard(printing))
			{
				continue;
			}
			foreach (CardPrintingData specializeFacet in SpecializeUtilities.GetSpecializeFacets(printing))
			{
				yield return specializeFacet;
			}
		}
	}

	public static void SortAndCacheList(ICardDatabaseAdapter cardDatabase, bool ignoreIsCollectible, bool allowSpecializeFacets, SortType[] sortCriteria)
	{
		if (!TryGetList(ignoreIsCollectible, allowSpecializeFacets, sortCriteria, out var _))
		{
			List<CardPrintingData> list2 = CardSorter.Sort(GenerateBaseList(cardDatabase.DatabaseUtilities, ignoreIsCollectible, allowSpecializeFacets, sortCriteria), cardDatabase, cardsSortedFromDatabase: true, sortCriteria).ToList();
			CacheList(ignoreIsCollectible, allowSpecializeFacets, sortCriteria, list2);
		}
	}
}
