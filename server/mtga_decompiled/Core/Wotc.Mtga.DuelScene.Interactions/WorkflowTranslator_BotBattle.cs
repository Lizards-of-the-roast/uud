using AssetLookupTree;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Interactions;

public class WorkflowTranslator_BotBattle : IWorkflowTranslator
{
	private readonly IHeadlessClientStrategy _strat;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly ICardViewProvider _cardViewProvider;

	private readonly ICardMovementController _cardMovementController;

	private readonly IWorkflowTranslation<IntermissionRequest> _intermissionTranslation;

	public WorkflowTranslator_BotBattle(IContext context, IHeadlessClientStrategy strat, AssetLookupSystem assetLookupSystem)
		: this(strat, context.Get<IGameStateProvider>(), context.Get<ICardViewProvider>(), context.Get<ICardMovementController>(), new IntermissionTranslation(context, assetLookupSystem))
	{
	}

	private WorkflowTranslator_BotBattle(IHeadlessClientStrategy strat, IGameStateProvider gameStateProvider, ICardViewProvider cardViewProvider, ICardMovementController cardMovementController, IWorkflowTranslation<IntermissionRequest> intermissionTranslation)
	{
		_strat = strat;
		_gameStateProvider = gameStateProvider;
		_cardViewProvider = cardViewProvider;
		_cardMovementController = cardMovementController;
		_intermissionTranslation = intermissionTranslation;
	}

	public WorkflowBase Translate(BaseUserRequest req)
	{
		if (req is IntermissionRequest req2)
		{
			return _intermissionTranslation.Translate(req2);
		}
		return new BotBattleWorkflow(_strat, _gameStateProvider, _cardViewProvider, _cardMovementController, req);
	}
}
