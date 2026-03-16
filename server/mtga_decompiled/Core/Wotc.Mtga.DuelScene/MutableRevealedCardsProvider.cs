using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class MutableRevealedCardsProvider : IRevealedCardsProvider
{
	public readonly Dictionary<uint, MtgCardInstance> RevealedCards = new Dictionary<uint, MtgCardInstance>();

	public readonly Dictionary<uint, OpponentHandCardHolder> CardHolderMap = new Dictionary<uint, OpponentHandCardHolder>();

	public MtgCardInstance GetCardRevealed(uint instanceId)
	{
		if (!RevealedCards.TryGetValue(instanceId, out var value))
		{
			return null;
		}
		return value;
	}

	public ICardHolder GetAssociatedCardHolder(uint instanceId)
	{
		if (!CardHolderMap.TryGetValue(instanceId, out var value))
		{
			return null;
		}
		return value;
	}
}
