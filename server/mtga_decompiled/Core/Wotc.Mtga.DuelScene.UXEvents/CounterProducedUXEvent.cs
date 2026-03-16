using GreClient.Rules;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class CounterProducedUXEvent : UXEvent
{
	private readonly GameManager _gameManager;

	private readonly uint _sourceId;

	private readonly uint _sinkId;

	private readonly CounterType _counterType;

	private readonly CardHolderReference<StackCardHolder> _stack;

	public override bool IsBlocking => true;

	public CounterProducedUXEvent(uint sourceId, uint sinkId, GameManager gameManager, CounterType counterType)
	{
		_gameManager = gameManager;
		_sourceId = sourceId;
		_sinkId = sinkId;
		_counterType = counterType;
		_stack = CardHolderReference<StackCardHolder>.Stack(gameManager.CardHolderManager);
	}

	public override void Execute()
	{
		MtgGameState currentGameState = _gameManager.CurrentGameState;
		MtgEntity entityById = currentGameState.GetEntityById(_sourceId);
		MtgEntity entityById2 = currentGameState.GetEntityById(_sinkId);
		IEntityView entity = _gameManager.ViewManager.GetEntity(_sourceId);
		IEntityView entity2 = _gameManager.ViewManager.GetEntity(_sinkId);
		Transform entityTransform = GetEntityTransform(entity);
		Transform entityTransform2 = GetEntityTransform(entity2);
		if (!entityTransform || !entityTransform2)
		{
			Complete();
		}
		else
		{
			ResourcePayload_Utils.PlayManaAnimation(new MtgMana(new ManaInfo(), 0u), entityTransform.position, entityTransform2.position, base.Complete, _gameManager, entityById, entityById2, _counterType);
		}
	}

	private Transform GetEntityTransform(IEntityView entity)
	{
		if (entity is DuelScene_AvatarView duelScene_AvatarView)
		{
			return duelScene_AvatarView.CounterPoolRoot;
		}
		if (entity is DuelScene_CDC duelScene_CDC)
		{
			MtgZone zone = duelScene_CDC.Model.Zone;
			if (zone != null && zone.Type == ZoneType.Stack)
			{
				return _stack.Get().transform;
			}
			MtgZone zone2 = duelScene_CDC.Model.Zone;
			if (zone2 != null && zone2.Type == ZoneType.Limbo)
			{
				return _gameManager.ViewManager.GetAvatarById(duelScene_CDC.Model.Controller.InstanceId)?.CounterPoolRoot;
			}
			if (_gameManager.ViewManager.TryGetCardView(duelScene_CDC.InstanceId, out var cardView))
			{
				if (cardView.IsVisible)
				{
					return cardView.Root;
				}
				if (_gameManager.ViewManager.TryGetCardView(duelScene_CDC.Model.Parent.InstanceId, out var cardView2) && cardView2.IsVisible)
				{
					return cardView2.Root;
				}
			}
		}
		return null;
	}

	protected override void Cleanup()
	{
		_stack.ClearCache();
		base.Cleanup();
	}
}
