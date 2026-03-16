using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public class MutableCardViewProvider : ICardViewProvider
{
	public readonly List<DuelScene_CDC> AllCards = new List<DuelScene_CDC>();

	public readonly Dictionary<uint, DuelScene_CDC> CardViews = new Dictionary<uint, DuelScene_CDC>();

	public DuelScene_CDC GetCardView(uint cardId)
	{
		if (!CardViews.TryGetValue(cardId, out var value))
		{
			return null;
		}
		return value;
	}

	public bool TryGetCardView(uint cardId, out DuelScene_CDC cardView)
	{
		cardView = GetCardView(cardId);
		return cardView != null;
	}

	public IEnumerable<DuelScene_CDC> GetAllCards()
	{
		return AllCards;
	}
}
