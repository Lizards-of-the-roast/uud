using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Interactions;

public class PayCostsTranslation : IWorkflowTranslation<PayCostsRequest>
{
	private readonly GameManager _gameManager;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly ICardViewProvider _cardViewProvider;

	private readonly IWorkflowTranslator _workflowTranslator;

	public PayCostsTranslation(IGameStateProvider gameStateProvider, ICardViewProvider cardViewProvider, IWorkflowTranslator workflowTranslator, GameManager gameManager)
	{
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_cardViewProvider = cardViewProvider ?? NullCardViewProvider.Default;
		_workflowTranslator = workflowTranslator;
		_gameManager = gameManager;
	}

	public WorkflowBase Translate(PayCostsRequest req)
	{
		return new PayCostWorkflow(req, _gameStateProvider, _cardViewProvider, _gameManager.InteractionSystem, _gameManager.UIMessageHandler, _gameManager.UIManager, _workflowTranslator);
	}
}
