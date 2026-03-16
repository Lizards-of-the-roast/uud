using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public interface IRevealedCardsProvider
{
	MtgCardInstance GetCardRevealed(uint instanceId);

	ICardHolder GetAssociatedCardHolder(uint instanceId);
}
