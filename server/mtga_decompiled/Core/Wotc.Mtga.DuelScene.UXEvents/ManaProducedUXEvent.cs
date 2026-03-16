using GreClient.Rules;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class ManaProducedUXEvent : UXEvent
{
	private readonly uint _sourceId;

	public readonly uint _sinkId;

	private readonly MtgMana _mana;

	private readonly GameManager _gameManager;

	private readonly EntityViewManager _viewManager;

	private readonly CardHolderReference<StackCardHolder> _stack;

	private readonly IGameStateProvider _gameStateProvider;

	private string _manaRiderCache;

	public bool Blocking { get; set; }

	public override bool IsBlocking => Blocking;

	private string _manaRiderKey
	{
		get
		{
			string text = _manaRiderCache;
			if (text == null)
			{
				string obj = _mana.GetManaRiderKey() ?? string.Empty;
				string text2 = obj;
				_manaRiderCache = obj;
				text = text2;
			}
			return text;
		}
	}

	public ManaProducedUXEvent(uint sourceId, uint sinkId, MtgMana mana, GameManager gameManager)
		: this(sourceId, sinkId, mana)
	{
		_gameManager = gameManager;
		_viewManager = gameManager.ViewManager;
		_stack = CardHolderReference<StackCardHolder>.Stack(gameManager.CardHolderManager);
		_gameStateProvider = gameManager.Context.Get<IGameStateProvider>();
	}

	public ManaProducedUXEvent(uint sourceId, uint sinkId, MtgMana mana)
	{
		_sourceId = sourceId;
		_sinkId = sinkId;
		_mana = mana;
	}

	public override void Execute()
	{
		MtgGameState mtgGameState = _gameStateProvider.CurrentGameState;
		MtgEntity entityById = mtgGameState.GetEntityById(_sourceId);
		MtgEntity entityById2 = mtgGameState.GetEntityById(_sinkId);
		Transform sourceTransform = GetSourceTransform(entityById);
		Transform sinkPosition = GetSinkPosition(entityById2, _mana);
		if (sourceTransform == null || sinkPosition == null)
		{
			Complete();
		}
		else
		{
			ResourcePayload_Utils.PlayManaAnimation(_mana, sourceTransform.position, sinkPosition.position, base.Complete, _gameManager, entityById, entityById2, CounterType.None);
		}
	}

	private Transform GetSourceTransform(MtgEntity source)
	{
		if (source is MtgPlayer mtgPlayer)
		{
			return _viewManager.GetAvatarById(mtgPlayer.InstanceId).transform;
		}
		if (source is MtgCardInstance mtgCardInstance)
		{
			if (_viewManager.TryGetCardView(source.InstanceId, out var cardView))
			{
				if (cardView.CurrentCardHolder.CardHolderType == CardHolderType.Battlefield)
				{
					IBattlefieldStack battlefieldStack = (cardView.CurrentCardHolder as IBattlefieldCardHolder)?.GetStackForCard(cardView);
					if (battlefieldStack != null && battlefieldStack.StackParent != null)
					{
						cardView = battlefieldStack.StackParent;
					}
				}
				if (cardView.IsVisible)
				{
					return cardView.Root;
				}
				if (mtgCardInstance.ParentId != 0 && _viewManager.TryGetCardView(mtgCardInstance.ParentId, out var cardView2))
				{
					return cardView2.Root;
				}
				return null;
			}
			if (mtgCardInstance.Zone != null && mtgCardInstance.Zone.Type == ZoneType.Limbo && mtgCardInstance.Controller != null && mtgCardInstance.Controller.ClientPlayerEnum != GREPlayerNum.Invalid)
			{
				return _viewManager.GetAvatarById(mtgCardInstance.Controller.InstanceId).transform;
			}
			return _viewManager.GetAvatarByPlayerSide(GREPlayerNum.LocalPlayer).transform;
		}
		return _viewManager.GetAvatarByPlayerSide(GREPlayerNum.LocalPlayer).transform;
	}

	private Transform GetSinkPosition(MtgEntity sink, MtgMana mana)
	{
		MtgPlayer mtgPlayer = sink as MtgPlayer;
		MtgCardInstance mtgCardInstance = sink as MtgCardInstance;
		if (mtgPlayer != null)
		{
			return _viewManager.GetAvatarById(mtgPlayer.InstanceId).GetTransformForManaButton(mana);
		}
		if (mtgCardInstance != null)
		{
			if (!_viewManager.TryGetCardView(sink.InstanceId, out var cardView) || (mtgCardInstance.Zone != null && mtgCardInstance.Zone.Type == ZoneType.Stack))
			{
				return _stack.Get().transform;
			}
			if (cardView.IsVisible)
			{
				return cardView.Root;
			}
			if (mtgCardInstance.ParentId != 0 && _viewManager.TryGetCardView(mtgCardInstance.ParentId, out var cardView2))
			{
				return cardView2.Root;
			}
		}
		return null;
	}

	public bool IsSame(UXEvent evt)
	{
		if (evt is ManaProducedUXEvent manaProducedUXEvent && CanGroupWith(manaProducedUXEvent))
		{
			return _mana.Color == manaProducedUXEvent._mana.Color;
		}
		return false;
	}

	public bool CanGroupWith(ManaProducedUXEvent otherManaProduced)
	{
		if (_sourceId == otherManaProduced._sourceId && _sinkId == otherManaProduced._sinkId)
		{
			return _manaRiderKey == otherManaProduced._manaRiderKey;
		}
		return false;
	}

	protected override void Cleanup()
	{
		_stack.ClearCache();
		base.Cleanup();
	}

	public override string ToString()
	{
		return string.Format("{0}: Source: {1}, Sink: {2}, Color: {3}, ManaRider: {4}", "ManaProducedUXEvent", _sourceId, _sinkId, _mana.Color, _manaRiderKey);
	}
}
