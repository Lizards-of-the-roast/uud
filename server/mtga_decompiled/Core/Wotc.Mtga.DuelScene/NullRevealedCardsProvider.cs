using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullRevealedCardsProvider : IRevealedCardsProvider
{
	public static readonly IRevealedCardsProvider Default = new NullRevealedCardsProvider();

	public MtgCardInstance GetCardRevealed(uint instanceId)
	{
		return null;
	}

	public ICardHolder GetAssociatedCardHolder(uint instanceId)
	{
		return null;
	}
}
