using System;
using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class CardChangedEventTranslator : IEventTranslator
{
	private readonly GameManager _gameManager;

	public CardChangedEventTranslator(GameManager gameManager)
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
		if (!(allChanges[changeIndex] is CardChangedEvent cardChangedEvent))
		{
			throw new ArgumentException("Event at given index does not implement CardChangedEvent");
		}
		if (((cardChangedEvent.OldInstance != null && cardChangedEvent.OldInstance.Zone.Type != ZoneType.Limbo) || (cardChangedEvent.NewInstance != null && cardChangedEvent.NewInstance.Zone.Type != ZoneType.Limbo)) && cardChangedEvent.OldInstance?.MutationParent == null)
		{
			UXEvent item = new UpdateCardModelUXEvent(_gameManager, cardChangedEvent);
			events.Add(item);
			if (cardChangedEvent.Property == PropertyType.DieRollResult)
			{
				events.Add(new WaitForSecondsUXEvent(0.75f));
			}
		}
	}
}
