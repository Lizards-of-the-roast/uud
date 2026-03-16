using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class CountersChangedEventTranslator : IEventTranslator
{
	protected readonly GameManager _gameManager;

	public CountersChangedEventTranslator(GameManager gameManager)
	{
		_gameManager = gameManager;
	}

	public void Translate(IReadOnlyList<GameRulesEvent> allChanges, int changeIndex, MtgGameState oldState, MtgGameState newState, List<UXEvent> events)
	{
		CountersChangedEvent countersChangedEvent = allChanges[changeIndex] as CountersChangedEvent;
		uint affectorId = countersChangedEvent.AffectorId;
		MtgEntity entityById = newState.GetEntityById(affectorId);
		if (entityById == null)
		{
			entityById = oldState.GetEntityById(affectorId);
		}
		CounterType type = countersChangedEvent.Type;
		if (type == CounterType.Poison || type == CounterType.Energy)
		{
			if (countersChangedEvent.Change > 0)
			{
				events.Add(new CounterProducedUXEvent(affectorId, countersChangedEvent.NewInstance.InstanceId, _gameManager, countersChangedEvent.Type));
			}
			else if (countersChangedEvent.Change < 0)
			{
				events.Add(new CounterProducedUXEvent(countersChangedEvent.NewInstance.InstanceId, affectorId, _gameManager, countersChangedEvent.Type));
			}
		}
		events.Add(new CountersChangedUXEvent(affectorId, countersChangedEvent.NewInstance.InstanceId, entityById, countersChangedEvent.NewInstance, countersChangedEvent.OldCount, countersChangedEvent.Change, countersChangedEvent.Type, _gameManager));
	}
}
