using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class ChoiceResultEventTranslator : IEventTranslator
{
	private readonly GameManager _gameManager;

	private readonly ICardViewProvider _cardViewProvider;

	public ChoiceResultEventTranslator(GameManager gameManager)
	{
		_gameManager = gameManager;
		_cardViewProvider = gameManager.ViewManager;
	}

	public void Translate(IReadOnlyList<GameRulesEvent> allChanges, int changeIndex, MtgGameState oldState, MtgGameState newState, List<UXEvent> events)
	{
		UXEvent uXEvent = Translate_Internal(allChanges[changeIndex] as ChoiceResultEvent);
		if (uXEvent != null)
		{
			events.Add(uXEvent);
		}
	}

	private UXEvent Translate_Internal(ChoiceResultEvent cre)
	{
		if (cre == null)
		{
			return null;
		}
		if (cre.IsRandom)
		{
			return new ChoiceResultUXEvent_Random(cre, _cardViewProvider);
		}
		if (cre.IsPermanent)
		{
			return new ChoiceResultUXEvent_Permanent(cre, _gameManager);
		}
		return new ChoiceResultUXEvent_General(cre, _gameManager);
	}
}
