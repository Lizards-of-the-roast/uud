using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;

namespace GreClient.Test;

public class MockAltFlavorTextKeyProvider : IAltFlavorTextKeyProvider
{
	private readonly Dictionary<string, string> _flavorTextKeyMap;

	public MockAltFlavorTextKeyProvider(Dictionary<string, string> flavorTextKeyMap)
	{
		_flavorTextKeyMap = flavorTextKeyMap ?? new Dictionary<string, string>();
	}

	public bool TryGetAltFlavorTextKey(ICardDataAdapter cardData, out string flavorTextKey)
	{
		return TryGetAltFlavorTextKey(cardData.ExpansionCode, $"{cardData.Printing.ArtId}_{cardData.SkinCode}", out flavorTextKey);
	}

	public bool TryGetAltFlavorTextKey(string setCode, string artID, out string flavorTextKey)
	{
		flavorTextKey = (_flavorTextKeyMap.TryGetValue(artID, out var value) ? value : null);
		return flavorTextKey != null;
	}
}
