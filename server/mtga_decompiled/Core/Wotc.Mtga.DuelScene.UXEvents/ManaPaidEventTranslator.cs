using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class ManaPaidEventTranslator : IEventTranslator
{
	private readonly GameManager _gameManager;

	public ManaPaidEventTranslator(GameManager gameManager)
	{
		_gameManager = gameManager;
	}

	public void Translate(IReadOnlyList<GameRulesEvent> allChanges, int changeIndex, MtgGameState oldState, MtgGameState newState, List<UXEvent> events)
	{
		if (allChanges[changeIndex] is ManaPaidEvent manaPaidEvent)
		{
			events.Add(new ManaProducedUXEvent(manaPaidEvent.ProducerId, manaPaidEvent.ConsumerId, manaPaidEvent.Mana, _gameManager));
		}
	}
}
