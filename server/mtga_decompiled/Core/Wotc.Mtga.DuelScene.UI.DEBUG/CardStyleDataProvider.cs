using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Network;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class CardStyleDataProvider : ICardStyleDataProvider
{
	private readonly ICardDataProvider _cardDataProvider;

	private readonly IGreLocProvider _locProvider;

	private readonly Dictionary<DeckConfig, IReadOnlyList<CardStyle>> _cache = new Dictionary<DeckConfig, IReadOnlyList<CardStyle>>();

	public CardStyleDataProvider(ICardDataProvider cardDataProvider, IGreLocProvider locProvider)
	{
		_cardDataProvider = cardDataProvider ?? NullCardDataProvider.Default;
		_locProvider = locProvider ?? NullGreLocManager.Default;
	}

	public IReadOnlyList<CardStyle> GetCardStylesForDeck(DeckConfig deck)
	{
		if (_cache.TryGetValue(deck, out var value))
		{
			return value;
		}
		HashSet<uint> hashSet = new HashSet<uint>();
		hashSet.UnionWith(deck.Deck);
		hashSet.UnionWith(deck.Sideboard);
		hashSet.UnionWith(deck.Commander);
		if (deck.Companion != 0)
		{
			hashSet.Add(deck.Companion);
		}
		List<CardStyle> list = new List<CardStyle>(StylesForIds(hashSet));
		list.Sort(delegate(CardStyle x, CardStyle y)
		{
			int num = x.Description.CompareTo(y.Description);
			if (num != 0)
			{
				return num;
			}
			num = x.GrpId.CompareTo(y.GrpId);
			if (num != 0)
			{
				return num;
			}
			num = x.Style.CompareTo(y.Style);
			return (num != 0) ? num : 0;
		});
		return _cache[deck] = list;
	}

	private IEnumerable<CardStyle> StylesForIds(IEnumerable<uint> grpIds)
	{
		foreach (uint grpId in grpIds)
		{
			CardPrintingData cardPrinting = _cardDataProvider.GetCardPrintingById(grpId);
			if (cardPrinting == null)
			{
				continue;
			}
			foreach (string knownSupportedStyle in cardPrinting.KnownSupportedStyles)
			{
				string description = _locProvider.GetLocalizedText(cardPrinting.TitleId, "en-US") + " (" + knownSupportedStyle + ")";
				yield return new CardStyle(grpId, knownSupportedStyle, description);
			}
		}
	}
}
