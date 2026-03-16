using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class UXEventUpdatePhase : UXEvent
{
	private readonly GameManager _gameManager;

	private readonly ICardViewProvider _cardViewProvider;

	private readonly ITurnController _turnController;

	private readonly CardHolderReference<IBattlefieldCardHolder> _battlefield;

	public Phase Phase { get; }

	public Step Step { get; }

	public UXEventUpdatePhase(Phase phase, Step step, GameManager gameManager, ICardViewProvider cardViewProvider, ITurnController turnController)
	{
		Phase = phase;
		Step = step;
		_gameManager = gameManager;
		_cardViewProvider = cardViewProvider ?? NullCardViewProvider.Default;
		_turnController = turnController ?? NullTurnController.Default;
		_battlefield = CardHolderReference<IBattlefieldCardHolder>.Battlefield(gameManager.CardHolderManager);
	}

	public override void Execute()
	{
		if (Phase != Phase.None)
		{
			if (Phase != Phase.Combat)
			{
				CleanupCombatIcons(_cardViewProvider.GetAllCards());
			}
			_turnController.SetPhase(Phase, Step);
			_gameManager.AutoRespManager.OnPhaseChanged();
			_battlefield.Get().UpdateForPhase(Phase, Step);
		}
		Complete();
	}

	private void CleanupCombatIcons(IEnumerable<DuelScene_CDC> cards)
	{
		foreach (DuelScene_CDC card in cards)
		{
			card.ClearCombatIcons();
		}
	}

	protected override void Cleanup()
	{
		_battlefield.ClearCache();
		base.Cleanup();
	}
}
