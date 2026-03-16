using GreClient.CardData;

namespace Wotc.Mtga.DuelScene;

public class NullCardViewController : ICardViewController
{
	public static readonly ICardViewController Default = new NullCardViewController();

	public DuelScene_CDC CreateCardView(ICardDataAdapter cardData)
	{
		return null;
	}

	public DuelScene_CDC UpdateIdForCardView(uint oldId, uint newId)
	{
		return null;
	}

	public void DeleteCard(params uint[] cardIds)
	{
	}
}
