using System.Collections.Generic;
using Core.Meta.Cards;
using Wizards.Mtga.Decks;

namespace Core.Meta;

public static class DeckImportMetadata
{
	public static readonly List<(string, string)> LocalizedKeyMappings = new List<(string, string)> { ("Name", "MainNav/DeckImporter/Metadata/Name") };

	public const string Metadata = "Metadata";

	public const string DeckName = "Name";

	public static void TryImportMetadata(List<string> lines, Dictionary<string, List<string>> validKeyNames, Client_Deck deck)
	{
		Dictionary<string, string> reverseKeyLookup = DeckImportParser.CreateReverseLocLookup(validKeyNames);
		foreach (string line in lines)
		{
			ParseLine(line, reverseKeyLookup, deck);
		}
	}

	private static void ParseLine(string line, Dictionary<string, string> reverseKeyLookup, Client_Deck deck)
	{
		if (TryParseLine(line, out (string, string) valueTuple) && reverseKeyLookup.TryGetValue(valueTuple.Item1, out var value) && value == "Name")
		{
			UpdateDeckName(deck, valueTuple.Item2);
		}
	}

	private static void UpdateDeckName(Client_Deck deck, string name)
	{
		deck.Summary.Name = name;
	}

	private static bool TryParseLine(string line, out (string Key, string Value) valueTuple)
	{
		string[] array = line.Split(' ', 2);
		if (array.Length != 2)
		{
			valueTuple = default((string, string));
			return false;
		}
		valueTuple = (Key: array[0].Trim(), Value: array[1].Trim());
		return true;
	}
}
