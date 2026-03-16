using System;
using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;

namespace GreClient.Test;

public class MockAbilityDataProvider : IAbilityDataProvider
{
	private readonly Dictionary<uint, AbilityPrintingData> _abilities;

	public MockAbilityDataProvider(Dictionary<uint, AbilityPrintingData> mockAbilityMap = null)
	{
		_abilities = mockAbilityMap ?? new Dictionary<uint, AbilityPrintingData>();
	}

	public MockAbilityDataProvider(IEnumerable<AbilityPrintingRecord> abilityRecords)
	{
		_abilities = new Dictionary<uint, AbilityPrintingData>();
		foreach (AbilityPrintingRecord item in abilityRecords ?? Array.Empty<AbilityPrintingRecord>())
		{
			AddAbility(item);
		}
	}

	public MockAbilityDataProvider(params AbilityPrintingRecord[] abilityRecords)
		: this((IEnumerable<AbilityPrintingRecord>)abilityRecords)
	{
	}

	public void AddAbility(AbilityPrintingRecord record)
	{
		_abilities[record.Id] = new AbilityPrintingData(record, this);
	}

	public AbilityPrintingRecord GetAbilityRecordById(uint id)
	{
		if (!_abilities.TryGetValue(id, out var value))
		{
			return AbilityPrintingRecord.Blank;
		}
		return value.Record;
	}

	public AbilityPrintingData GetAbilityPrintingById(uint id)
	{
		if (!_abilities.TryGetValue(id, out var value))
		{
			return null;
		}
		return value;
	}

	public IEnumerable<uint> GetAbilityIds()
	{
		return _abilities.Keys;
	}

	public void Clear()
	{
		_abilities.Clear();
	}
}
