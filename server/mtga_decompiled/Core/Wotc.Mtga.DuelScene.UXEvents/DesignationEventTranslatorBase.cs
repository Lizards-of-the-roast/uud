using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.UXEvents;

public abstract class DesignationEventTranslatorBase : IEventTranslator
{
	protected readonly IDesignationController _designationController;

	protected readonly GameManager _gameManager;

	public DesignationEventTranslatorBase(IDesignationController designationController, GameManager gameManager)
	{
		_designationController = designationController ?? NullDesignationController.Default;
		_gameManager = gameManager;
	}

	public abstract void Translate(IReadOnlyList<GameRulesEvent> allChanges, int changeIndex, MtgGameState oldState, MtgGameState newState, List<UXEvent> events);
}
