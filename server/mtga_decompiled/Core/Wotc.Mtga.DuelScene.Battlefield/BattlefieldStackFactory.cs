using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.Interactions;

namespace Wotc.Mtga.DuelScene.Battlefield;

public class BattlefieldStackFactory : IBattlefieldStackFactory
{
	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly IWorkflowProvider _workflowProvider;

	private readonly ICardViewProvider _cardViewProvider;

	private readonly IEqualityComparer<DuelScene_CDC> _canStackComparer;

	public BattlefieldStackFactory(IContext context, IEqualityComparer<DuelScene_CDC> equalityComparer)
		: this(context.Get<ICardDatabaseAdapter>(), context.Get<IGameStateProvider>(), context.Get<IWorkflowProvider>(), context.Get<ICardViewProvider>(), equalityComparer)
	{
	}

	public BattlefieldStackFactory(ICardDatabaseAdapter cardDatabase, IGameStateProvider gameStateProvider, IWorkflowProvider workflowProvider, ICardViewProvider cardViewProvider, IEqualityComparer<DuelScene_CDC> canStackComparer)
	{
		_cardDatabase = cardDatabase ?? NullCardDatabaseAdapter.Default;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_workflowProvider = workflowProvider ?? NullWorkflowProvider.Default;
		_cardViewProvider = cardViewProvider ?? NullCardViewProvider.Default;
		_canStackComparer = canStackComparer;
	}

	public BattlefieldCardHolder.BattlefieldStack Create(DuelScene_CDC parent, List<DuelScene_CDC> attachmentsAndExiles)
	{
		return new BattlefieldCardHolder.BattlefieldStack(parent, attachmentsAndExiles, _cardDatabase, _gameStateProvider, _workflowProvider, _cardViewProvider, _canStackComparer);
	}

	public BattlefieldCardHolder.BattlefieldStack Create(ICardDataAdapter parentModel, List<DuelScene_CDC> attachmentsAndExiles)
	{
		return new BattlefieldCardHolder.BattlefieldStack(parentModel, attachmentsAndExiles, _cardDatabase, _gameStateProvider, _workflowProvider, _cardViewProvider, _canStackComparer);
	}

	public BattlefieldCardHolder.BattlefieldStack Create(MtgCardInstance parentInstance, List<DuelScene_CDC> attachmentsAndExiles)
	{
		return Create(parentInstance.ToCardData(_cardDatabase), attachmentsAndExiles);
	}
}
