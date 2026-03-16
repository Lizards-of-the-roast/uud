using System;
using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public class NullCardViewProvider : ICardViewProvider
{
	public static readonly ICardViewProvider Default = new NullCardViewProvider();

	public DuelScene_CDC GetCardView(uint cardId)
	{
		return null;
	}

	public IEnumerable<DuelScene_CDC> GetAllCards()
	{
		return Array.Empty<DuelScene_CDC>();
	}

	public bool TryGetCardView(uint cardId, out DuelScene_CDC cardView)
	{
		cardView = null;
		return false;
	}
}
