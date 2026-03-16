using System;
using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class DesignationUpdatedTranslator : DesignationEventTranslatorBase
{
	public DesignationUpdatedTranslator(IDesignationController designationController, GameManager gameManager)
		: base(designationController, gameManager)
	{
	}

	public override void Translate(IReadOnlyList<GameRulesEvent> allChanges, int changeIndex, MtgGameState oldState, MtgGameState newState, List<UXEvent> events)
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
		if (!(allChanges[changeIndex] is DesignationUpdatedEvent designationUpdatedEvent))
		{
			throw new ArgumentException("Event at given index does not implement DesignationUpdatedEvent");
		}
		events.Add(new UpdateDesignationUXEvent(designationUpdatedEvent.OldDesignation, designationUpdatedEvent.NewDesignation, _designationController, _gameManager));
	}
}
