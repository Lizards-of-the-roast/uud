namespace Wotc.Mtga.DuelScene;

public interface IRevealedCardsManager : IRevealedCardsProvider, IRevealedCardsController
{
	bool TryGetAssociatedCardHolder<T>(uint instanceId, out T cardHolder) where T : ICardHolder
	{
		cardHolder = (T)GetAssociatedCardHolder(instanceId);
		return cardHolder != null;
	}
}
