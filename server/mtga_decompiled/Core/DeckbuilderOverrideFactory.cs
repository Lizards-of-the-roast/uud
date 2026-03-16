using System.Collections.Generic;
using Wotc.Mtga.Cards.Database;

public class DeckbuilderOverrideFactory
{
	private readonly CardSkinDatabase _cardSkinDB;

	private readonly CardDatabase _cardDB;

	private Dictionary<string, DeckBuilderOverride> _cache = new Dictionary<string, DeckBuilderOverride>();

	public DeckbuilderOverrideFactory(CardSkinDatabase cardSkinDatabase, CardDatabase cardDatabase)
	{
		_cardSkinDB = cardSkinDatabase;
		_cardDB = cardDatabase;
	}

	public DeckBuilderOverride DeckBuilderOverrideForSkin(string skin)
	{
		if (_cache.TryGetValue(skin, out var value))
		{
			return value;
		}
		Dictionary<uint, uint> dictionary = new Dictionary<uint, uint>();
		Dictionary<uint, string> dictionary2 = new Dictionary<uint, string>();
		foreach (uint item in _cardSkinDB.CardIdsWithSkin(skin))
		{
			dictionary.Add(item, 4u);
			dictionary2.Add(item, skin);
		}
		DeckBuilderOverride deckBuilderOverride = new DeckBuilderOverride(dictionary, dictionary2);
		_cache[skin] = deckBuilderOverride;
		return deckBuilderOverride;
	}
}
