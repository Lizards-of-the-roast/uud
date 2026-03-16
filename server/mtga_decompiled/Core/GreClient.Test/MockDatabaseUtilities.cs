using System;
using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Cards;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;

namespace GreClient.Test;

public class MockDatabaseUtilities : IDatabaseUtilities
{
	private readonly Dictionary<string, List<CardPrintingData>> _printingsByLocalizedTitle;

	private readonly Dictionary<uint, List<CardPrintingData>> _printingsByTitleId;

	private readonly Dictionary<string, List<CardPrintingData>> _printingsByExpansion;

	public MockDatabaseUtilities(Dictionary<string, List<CardPrintingData>> printingsByLocalizedTitle = null, Dictionary<uint, List<CardPrintingData>> printingsByTitleId = null, Dictionary<string, List<CardPrintingData>> printingsByExpansion = null)
	{
		_printingsByTitleId = printingsByTitleId;
		_printingsByLocalizedTitle = printingsByLocalizedTitle ?? new Dictionary<string, List<CardPrintingData>>();
		_printingsByExpansion = printingsByExpansion ?? new Dictionary<string, List<CardPrintingData>>();
	}

	public ISet<string> SetsInDatabase()
	{
		return new HashSet<string>();
	}

	public ISet<string> DigitalReleaseSetsInDatabase()
	{
		return new HashSet<string>();
	}

	public IReadOnlyDictionary<uint, CardPrintingData> GetAllPrintings()
	{
		return DictionaryExtensions.Empty<uint, CardPrintingData>();
	}

	public IReadOnlyList<CardPrintingData> GetPrimaryPrintings(params SortType[] sortTypes)
	{
		return Array.Empty<CardPrintingData>();
	}

	public IReadOnlyList<CardPrintingData> GetPrintingsByTitleId(uint titleId)
	{
		if (!_printingsByTitleId.TryGetValue(titleId, out var value))
		{
			return Array.Empty<CardPrintingData>();
		}
		return value;
	}

	public IReadOnlyList<CardPrintingData> GetPrintingsByArtId(uint artId)
	{
		return Array.Empty<CardPrintingData>();
	}

	public IReadOnlyList<CardPrintingData> GetPrintingsByExpansion(string expansionCode)
	{
		if (!_printingsByExpansion.TryGetValue(expansionCode, out var value))
		{
			return Array.Empty<CardPrintingData>();
		}
		return value;
	}

	public IReadOnlyList<CardPrintingData> GetPrintingsByEnglishTitle(string title)
	{
		return Array.Empty<CardPrintingData>();
	}

	public IReadOnlyList<CardPrintingData> GetPrintingsByLocalizedTitle(string title)
	{
		if (!_printingsByLocalizedTitle.TryGetValue(title, out var value))
		{
			return Array.Empty<CardPrintingData>();
		}
		return value;
	}

	public IReadOnlyCollection<uint> GetTitleIdsLegalForFormatData(IReadOnlyCollection<string> allowedSets, bool isPauper, bool isArtisan, bool isRebalanced)
	{
		return (IReadOnlyCollection<uint>)(object)Array.Empty<uint>();
	}

	public IReadOnlyDictionary<string, IReadOnlyDictionary<uint, string>> GetAllLocalizationData()
	{
		return DictionaryExtensions.Empty<string, IReadOnlyDictionary<uint, string>>();
	}

	public IReadOnlyDictionary<uint, (string, string)> GetAllEnglishCardTitles()
	{
		return DictionaryExtensions.Empty<uint, (string, string)>();
	}

	public IReadOnlyDictionary<uint, string> GetAllEnglishAbilitiesText()
	{
		return DictionaryExtensions.Empty<uint, string>();
	}

	public void PreloadPrintingsByTitleIdCache()
	{
	}
}
