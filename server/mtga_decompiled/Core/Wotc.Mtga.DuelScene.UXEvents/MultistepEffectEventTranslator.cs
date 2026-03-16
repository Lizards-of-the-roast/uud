using System;
using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class MultistepEffectEventTranslator : IEventTranslator
{
	private readonly GameManager _gameManager;

	public MultistepEffectEventTranslator(GameManager gameManager)
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
		if (!(allChanges[changeIndex] is MultistepEffectEvent multistepEffectEvent))
		{
			throw new ArgumentException("Event at given index does not implement MultistepEffectEvent");
		}
		UXEvent item = (multistepEffectEvent.IsStarting ? ((MultistepEffectUXEventBase)new MultistepEffectStartedUXEvent(multistepEffectEvent.Affected, multistepEffectEvent.Affector, multistepEffectEvent.AbilityCategory, _gameManager)) : ((MultistepEffectUXEventBase)new MultistepEffectCompletedUXEvent(multistepEffectEvent.Affected, multistepEffectEvent.Affector, multistepEffectEvent.AbilityCategory, _gameManager)));
		events.Add(item);
	}
}
