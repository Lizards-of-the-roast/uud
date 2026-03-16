using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class HandShuffleUxEvent : UXEvent
{
	private readonly ICardHolderProvider _cardHolderProvider;

	public override bool IsBlocking => true;

	public HandShuffleUxEvent(ICardHolderProvider cardHolderProvider)
	{
		_cardHolderProvider = cardHolderProvider;
	}

	public override bool CanExecute(List<UXEvent> currentlyRunningEvents)
	{
		return !currentlyRunningEvents.Exists((UXEvent x) => x.HasWeight);
	}

	public override void Execute()
	{
		if (_cardHolderProvider.TryGetCardHolder(GREPlayerNum.Opponent, CardHolderType.Hand, out OpponentHandCardHolder result))
		{
			result.Shuffle();
		}
		Complete();
	}
}
