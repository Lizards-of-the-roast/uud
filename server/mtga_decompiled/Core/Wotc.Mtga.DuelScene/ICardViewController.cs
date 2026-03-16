using GreClient.CardData;

namespace Wotc.Mtga.DuelScene;

public interface ICardViewController
{
	DuelScene_CDC CreateCardView(ICardDataAdapter cardData);

	DuelScene_CDC UpdateIdForCardView(uint oldId, uint newId);

	void DeleteCard(params uint[] cardIds);
}
