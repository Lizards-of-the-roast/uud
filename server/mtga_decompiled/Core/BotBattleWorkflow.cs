using GreClient.Rules;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.Interactions;

public class BotBattleWorkflow : WorkflowBase<BaseUserRequest>
{
	private readonly IHeadlessClientStrategy _strat;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly ICardViewProvider _cardViewProvider;

	private readonly ICardMovementController _cardMovementController;

	public BotBattleWorkflow(IHeadlessClientStrategy strat, IGameStateProvider gameStateProvider, ICardViewProvider cardViewProvider, ICardMovementController cardMovementController, BaseUserRequest request)
		: base(request)
	{
		_strat = strat;
		_gameStateProvider = gameStateProvider;
		_cardViewProvider = cardViewProvider;
		_cardMovementController = cardMovementController;
	}

	protected override void ApplyInteractionInternal()
	{
		if (_request is MulliganRequest)
		{
			foreach (uint cardId in ((MtgGameState)_gameStateProvider.CurrentGameState).LocalHand.CardIds)
			{
				if (_cardViewProvider.TryGetCardView(cardId, out var cardView))
				{
					_cardMovementController.MoveCard(cardView, cardView.Model.Zone);
				}
			}
		}
		_strat.SetGameState(_gameStateProvider.LatestGameState);
		_strat.HandleRequest(_request);
	}
}
