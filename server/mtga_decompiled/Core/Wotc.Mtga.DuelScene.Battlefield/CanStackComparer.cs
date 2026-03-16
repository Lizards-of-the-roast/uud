using System;
using System.Collections.Generic;
using Pooling;
using ReferenceMap;
using Wotc.Mtga.DuelScene.Interactions;

namespace Wotc.Mtga.DuelScene.Battlefield;

public class CanStackComparer : IEqualityComparer<DuelScene_CDC>
{
	private readonly IObjectPool _objectPool;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly IWorkflowProvider _workflowProvider;

	private readonly ICardViewProvider _cardViewProvider;

	private readonly MapAggregate _mapAggregate;

	private readonly UIManager _uiManager;

	public CanStackComparer(IContext context, MapAggregate mapAggregate, UIManager uiManager)
		: this(context.Get<IObjectPool>(), context.Get<IGameStateProvider>(), context.Get<IWorkflowProvider>(), context.Get<ICardViewProvider>(), mapAggregate, uiManager)
	{
	}

	public CanStackComparer(IObjectPool objectPool, IGameStateProvider gameStateProvider, IWorkflowProvider workflowProvider, ICardViewProvider cardViewProvider, MapAggregate mapAggregate, UIManager uiManager)
	{
		_objectPool = objectPool;
		_gameStateProvider = gameStateProvider;
		_workflowProvider = workflowProvider;
		_cardViewProvider = cardViewProvider;
		_mapAggregate = mapAggregate;
		_uiManager = uiManager;
	}

	public bool Equals(DuelScene_CDC x, DuelScene_CDC y)
	{
		return CardViewUtilities.CanStack(x, y, _objectPool, _gameStateProvider, _workflowProvider, _cardViewProvider, _mapAggregate, _uiManager);
	}

	public int GetHashCode(DuelScene_CDC obj)
	{
		throw new NotImplementedException();
	}
}
