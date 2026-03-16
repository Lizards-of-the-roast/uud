using System;
using System.Collections.Generic;
using System.Linq;
using Core.Shared.Code.CardFilters;
using GreClient.CardData;
using Wotc.Mtga.Cards;
using Wotc.Mtga.Cards.Database;

public class CardList
{
	private readonly List<CardPrintingQuantity> _filteredSortedList;

	private readonly List<CardPrintingQuantity> _sortedCardCollection;

	private IReadOnlyList<Func<CardFilterGroup, CardFilterGroup>> _filters = Array.Empty<Func<CardFilterGroup, CardFilterGroup>>();

	private readonly ICardDatabaseAdapter _db;

	private Dictionary<uint, uint> _grpQuantityTable;

	private SortType[] _sortCriteria = new SortType[0];

	private bool _includeUnowned = true;

	private readonly bool _ignoreIsCollectible;

	private readonly bool _allowSpecializeFacets;

	private bool _sortIsDirty = true;

	private uint? _totalSizeCache;

	private readonly Dictionary<uint, uint> _titleQuantityTable = new Dictionary<uint, uint>();

	public CardList(ICardDatabaseAdapter db, Dictionary<uint, uint> grpIdCounts, bool ignoreIsCollectible = false, bool allowSpecializeFacets = false)
	{
		_db = db;
		_grpQuantityTable = new Dictionary<uint, uint>(grpIdCounts);
		BuildTitleCountCache(grpIdCounts);
		_filteredSortedList = new List<CardPrintingQuantity>();
		_sortedCardCollection = new List<CardPrintingQuantity>();
		_ignoreIsCollectible = ignoreIsCollectible;
		_allowSpecializeFacets = allowSpecializeFacets;
	}

	public uint GetQuantityByGrpId(uint grpId)
	{
		return GetDictionaryCount(_grpQuantityTable, grpId);
	}

	public uint GetQuantityByTitle(uint titleId)
	{
		return GetDictionaryCountByTitle(_titleQuantityTable, titleId);
	}

	public uint GetTotalSize()
	{
		uint valueOrDefault = _totalSizeCache.GetValueOrDefault();
		if (!_totalSizeCache.HasValue)
		{
			valueOrDefault = (uint)_grpQuantityTable.Values.Sum((uint x) => x);
			_totalSizeCache = valueOrDefault;
			return valueOrDefault;
		}
		return valueOrDefault;
	}

	public void AddByGrpId(uint grpId, uint count = 1u)
	{
		IncrementDictionary(_grpQuantityTable, grpId, count);
		CardPrintingData cardPrintingById = _db.CardDataProvider.GetCardPrintingById(grpId);
		if (!_titleQuantityTable.TryAdd(cardPrintingById.TitleId, count))
		{
			_titleQuantityTable[cardPrintingById.TitleId] += count;
		}
		_totalSizeCache = null;
	}

	public void RemoveByGrpId(uint grpId, uint count = 1u)
	{
		if (GetQuantityByGrpId(grpId) == 0)
		{
			throw new InvalidOperationException($"Cannot remove GRP ID {grpId} from collection because none exist in the collection");
		}
		DecrementDictionary(_grpQuantityTable, grpId, count);
		CardPrintingData cardPrintingById = _db.CardDataProvider.GetCardPrintingById(grpId);
		if (_titleQuantityTable.ContainsKey(cardPrintingById.TitleId))
		{
			_titleQuantityTable[cardPrintingById.TitleId] -= count;
			if (_titleQuantityTable[cardPrintingById.TitleId] == 0)
			{
				_titleQuantityTable.Remove(cardPrintingById.TitleId);
			}
		}
		_totalSizeCache = null;
	}

	public void Clear()
	{
		_grpQuantityTable.Clear();
		_titleQuantityTable.Clear();
		_sortIsDirty = true;
		_totalSizeCache = null;
	}

	public IReadOnlyDictionary<uint, uint> GetRawTable()
	{
		return _grpQuantityTable;
	}

	public IReadOnlyList<CardPrintingQuantity> GetFilteredSortedList()
	{
		IReadOnlyList<CardPrintingQuantity> filteredSortedList = _filteredSortedList;
		return filteredSortedList ?? Array.Empty<CardPrintingQuantity>();
	}

	public IReadOnlyList<CardPrintingQuantity> GetCardCollection()
	{
		IReadOnlyList<CardPrintingQuantity> sortedCardCollection = _sortedCardCollection;
		return sortedCardCollection ?? Array.Empty<CardPrintingQuantity>();
	}

	public IReadOnlyList<CardPrintingData> GetBaseSortedCardCache()
	{
		if (!BaseSortedCardCache.TryGetList(_ignoreIsCollectible, _allowSpecializeFacets, _sortCriteria, out var list))
		{
			throw new InvalidProgramException("Unable to find a cached list when one should have existed. Ensure a list has been created and sorted before applying filters.");
		}
		return list;
	}

	public void SetQuantities(Dictionary<uint, uint> grpQuantityTable)
	{
		_grpQuantityTable = new Dictionary<uint, uint>(grpQuantityTable);
		BuildTitleCountCache(grpQuantityTable);
		_totalSizeCache = null;
		BaseSortedCardCache.ClearCachedList(_ignoreIsCollectible, _allowSpecializeFacets, _sortCriteria);
		ReSortAndFilter();
	}

	private void BuildTitleCountCache(Dictionary<uint, uint> grpQuantityTable)
	{
		_titleQuantityTable.Clear();
		foreach (KeyValuePair<uint, uint> item in grpQuantityTable ?? new Dictionary<uint, uint>())
		{
			item.Deconstruct(out var key, out var value);
			uint id = key;
			uint num = value;
			CardPrintingData cardPrintingById = _db.CardDataProvider.GetCardPrintingById(id);
			if (cardPrintingById != null && !_titleQuantityTable.TryAdd(cardPrintingById.TitleId, num))
			{
				Dictionary<uint, uint> titleQuantityTable = _titleQuantityTable;
				value = cardPrintingById.TitleId;
				titleQuantityTable[value] += num;
			}
		}
	}

	private static void DecrementDictionary(Dictionary<uint, uint> dict, uint key, uint count)
	{
		dict.TryGetValue(key, out var value);
		if (value <= count)
		{
			dict.Remove(key);
		}
		else
		{
			dict[key] = value - count;
		}
	}

	private static void IncrementDictionary(Dictionary<uint, uint> dict, uint key, uint count)
	{
		if (!dict.TryAdd(key, count))
		{
			dict[key] += count;
		}
	}

	private static uint GetDictionaryCount(Dictionary<uint, uint> dict, uint key)
	{
		dict.TryGetValue(key, out var value);
		return value;
	}

	private static uint GetDictionaryCountByTitle(Dictionary<uint, uint> titleIdTable, uint titleId)
	{
		titleIdTable.TryGetValue(titleId, out var value);
		return value;
	}

	public void ReSortSkipFilter()
	{
		_sortIsDirty = true;
		SetSortSkipFilter(_sortCriteria);
	}

	public void ReSortAndFilter()
	{
		_sortIsDirty = true;
		SetSortAndFilter(_sortCriteria);
	}

	public void SetSortSkipFilter(params SortType[] criteria)
	{
		if (_sortIsDirty || !criteria.SequenceEqual(_sortCriteria))
		{
			_sortIsDirty = false;
			_sortCriteria = criteria;
			BaseSortedCardCache.SortAndCacheList(_db, _ignoreIsCollectible, _allowSpecializeFacets, _sortCriteria);
		}
	}

	public void SetSortAndFilter(params SortType[] criteria)
	{
		if (_sortIsDirty || !criteria.SequenceEqual(_sortCriteria))
		{
			_sortIsDirty = false;
			_sortCriteria = criteria;
			BaseSortedCardCache.SortAndCacheList(_db, _ignoreIsCollectible, _allowSpecializeFacets, _sortCriteria);
			ApplyFilters();
		}
	}

	public void SetFilters(IReadOnlyList<Func<CardFilterGroup, CardFilterGroup>> filters)
	{
		_filters = filters;
		ApplyFilters();
	}

	public void SetIncludeUnownedOnly(bool includeUnowned)
	{
		_includeUnowned = includeUnowned;
		_sortIsDirty = true;
	}

	public void ApplyFilters()
	{
		_filteredSortedList.Clear();
		_sortedCardCollection.Clear();
		IReadOnlyList<CardPrintingData> baseSortedCardCache = GetBaseSortedCardCache();
		List<CardFilterGroup.FilteredCard> list = new List<CardFilterGroup.FilteredCard>();
		int num = 0;
		foreach (CardPrintingData item2 in baseSortedCardCache)
		{
			uint quantityByGrpId = GetQuantityByGrpId(item2.GrpId);
			if (_includeUnowned || quantityByGrpId != 0)
			{
				list.Add(new CardFilterGroup.FilteredCard(item2, num));
				num++;
			}
		}
		if (list.Count > 0)
		{
			CardFilterGroup cardFilterGroup = new CardFilterGroup(list);
			foreach (Func<CardFilterGroup, CardFilterGroup> filter in _filters)
			{
				cardFilterGroup = filter(cardFilterGroup);
			}
			list = cardFilterGroup.GetFilteredCards_Passed();
		}
		if (list.Count <= 0)
		{
			return;
		}
		foreach (CardFilterGroup.FilteredCard item3 in list)
		{
			CardPrintingData card = item3.Card;
			uint quantityByGrpId2 = GetQuantityByGrpId(card.GrpId);
			CardPrintingQuantity item = new CardPrintingQuantity
			{
				Printing = card,
				Quantity = quantityByGrpId2
			};
			_filteredSortedList.Add(item);
			if (_grpQuantityTable.ContainsKey(card.GrpId))
			{
				_sortedCardCollection.Add(item);
			}
		}
	}
}
