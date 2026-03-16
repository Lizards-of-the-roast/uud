using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class DisqualifiedEffectEventTranslator : IEventTranslator
{
	private readonly GameManager _gameManager;

	private readonly IGameEffectController _gameEffectController;

	private readonly ICardViewProvider _cardViewProvider;

	public DisqualifiedEffectEventTranslator(GameManager gameManager, IContext context)
	{
		_gameManager = gameManager;
		_cardViewProvider = context.Get<ICardViewProvider>();
		_gameEffectController = context.Get<IGameEffectController>();
	}

	public void Translate(IReadOnlyList<GameRulesEvent> allChanges, int changeIndex, MtgGameState oldState, MtgGameState newState, List<UXEvent> events)
	{
		if (allChanges[changeIndex] is DisqualifiedEffectEvent disqualifiedEffectEvent)
		{
			MtgCardInstance disqualifierCard = GetDisqualifierCard(newState, disqualifiedEffectEvent.AffectorId);
			if (disqualifierCard != null)
			{
				events.Add(new DisqualifiedEffectUXEvent(disqualifierCard, _cardViewProvider, _gameEffectController, _gameManager));
			}
		}
	}

	private MtgCardInstance GetDisqualifierCard(MtgGameState gameState, uint id)
	{
		if (gameState.TryGetCard(id, out var card))
		{
			return card;
		}
		if (gameState.TrackedHistoricCards.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}
}
