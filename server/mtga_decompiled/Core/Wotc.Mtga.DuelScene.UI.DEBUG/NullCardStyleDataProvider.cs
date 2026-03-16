using System;
using System.Collections.Generic;
using GreClient.Network;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class NullCardStyleDataProvider : ICardStyleDataProvider
{
	public static readonly ICardStyleDataProvider Default = new NullCardStyleDataProvider();

	public IReadOnlyList<CardStyle> GetCardStylesForDeck(DeckConfig deck)
	{
		return Array.Empty<CardStyle>();
	}
}
