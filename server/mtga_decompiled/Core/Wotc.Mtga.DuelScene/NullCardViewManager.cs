using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.DuelScene;

public class NullCardViewManager : ICardViewManager, ICardViewProvider, ICardViewController
{
	public static readonly ICardViewManager Default = new NullCardViewManager();

	private static ICardViewProvider Provider => NullCardViewProvider.Default;

	private static ICardViewController Controller => NullCardViewController.Default;

	public uint GetCardUpdatedId(uint id)
	{
		return 0u;
	}

	public uint GetCardPreviousId(uint id)
	{
		return 0u;
	}

	public DuelScene_CDC GetCardView(uint cardId)
	{
		return Provider.GetCardView(cardId);
	}

	public IEnumerable<DuelScene_CDC> GetAllCards()
	{
		return Provider.GetAllCards();
	}

	public bool TryGetCardView(uint cardId, out DuelScene_CDC cardView)
	{
		return Provider.TryGetCardView(cardId, out cardView);
	}

	public DuelScene_CDC CreateCardView(ICardDataAdapter cardData)
	{
		return Controller.CreateCardView(cardData);
	}

	public DuelScene_CDC UpdateIdForCardView(uint oldId, uint newId)
	{
		return Controller.UpdateIdForCardView(oldId, newId);
	}

	public void DeleteCard(params uint[] cardIds)
	{
		Controller.DeleteCard(cardIds);
	}
}
