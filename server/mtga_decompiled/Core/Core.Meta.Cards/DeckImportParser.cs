using System;
using System.Collections.Generic;
using System.IO;
using Wotc.Mtga.Loc;

namespace Core.Meta.Cards;

public static class DeckImportParser
{
	public static readonly List<(string, string)> LocalizedPileMappings = new List<(string, string)>
	{
		("Main", "MainNav/DeckBuilder/Deck_Label"),
		("Sideboard", "MainNav/DeckBuilder/Sideboard_Label"),
		("CommandZone", "MainNav/DeckBuilder/Commander"),
		("Companions", "MainNav/DeckBuilder/Companion"),
		("Metadata", "MainNav/DeckImporter/Metadata")
	};

	public static readonly List<string> PilesInSequence = new List<string> { "Main", "Sideboard", "CommandZone" };

	public static Dictionary<string, List<string>> ParseDeckFromString(string inputString, Dictionary<string, List<string>> validPileNames, List<string> defaultPileNames)
	{
		int currentIndex = 0;
		Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
		Dictionary<string, string> dictionary2 = CreateReverseLocLookup(validPileNames);
		List<string> list = null;
		StringReader stringReader = new StringReader(inputString);
		do
		{
			string text = ReadUntilWeHaveContents(stringReader, out var linesRead);
			if (text == null)
			{
				break;
			}
			text = text.Trim();
			bool flag = false;
			if (dictionary2.TryGetValue(text, out var value))
			{
				list = PileForName(value, dictionary);
				flag = true;
			}
			else if (list == null || linesRead > 1)
			{
				list = GetNextDefaultPile(ref currentIndex, defaultPileNames, dictionary);
				flag = false;
			}
			if (!flag)
			{
				list?.Add(text);
			}
		}
		while (!EndOfFileReached(stringReader));
		return dictionary;
	}

	private static bool EndOfFileReached(StringReader stringReader)
	{
		return stringReader.Peek() == -1;
	}

	private static List<string> GetNextDefaultPile(ref int currentIndex, List<string> defaultPileNames, Dictionary<string, List<string>> allPiles)
	{
		int index = Math.Min(currentIndex++, defaultPileNames.Count - 1);
		return PileForName(defaultPileNames[index], allPiles);
	}

	private static List<string> PileForName(string pileName, Dictionary<string, List<string>> allPiles)
	{
		if (!allPiles.TryGetValue(pileName, out var value))
		{
			value = (allPiles[pileName] = new List<string>());
		}
		return value;
	}

	public static Dictionary<string, string> CreateReverseLocLookup(Dictionary<string, List<string>> validPileNames)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (KeyValuePair<string, List<string>> validPileName in validPileNames)
		{
			foreach (string item in validPileName.Value)
			{
				dictionary[item] = validPileName.Key;
			}
		}
		return dictionary;
	}

	private static string ReadUntilWeHaveContents(StringReader stringReader, out int linesRead)
	{
		linesRead = 1;
		string text;
		while (true)
		{
			text = stringReader.ReadLine();
			if (text == null)
			{
				return null;
			}
			if (!string.IsNullOrWhiteSpace(text))
			{
				break;
			}
			linesRead++;
		}
		return text;
	}

	public static bool IsEmpty(string strVal)
	{
		int linesRead;
		return ReadUntilWeHaveContents(new StringReader(strVal), out linesRead) == null;
	}

	public static Dictionary<string, List<string>> GetLocalizedNames(IClientLocProvider localizationManager, IEnumerable<string> langCodes, IEnumerable<(string PileName, string LocKey)> pileKeyTuples)
	{
		Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
		foreach (var (key, key2) in pileKeyTuples)
		{
			List<string> list = (dictionary[key] = new List<string>());
			foreach (string langCode in langCodes)
			{
				if (localizationManager.TryGetLocalizedTextForLanguage(key2, langCode, null, out var loc))
				{
					string item = loc;
					if (!string.IsNullOrWhiteSpace(loc) && !list.Contains(item))
					{
						list.Add(item);
					}
				}
			}
		}
		return dictionary;
	}
}
