using System;
using System.Collections.Generic;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;

namespace GreClient.Test;

public class MockGreLocProvider : IGreLocProvider
{
	private readonly Dictionary<uint, string> _loc;

	public MockGreLocProvider(Dictionary<uint, string> loc = null)
	{
		_loc = loc ?? new Dictionary<uint, string>();
	}

	public string GetLocalizedText(uint locId, string overrideLanguageCode = null, bool formatted = true)
	{
		if (!_loc.TryGetValue(locId, out var value))
		{
			return null;
		}
		return value;
	}

	public string GetLocalizedTextForEnumValue(string enumName, int enumVal, bool formatted = true, string overrideLanguageCode = null)
	{
		return $"Enum {enumName}:{enumVal}";
	}

	public uint GetEnumLocId(string enumName, int enumVal)
	{
		return 0u;
	}

	public IEnumerable<uint> GetLocIds()
	{
		return _loc.Keys;
	}

	public HashSet<uint> GetLocIdsForSearchTerms(string substring, IList<string> additionalSubstrings)
	{
		HashSet<uint> hashSet = new HashSet<uint>();
		foreach (KeyValuePair<uint, string> kvp in _loc)
		{
			if (kvp.Value.IndexOf(substring, StringComparison.OrdinalIgnoreCase) >= 0 || additionalSubstrings.Exists((string a) => kvp.Value.IndexOf(a, StringComparison.OrdinalIgnoreCase) >= 0))
			{
				hashSet.Add(kvp.Key);
			}
		}
		return hashSet;
	}

	public IEnumerable<(string, int)> GetEnums()
	{
		return Array.Empty<(string, int)>();
	}

	public string GetLocalizedTextForEnumValue<T>(T enumVal, bool formatted = true, string overrideLanguageCode = null) where T : Enum
	{
		return GetLocalizedTextForEnumValue("T", Convert.ToInt32(enumVal), formatted, overrideLanguageCode);
	}

	public void Clear()
	{
		_loc.Clear();
	}
}
