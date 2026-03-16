using GreClient.CardData;

namespace Wotc.Mtga.DuelScene;

public interface IAltCardHolderCalculator
{
	bool TryGetAltCardHolder(ICardDataAdapter cardData, ICardHolder originalDest, out ICardHolder altDest);
}
