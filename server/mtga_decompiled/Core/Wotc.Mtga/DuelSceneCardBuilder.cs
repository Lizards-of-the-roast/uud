using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.DuelScene.VFX;

namespace Wotc.Mtga;

public class DuelSceneCardBuilder : ICardBuilder<DuelScene_CDC>
{
	private readonly CardViewBuilder _cardBuilder;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly IWorkflowProvider _workflowProvider;

	private readonly IVfxProvider _vfxProvider;

	private readonly IEntityNameProvider<uint> _entityNameProvider;

	public DuelSceneCardBuilder(CardViewBuilder cardViewBuilder, IGameStateProvider gameStateProvider, IWorkflowProvider workflowProvider, IVfxProvider vfxProvider, IEntityNameProvider<uint> entityNameProvider)
	{
		_cardBuilder = cardViewBuilder;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_workflowProvider = workflowProvider ?? NullWorkflowProvider.Default;
		_vfxProvider = vfxProvider ?? NullVfxProvider.Default;
		_entityNameProvider = entityNameProvider ?? NullIdNameProvider.Default;
	}

	private MtgGameState GetGameState()
	{
		return _gameStateProvider.CurrentGameState;
	}

	private WorkflowBase GetInteraction()
	{
		return _workflowProvider.GetCurrentWorkflow();
	}

	public DuelScene_CDC CreateCDC(ICardDataAdapter cardData, bool isVisible = false)
	{
		return _cardBuilder.CreateDuelSceneCdc(cardData, GetGameState, GetInteraction, _vfxProvider, _entityNameProvider, isVisible);
	}

	public void DestroyCDC(DuelScene_CDC cdc)
	{
		_cardBuilder.DestroyCDC(cdc);
	}
}
