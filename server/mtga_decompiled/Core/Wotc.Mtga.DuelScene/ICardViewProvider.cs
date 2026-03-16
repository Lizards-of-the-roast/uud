using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public interface ICardViewProvider
{
	DuelScene_CDC GetCardView(uint cardId);

	IEnumerable<DuelScene_CDC> GetAllCards();

	bool TryGetCardView(uint cardId, out DuelScene_CDC cardView);

	List<DuelScene_CDC> GetCardViews(IEnumerable<uint> ids)
	{
		List<DuelScene_CDC> list = new List<DuelScene_CDC>();
		foreach (uint id in ids)
		{
			if (TryGetCardView(id, out var cardView))
			{
				list.Add(cardView);
			}
		}
		return list;
	}
}
