using GreClient.CardData;

namespace Wotc.Mtga.DuelScene;

public interface IFakeCardViewController
{
	DuelScene_CDC CreateFakeCard(string key, ICardDataAdapter cardData, bool isVisible = false);

	bool DeleteFakeCard(string key);
}
