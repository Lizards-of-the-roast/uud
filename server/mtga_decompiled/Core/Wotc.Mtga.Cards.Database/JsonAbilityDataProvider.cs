using System;
using System.Collections.Generic;
using System.IO;
using GreClient.CardData;
using GreClient.CardData.ImportExport;
using Newtonsoft.Json;

namespace Wotc.Mtga.Cards.Database;

public class JsonAbilityDataProvider : IAbilityDataProvider
{
	private static readonly AbilityPrintingRecordConverter RecordConverter = new AbilityPrintingRecordConverter();

	private Dictionary<uint, AbilityPrintingRecord> _recordMap = new Dictionary<uint, AbilityPrintingRecord>(500);

	private Dictionary<uint, AbilityPrintingData> _printingMap = new Dictionary<uint, AbilityPrintingData>(500);

	public JsonAbilityDataProvider(string abilityDataPath)
	{
		Type typeFromHandle = typeof(AbilityPrintingRecord);
		JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.Auto,
			Formatting = Formatting.Indented,
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
			DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
			ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
			NullValueHandling = NullValueHandling.Ignore
		});
		using TextReader reader = FileSystemUtils.OpenText(abilityDataPath);
		using JsonTextReader jsonTextReader = new JsonTextReader(reader);
		jsonTextReader.Read();
		jsonTextReader.Read();
		while (jsonTextReader.TokenType != JsonToken.EndArray)
		{
			AbilityPrintingRecord value = RecordConverter.ReadJson(jsonTextReader, typeFromHandle, AbilityPrintingRecord.Blank, hasExistingValue: false, serializer);
			if (value.Id != 0)
			{
				_recordMap[value.Id] = value;
			}
		}
	}

	public AbilityPrintingRecord GetAbilityRecordById(uint id)
	{
		if (!_recordMap.TryGetValue(id, out var value))
		{
			return AbilityPrintingRecord.Blank;
		}
		return value;
	}

	public AbilityPrintingData GetAbilityPrintingById(uint id)
	{
		if (!_printingMap.TryGetValue(id, out var value))
		{
			if (!_recordMap.TryGetValue(id, out var value2))
			{
				return null;
			}
			return _printingMap[id] = new AbilityPrintingData(value2, this);
		}
		return value;
	}

	public IEnumerable<uint> GetAbilityIds()
	{
		return _recordMap.Keys;
	}
}
