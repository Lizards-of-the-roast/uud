using System.Collections.Generic;
using GreClient.Network;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public interface ICardStyleDataProvider
{
	IReadOnlyList<CardStyle> GetCardStylesForDeck(DeckConfig deck);
}
