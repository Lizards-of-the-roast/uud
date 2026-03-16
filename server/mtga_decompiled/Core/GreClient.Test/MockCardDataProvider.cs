using System;
using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;

namespace GreClient.Test;

public class MockCardDataProvider : ICardDataProvider
{
	private readonly Dictionary<uint, CardPrintingData> _cards;

	private readonly Dictionary<uint, Dictionary<string, uint>> _altPrintings;

	private readonly IAbilityDataProvider _abilityProvider;

	public MockCardDataProvider(IAbilityDataProvider abilityProvider, Dictionary<uint, CardPrintingData> cards = null, Dictionary<uint, Dictionary<string, uint>> altPrintings = null)
	{
		_cards = cards ?? new Dictionary<uint, CardPrintingData>();
		_altPrintings = altPrintings ?? new Dictionary<uint, Dictionary<string, uint>>();
		_abilityProvider = abilityProvider;
	}

	public MockCardDataProvider(IAbilityDataProvider abilityProvider, params CardPrintingRecord[] records)
	{
		_cards = new Dictionary<uint, CardPrintingData>();
		_abilityProvider = abilityProvider;
		CardPrintingRecord[] array = records ?? Array.Empty<CardPrintingRecord>();
		foreach (CardPrintingRecord record in array)
		{
			AddCard(record);
		}
	}

	public void AddCard(CardPrintingRecord record)
	{
		_cards[record.GrpId] = new CardPrintingData(record, this, _abilityProvider);
	}

	public void AddAltPrinting(uint baseId, uint altGrpId, string skinCode)
	{
		if (!_altPrintings.ContainsKey(baseId))
		{
			_altPrintings.Add(baseId, new Dictionary<string, uint>());
		}
		_altPrintings[baseId][skinCode] = altGrpId;
	}

	public void Clear()
	{
		_cards.Clear();
		_altPrintings.Clear();
	}

	public CardPrintingData GetCardPrintingById(uint id, string skinCode = null)
	{
		if (!string.IsNullOrEmpty(skinCode) && _altPrintings.TryGetValue(id, out var value) && value.TryGetValue(skinCode, out var value2) && _cards.TryGetValue(value2, out var value3))
		{
			return value3;
		}
		if (!_cards.TryGetValue(id, out var value4))
		{
			return null;
		}
		return value4;
	}

	public CardPrintingRecord GetCardRecordById(uint id, string skinCode = null)
	{
		return GetCardPrintingById(id, skinCode)?.Record ?? CardPrintingRecord.Blank;
	}

	public IEnumerable<uint> GetCardIds()
	{
		return _cards.Keys;
	}
}
