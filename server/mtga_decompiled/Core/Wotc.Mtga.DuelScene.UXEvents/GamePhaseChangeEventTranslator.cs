using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class GamePhaseChangeEventTranslator : IEventTranslator
{
	private readonly GameManager _gameManager;

	private readonly ITurnController _turnController;

	private readonly ICardViewProvider _cardViewProvider;

	public GamePhaseChangeEventTranslator(GameManager gameManager, ITurnController turnController, ICardViewProvider cardViewProvider)
	{
		_gameManager = gameManager;
		_turnController = turnController ?? NullTurnController.Default;
		_cardViewProvider = cardViewProvider ?? NullCardViewProvider.Default;
	}

	public void Translate(IReadOnlyList<GameRulesEvent> allChanges, int changeIndex, MtgGameState oldState, MtgGameState newState, List<UXEvent> events)
	{
		if (allChanges[changeIndex] is GamePhaseChangeEvent gamePhaseChangeEvent)
		{
			events.Add(new UXEventUpdatePhase(gamePhaseChangeEvent.Phase, gamePhaseChangeEvent.Step, _gameManager, _cardViewProvider, _turnController));
		}
	}
}
