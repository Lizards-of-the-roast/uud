using System;
using System.Collections.Generic;
using System.Linq;
using Wizards.Unification.Models.Cards;
using Wotc.Mtga.Cards.Database;

namespace Core.Shared.Code.Providers;

public class StaticCardNicknames : ICardNicknamesProvider
{
	private static Dictionary<string, uint[]> _allNames;

	private static Dictionary<string, uint[]> LtrNames
	{
		get
		{
			Dictionary<string, uint[]> dictionary = new Dictionary<string, uint[]>();
			dictionary.Add("Fool of a Took", PippinCards);
			dictionary.Add("Mithrandir", GandalfCreatures);
			dictionary.Add("Stormcrow", GandalfCreatures);
			dictionary.Add("The Lord of the Rings", SauronCreatures);
			dictionary.Add("Slinker", new uint[1] { 703868u });
			dictionary.Add("Stinker", new uint[1] { 704082u });
			dictionary.Add("Sharkey", new uint[1] { 703837u });
			dictionary.Add("Treebeard", new uint[1] { 703610u });
			dictionary.Add("Elessar", new uint[1] { 703709u });
			dictionary.Add("Ring-maker", new uint[1] { 703840u });
			return dictionary;
		}
	}

	private static uint[] PippinCards => new uint[2] { 703674u, 703822u };

	private static uint[] GandalfCreatures => new uint[3] { 703208u, 703773u, 704076u };

	private static uint[] SauronCreatures => new uint[3] { 703840u, 703403u, 704113u };

	public void SetData(List<CardNickname> cardNicknames)
	{
	}

	public IEnumerable<uint> GetTitleIdsForNickname(string nickname)
	{
		string key = LocalizationManagerUtils.NormalizeLocalizedText(nickname).ToUpperInvariant();
		if (!AllNames().TryGetValue(key, out var value))
		{
			return Array.Empty<uint>();
		}
		return value;
	}

	private static Dictionary<string, uint[]> AllNames()
	{
		if (_allNames == null)
		{
			IEnumerable<KeyValuePair<string, uint[]>> first = Enumerable.Empty<KeyValuePair<string, uint[]>>();
			first = first.Concat(LtrNames);
			_allNames = first.ToDictionary((KeyValuePair<string, uint[]> kvp) => LocalizationManagerUtils.NormalizeLocalizedText(kvp.Key).ToUpperInvariant(), (KeyValuePair<string, uint[]> kvp) => kvp.Value);
		}
		return _allNames;
	}
}
