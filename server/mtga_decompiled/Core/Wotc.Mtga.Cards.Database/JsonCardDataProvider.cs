using System;
using System.Collections.Generic;
using System.IO;
using GreClient.CardData;
using GreClient.CardData.ImportExport;
using Newtonsoft.Json;
using Wizards.Mtga.Utils;

namespace Wotc.Mtga.Cards.Database;

public class JsonCardDataProvider : ICardDataProvider
{
	private readonly Dictionary<uint, CardPrintingRecord> _recordMap = new Dictionary<uint, CardPrintingRecord>(500);

	private readonly Dictionary<uint, CardPrintingData> _printingMap = new Dictionary<uint, CardPrintingData>(500);

	private readonly IAbilityDataProvider _abilityProvider;

	public JsonCardDataProvider(IAbilityDataProvider abilityProvider, StringCache stringCache, string cardDataPath)
	{
		_abilityProvider = abilityProvider;
		CardPrintingRecordConverter cardPrintingRecordConverter = new CardPrintingRecordConverter(stringCache);
		Type typeFromHandle = typeof(CardPrintingRecord);
		JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.Auto,
			Formatting = Formatting.Indented,
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
			DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
			ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
			NullValueHandling = NullValueHandling.Ignore
		});
		using TextReader reader = FileSystemUtils.OpenText(cardDataPath);
		using JsonTextReader jsonTextReader = new JsonTextReader(reader);
		jsonTextReader.Read();
		while (jsonTextReader.Read() && jsonTextReader.TokenType != JsonToken.EndArray)
		{
			CardPrintingRecord value = cardPrintingRecordConverter.ReadJson(jsonTextReader, typeFromHandle, CardPrintingRecord.Blank, hasExistingValue: false, serializer);
			if (value.GrpId != 0)
			{
				_recordMap[value.GrpId] = value;
			}
		}
		jsonTextReader.Read();
	}

	public CardPrintingRecord GetCardRecordById(uint id, string skinCode = null)
	{
		if (!_recordMap.TryGetValue(id, out var value))
		{
			return CardPrintingRecord.Blank;
		}
		return value;
	}

	public CardPrintingData GetCardPrintingById(uint id, string skinCode = null)
	{
		if (!_printingMap.TryGetValue(id, out var value))
		{
			if (!_recordMap.TryGetValue(id, out var value2))
			{
				return null;
			}
			return _printingMap[id] = new CardPrintingData(value2, this, _abilityProvider);
		}
		return value;
	}

	public IEnumerable<uint> GetCardIds()
	{
		return _recordMap.Keys;
	}
}
