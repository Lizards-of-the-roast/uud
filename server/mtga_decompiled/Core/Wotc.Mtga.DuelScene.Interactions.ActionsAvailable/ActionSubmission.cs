using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class ActionSubmission : IActionSubmission
{
	private readonly ActionsAvailableRequest _request;

	private readonly DuelSceneLogger _logger;

	private readonly ICardViewProvider _cardViewProvider;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly ICardHolderProvider _cardHolderProvider;

	private readonly ICardMovementController _cardMovementController;

	private readonly IGameplaySettingsProvider _settingsProvider;

	public ActionSubmission(ActionsAvailableRequest request, DuelSceneLogger logger, IContext context)
		: this(request, logger, context.Get<IGameStateProvider>(), context.Get<ICardHolderProvider>(), context.Get<ICardViewProvider>(), context.Get<ICardMovementController>(), context.Get<IGameplaySettingsProvider>())
	{
	}

	private ActionSubmission(ActionsAvailableRequest request, DuelSceneLogger logger, IGameStateProvider gameStateProvider, ICardHolderProvider cardHolderProvider, ICardViewProvider cardViewProvider, ICardMovementController cardMovementController, IGameplaySettingsProvider gameplaySettingsProvider)
	{
		_request = request;
		_logger = logger;
		_cardViewProvider = cardViewProvider;
		_gameStateProvider = gameStateProvider;
		_cardHolderProvider = cardHolderProvider;
		_cardMovementController = cardMovementController;
		_settingsProvider = gameplaySettingsProvider;
	}

	public void SubmitAction(GreInteraction interaction)
	{
		SubmitAction(interaction.GreAction);
	}

	public void SubmitAction(Action action)
	{
		ActionType actionType = action.ActionType;
		MtgGameState mtgGameState = _gameStateProvider.LatestGameState;
		if (actionType == ActionType.Pass)
		{
			_logger?.PriorityPassed();
			_request.SubmitPass();
			return;
		}
		DuelScene_CDC cardView;
		if (action.IsAbilityAction())
		{
			_logger.AbilityUsed(action.AbilityGrpId);
		}
		else if (_cardViewProvider.TryGetCardView(action.InstanceId, out cardView))
		{
			CardHolderType cardHolderType = cardView.CurrentCardHolder.CardHolderType;
			if (cardHolderType == CardHolderType.CardBrowserDefault || cardHolderType == CardHolderType.CardBrowserViewDismiss)
			{
				_cardMovementController.MoveCard(cardView, cardView.Model.Zone);
			}
			if (action.IsPlayAction())
			{
				_cardMovementController.MoveCard(cardView, _cardHolderProvider.GetCardHolderByZoneId(mtgGameState.Battlefield.Id));
			}
			else if (action.IsCastAction())
			{
				_cardMovementController.MoveCard(cardView, _cardHolderProvider.GetCardHolderByZoneId(mtgGameState.Stack.Id));
			}
		}
		_logger.UpdateActionsPerPhaseStep(mtgGameState.CurrentPhase, mtgGameState.CurrentStep);
		_request.SubmitAction(action, _settingsProvider.FullControlDisabled);
	}
}
