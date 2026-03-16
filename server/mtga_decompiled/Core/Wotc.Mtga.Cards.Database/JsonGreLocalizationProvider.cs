using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Wizards.Mtga;
using Wizards.Mtga.Utils;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Cards.Database;

public class JsonGreLocalizationProvider : IGreLocProvider
{
	private readonly Dictionary<string, Dictionary<uint, (string, string)>> _localizationMap = new Dictionary<string, Dictionary<uint, (string, string)>>();

	private readonly Dictionary<string, Dictionary<int, uint>> _enumMap = new Dictionary<string, Dictionary<int, uint>>();

	public JsonGreLocalizationProvider(string locDataPath, string enumDataPath)
	{
		LoadLocalizationData(locDataPath);
		LoadEnumData(enumDataPath);
	}

	public string GetLocalizedText(uint locId, string overrideLanguageCode = null, bool formatted = true)
	{
		string text = overrideLanguageCode ?? Languages.CurrentLanguage;
		if (_localizationMap.TryGetValue(text, out var value) && value.TryGetValue(locId, out var value2) && !LocalizationManagerUtils.IsDefaultGreString(value2.Item1))
		{
			if (!formatted)
			{
				return value2.Item1;
			}
			return value2.Item2;
		}
		IAccountClient accountClient = Pantry.Get<IAccountClient>();
		if (accountClient != null && accountClient.IsPreProd)
		{
			return $"missing {text} translation for LocID: {locId}";
		}
		if (!string.Equals(text, "en-US") && _localizationMap.TryGetValue("en-US", out value) && value.TryGetValue(locId, out value2))
		{
			if (!formatted)
			{
				return value2.Item1;
			}
			return value2.Item2;
		}
		return string.Empty;
	}

	public string GetLocalizedTextForEnumValue<T>(T enumVal, bool formatted = true, string overrideLanguageCode = null) where T : Enum
	{
		return GetLocalizedTextForEnumValue("T", Convert.ToInt32(enumVal), formatted, overrideLanguageCode);
	}

	public string GetLocalizedTextForEnumValue(string enumName, int enumVal, bool formatted = true, string overrideLanguageCode = null)
	{
		if (!_enumMap.TryGetValue(enumName, out var value))
		{
			return "UNKNOWN ENUM NAME (" + enumName + ")";
		}
		if (!value.TryGetValue(enumVal, out var value2))
		{
			return $"UNKNOWN ENUM ID ({enumVal}) FOR NAME ({enumName})";
		}
		string localizedText = GetLocalizedText(value2, overrideLanguageCode, formatted);
		if (string.IsNullOrEmpty(localizedText))
		{
			return $"UNKNOWN LOCALIZATION ID ({value2})";
		}
		return localizedText;
	}

	public uint GetEnumLocId(string enumName, int enumVal)
	{
		if (_enumMap.TryGetValue(enumName, out var value) && value.TryGetValue(enumVal, out var value2))
		{
			return value2;
		}
		return 0u;
	}

	public IEnumerable<uint> GetLocIds()
	{
		if (_localizationMap.TryGetValue("en-US", out var value))
		{
			return value.Keys;
		}
		return Array.Empty<uint>();
	}

	public HashSet<uint> GetLocIdsForSearchTerms(string substring, IList<string> additionalSubstrings)
	{
		HashSet<uint> hashSet = new HashSet<uint>();
		if (_localizationMap.TryGetValue(Languages.CurrentLanguage, out var value) || _localizationMap.TryGetValue("en-US", out value))
		{
			foreach (KeyValuePair<uint, (string, string)> item2 in value)
			{
				item2.Deconstruct(out var key, out var value2);
				(string, string) tuple = value2;
				uint item = key;
				var (rawLoc, formattedLoc) = tuple;
				if (rawLoc.IndexOf(substring, StringComparison.OrdinalIgnoreCase) >= 0 || additionalSubstrings.Exists((string add) => rawLoc.IndexOf(add, StringComparison.OrdinalIgnoreCase) >= 0) || formattedLoc.IndexOf(substring, StringComparison.OrdinalIgnoreCase) >= 0 || additionalSubstrings.Exists((string add) => formattedLoc.IndexOf(add, StringComparison.OrdinalIgnoreCase) >= 0))
				{
					hashSet.Add(item);
				}
			}
		}
		return hashSet;
	}

	public IEnumerable<(string, int)> GetEnums()
	{
		foreach (KeyValuePair<string, Dictionary<int, uint>> enumKvp in _enumMap)
		{
			foreach (KeyValuePair<int, uint> item in enumKvp.Value)
			{
				yield return (enumKvp.Key, item.Key);
			}
		}
	}

	private void LoadLocalizationData(string locDataPath)
	{
		_localizationMap.Clear();
		StringCache stringCache = new StringCache();
		using TextReader reader = FileSystemUtils.OpenText(locDataPath);
		using JsonTextReader jsonTextReader = new JsonTextReader(reader);
		while (jsonTextReader.TokenType != JsonToken.EndArray)
		{
			jsonTextReader.Read();
			jsonTextReader.Read();
			while (jsonTextReader.TokenType != JsonToken.EndArray)
			{
				jsonTextReader.Read();
				jsonTextReader.Read();
				jsonTextReader.Read();
				string key = stringCache.Get(jsonTextReader.ReadAsString());
				jsonTextReader.Read();
				jsonTextReader.Read();
				jsonTextReader.Read();
				while (jsonTextReader.TokenType != JsonToken.EndArray)
				{
					uint key2 = 0u;
					string text = null;
					string item = null;
					do
					{
						jsonTextReader.Read();
						switch (jsonTextReader.Value as string)
						{
						case "id":
							key2 = (uint)jsonTextReader.ReadAsInt32().Value;
							break;
						case "raw":
							text = stringCache.Get(jsonTextReader.ReadAsString());
							break;
						case "text":
							if (string.IsNullOrEmpty(text))
							{
								text = stringCache.Get(jsonTextReader.ReadAsString());
								item = text;
							}
							else
							{
								item = stringCache.Get(jsonTextReader.ReadAsString());
							}
							break;
						}
					}
					while (jsonTextReader.Value != null);
					jsonTextReader.Read();
					if (!_localizationMap.TryGetValue(key, out var value))
					{
						value = (_localizationMap[key] = new Dictionary<uint, (string, string)>());
					}
					if (!value.ContainsKey(key2))
					{
						value.Add(key2, (text, item));
					}
				}
				jsonTextReader.Read();
				jsonTextReader.Read();
			}
		}
	}

	private void LoadEnumData(string enumDataPath)
	{
		StringCache stringCache = new StringCache();
		using TextReader reader = FileSystemUtils.OpenText(enumDataPath);
		using JsonTextReader jsonTextReader = new JsonTextReader(reader);
		jsonTextReader.Read();
		jsonTextReader.Read();
		while (jsonTextReader.TokenType != JsonToken.EndArray)
		{
			jsonTextReader.Read();
			string key = stringCache.Get(jsonTextReader.ReadAsString());
			Dictionary<int, uint> dictionary = new Dictionary<int, uint>();
			_enumMap.Add(key, dictionary);
			jsonTextReader.Read();
			jsonTextReader.Read();
			jsonTextReader.Read();
			while (jsonTextReader.TokenType != JsonToken.EndArray)
			{
				jsonTextReader.Read();
				int value = jsonTextReader.ReadAsInt32().Value;
				jsonTextReader.Read();
				uint value2 = (uint)jsonTextReader.ReadAsInt32().Value;
				jsonTextReader.Read();
				jsonTextReader.Read();
				dictionary.Add(value, value2);
			}
			jsonTextReader.Read();
			jsonTextReader.Read();
		}
	}
}
