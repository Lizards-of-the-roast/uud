using System;
using System.Collections.Generic;
using GreClient.Rules;
using Pooling;

namespace Wotc.Mtga.DuelScene;

public class PlayerHandShakeMediator : IDisposable
{
	private readonly IObjectPool _objectPool;

	private readonly CombatAnimationPlayer _combatAnimationPlayer;

	private readonly ISignalListen<ZoneCardHolderCreatedSignalArgs> _cardHolderCreated;

	private readonly Dictionary<uint, BaseHandCardHolder> _idToHandMap;

	public PlayerHandShakeMediator(IObjectPool objectPool, CombatAnimationPlayer combatAnimationPlayer, ISignalListen<ZoneCardHolderCreatedSignalArgs> cardHolderCreated)
	{
		_objectPool = objectPool;
		_combatAnimationPlayer = combatAnimationPlayer;
		_cardHolderCreated = cardHolderCreated;
		_idToHandMap = _objectPool.PopObject<Dictionary<uint, BaseHandCardHolder>>();
		_combatAnimationPlayer.PlayerDamaged += OnPlayerDamaged;
		_cardHolderCreated.Listeners += OnCardHolderCreated;
	}

	private void OnCardHolderCreated(ZoneCardHolderCreatedSignalArgs args)
	{
		OnCardHolderCreated(args.CardHolder, args.Zone);
	}

	private void OnCardHolderCreated(ICardHolder cardHolder, MtgZone zone)
	{
		if (cardHolder is BaseHandCardHolder value)
		{
			_idToHandMap[zone.OwnerId] = value;
		}
	}

	private void OnPlayerDamaged(uint playerId)
	{
		if (_idToHandMap.TryGetValue(playerId, out var value))
		{
			value.PlayImpactShake();
		}
	}

	public void Dispose()
	{
		_idToHandMap.Clear();
		_objectPool.PushObject(_idToHandMap, tryClear: false);
		_combatAnimationPlayer.PlayerDamaged -= OnPlayerDamaged;
		_cardHolderCreated.Listeners -= OnCardHolderCreated;
	}
}
