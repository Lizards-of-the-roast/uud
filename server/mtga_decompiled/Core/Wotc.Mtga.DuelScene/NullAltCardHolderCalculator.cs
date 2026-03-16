using GreClient.CardData;

namespace Wotc.Mtga.DuelScene;

public class NullAltCardHolderCalculator : IAltCardHolderCalculator
{
	public static readonly IAltCardHolderCalculator Default = new NullAltCardHolderCalculator();

	public bool TryGetAltCardHolder(ICardDataAdapter cardData, ICardHolder originalDest, out ICardHolder altDest)
	{
		altDest = null;
		return false;
	}
}
