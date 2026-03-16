using System;
using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class CardDecoratorUpdatedEventTranslator : IEventTranslator
{
	private readonly GameManager _gameManager;

	public CardDecoratorUpdatedEventTranslator(GameManager gameManager)
	{
		_gameManager = gameManager;
	}

	public void Translate(IReadOnlyList<GameRulesEvent> allChanges, int changeIndex, MtgGameState oldState, MtgGameState newState, List<UXEvent> events)
	{
		if (allChanges == null)
		{
			throw new ArgumentNullException("allChanges");
		}
		if (changeIndex < 0 || changeIndex >= allChanges.Count)
		{
			throw new ArgumentOutOfRangeException("changeIndex");
		}
		if (events == null)
		{
			throw new ArgumentNullException("events");
		}
		if (!(allChanges[changeIndex] is CardDecoratorUpdatedEvent cardDecoratorUpdatedEvent))
		{
			throw new ArgumentException("Event at given index does not implement CardDecoratorUpdatedEvent");
		}
		UXEvent item = ((!cardDecoratorUpdatedEvent.Added) ? ((CardDecoratorUXEvent_Base)new RemoveCardDecoratorUXEvent(cardDecoratorUpdatedEvent.AffectorCard, cardDecoratorUpdatedEvent.AffectedCard, cardDecoratorUpdatedEvent.Decorators, cardDecoratorUpdatedEvent.AssociatedPropertyType, _gameManager)) : ((CardDecoratorUXEvent_Base)new AddCardDecoratorUXEvent(cardDecoratorUpdatedEvent.AffectorCard, cardDecoratorUpdatedEvent.AffectedCard, cardDecoratorUpdatedEvent.Decorators, cardDecoratorUpdatedEvent.AssociatedPropertyType, _gameManager)));
		events.Add(item);
	}
}
