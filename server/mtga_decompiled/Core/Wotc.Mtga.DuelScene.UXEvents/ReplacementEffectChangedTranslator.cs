using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class ReplacementEffectChangedTranslator : IEventTranslator
{
	private readonly GameManager _gameManager;

	private readonly IReplacementEffectController _replacementEffectController;

	public ReplacementEffectChangedTranslator(GameManager gameManager, IReplacementEffectController replacementEffectController)
	{
		_gameManager = gameManager;
		_replacementEffectController = replacementEffectController ?? NullReplacementEffectController.Default;
	}

	public void Translate(IReadOnlyList<GameRulesEvent> allChanges, int changeIndex, MtgGameState oldState, MtgGameState newState, List<UXEvent> events)
	{
		if (allChanges[changeIndex] is ReplacementEffectChangedEvent replacementEffectChangedEvent)
		{
			if (replacementEffectChangedEvent.Added && !replacementEffectChangedEvent.Removed)
			{
				events.Add(new AddReplacementEffectUXEvent(replacementEffectChangedEvent.EffectData, replacementEffectChangedEvent.Entity, _gameManager, _replacementEffectController));
			}
			else if (!replacementEffectChangedEvent.Added && !replacementEffectChangedEvent.Removed)
			{
				events.Add(new UpdateReplacementEffectUXEvent(replacementEffectChangedEvent.EffectData, replacementEffectChangedEvent.Entity, _gameManager, _replacementEffectController));
			}
			else if (!replacementEffectChangedEvent.Added && replacementEffectChangedEvent.Removed)
			{
				events.Add(new RemoveReplacementEffectUXEvent(replacementEffectChangedEvent.EffectData, replacementEffectChangedEvent.Entity, _gameManager, _replacementEffectController));
			}
		}
	}
}
