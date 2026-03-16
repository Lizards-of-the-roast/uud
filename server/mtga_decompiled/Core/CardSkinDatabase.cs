using System;
using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Catalog;

public class CardSkinDatabase : IDisposable
{
	private readonly CardSkinCatalog _skinCatalog;

	private readonly CardDatabase _cardDatabase;

	private readonly Dictionary<string, List<uint>> _idToSkinCache = new Dictionary<string, List<uint>>();

	private readonly Dictionary<string, List<uint>> _skinToCardIdCache = new Dictionary<string, List<uint>>();

	public CardSkinDatabase(CardSkinCatalog skinCatalog, CardDatabase cardDatabase)
	{
		_skinCatalog = skinCatalog;
		_cardDatabase = cardDatabase;
	}

	public List<uint> ArtIdsForSkin(string skin)
	{
		if (_idToSkinCache.TryGetValue(skin, out var value))
		{
			return value;
		}
		List<uint> list = new List<uint>();
		foreach (KeyValuePair<string, ArtStyleEntry> item in _skinCatalog)
		{
			ArtStyleEntry value2 = item.Value;
			if (value2.Variant == skin)
			{
				list.Add((uint)value2.ArtId);
			}
		}
		_idToSkinCache[skin] = list;
		return list;
	}

	public List<uint> CardIdsWithSkin(string skin)
	{
		if (_skinToCardIdCache.TryGetValue(skin, out var value))
		{
			return value;
		}
		List<uint> list = new List<uint>();
		foreach (uint item in ArtIdsForSkin(skin))
		{
			CardPrintingData cardPrintingData = _cardDatabase.DatabaseUtilities.GetPrintingsByArtId(item).FirstOrDefault();
			if (cardPrintingData != null)
			{
				list.Add(cardPrintingData.GrpId);
			}
		}
		_skinToCardIdCache[skin] = list;
		return list;
	}

	public void Dispose()
	{
		_idToSkinCache.Clear();
		_skinToCardIdCache.Clear();
	}
}
