using GreClient.Rules;
using Pooling;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.DuelScene.Interactions;

public class DeclareAttackersTranslation_NPE : IWorkflowTranslation<DeclareAttackerRequest>
{
	private readonly NPEDirector _npeDirector;

	private readonly IObjectPool _objPool;

	private readonly IPromptEngine _promptEngine;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly ICardViewProvider _cardViewProvider;

	private readonly ICardHolderProvider _cardHolderProvider;

	private readonly UIManager _uiManager;

	private readonly GameManager _gameManager;

	public DeclareAttackersTranslation_NPE(NPEDirector npeDirector, IContext context, UIManager uiManager, GameManager gameManager)
		: this(npeDirector, context.Get<IObjectPool>(), context.Get<IPromptEngine>(), context.Get<IGameStateProvider>(), context.Get<ICardViewProvider>(), context.Get<ICardHolderProvider>(), uiManager, gameManager)
	{
	}

	private DeclareAttackersTranslation_NPE(NPEDirector npeDirector, IObjectPool objPool, IPromptEngine promptEngine, IGameStateProvider gameStateProvider, ICardViewProvider cardViewProvider, ICardHolderProvider cardHolderProvider, UIManager uiManager, GameManager gameManager)
	{
		_npeDirector = npeDirector;
		_objPool = objPool ?? NullObjectPool.Default;
		_promptEngine = promptEngine ?? NullPromptEngine.Default;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_cardViewProvider = cardViewProvider ?? NullCardViewProvider.Default;
		_cardHolderProvider = cardHolderProvider ?? NullCardHolderProvider.Default;
		_uiManager = uiManager;
		_gameManager = gameManager;
	}

	public WorkflowBase Translate(DeclareAttackerRequest req)
	{
		return new DeclareAttackersWorkflow_NPE(req, _npeDirector, _objPool, _promptEngine, _gameStateProvider, _cardViewProvider, GetBattlefield(), _uiManager, _gameManager);
	}

	private IBattlefieldCardHolder GetBattlefield()
	{
		MtgGameState mtgGameState = _gameStateProvider.LatestGameState;
		if (mtgGameState == null)
		{
			return null;
		}
		if (mtgGameState.Battlefield == null)
		{
			return null;
		}
		return _cardHolderProvider.GetCardHolder(GREPlayerNum.Invalid, CardHolderType.Battlefield) as IBattlefieldCardHolder;
	}
}
