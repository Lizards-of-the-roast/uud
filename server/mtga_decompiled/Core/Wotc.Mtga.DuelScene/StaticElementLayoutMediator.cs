using System;

namespace Wotc.Mtga.DuelScene;

public class StaticElementLayoutMediator : IDisposable
{
	private readonly ISignalListen<CardHolderCreatedSignalArgs> _cardHolderCreatedEvent;

	private readonly ISignalListen<CardHolderDeletedSignalArgs> _cardHolderDeletedEvent;

	private readonly BattleFieldStaticElementsLayout _staticElementsLayout;

	public StaticElementLayoutMediator(ISignalListen<CardHolderCreatedSignalArgs> cardHolderCreatedEvent, ISignalListen<CardHolderDeletedSignalArgs> cardHolderDeletedEvent, BattleFieldStaticElementsLayout staticElementsLayout)
	{
		_cardHolderCreatedEvent = cardHolderCreatedEvent;
		_cardHolderDeletedEvent = cardHolderDeletedEvent;
		_staticElementsLayout = staticElementsLayout;
		_cardHolderCreatedEvent.Listeners += OnCardHolderCreated;
		_cardHolderDeletedEvent.Listeners += OnCardHolderDeleted;
	}

	private void OnCardHolderCreated(CardHolderCreatedSignalArgs args)
	{
		ICardHolder cardHolder = args.CardHolder;
		if (!IgnoreCardHolder(cardHolder))
		{
			_staticElementsLayout.AddCardHolder(cardHolder);
		}
	}

	private bool IgnoreCardHolder(ICardHolder cardHolder)
	{
		if (cardHolder != null && !(cardHolder is GlobalCommandCardHolder))
		{
			return cardHolder is ExileCardHolder;
		}
		return true;
	}

	private void OnCardHolderDeleted(CardHolderDeletedSignalArgs args)
	{
		ICardHolder cardHolder = args.CardHolder;
		if (!IgnoreCardHolder(cardHolder))
		{
			_staticElementsLayout.RemoveCardHolder(cardHolder);
		}
	}

	public void Dispose()
	{
		_cardHolderDeletedEvent.Listeners -= OnCardHolderDeleted;
		_cardHolderCreatedEvent.Listeners -= OnCardHolderCreated;
	}
}
