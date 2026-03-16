using System;
using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class DesignationDeletedTranslator : DesignationEventTranslatorBase
{
	public DesignationDeletedTranslator(IDesignationController designationController, GameManager gameManager)
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
		if (!(allChanges[changeIndex] is DesignationDeletedEvent designationDeletedEvent))
		{
			throw new ArgumentException("Event at given index does not implement DesignationDeletedEvent");
		}
		events.Add(new RemoveDesignationUXEvent(designationDeletedEvent.Designation, _designationController, _gameManager));
	}
}
