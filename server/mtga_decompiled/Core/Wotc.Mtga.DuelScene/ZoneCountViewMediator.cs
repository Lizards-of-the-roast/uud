using System;
using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class ZoneCountViewMediator : IDisposable
{
	private readonly IGameStateProvider _gameStateProvider;

	private readonly ISignalListen<ZoneCountCreatedSignalArgs> _zoneCountCreated;

	private readonly ISignalListen<ZoneCountDeletedSignalArgs> _zoneCountDeleted;

	private readonly ISignalListen<CardHolderCreatedSignalArgs> _cardHolderCreatedEvent;

	private readonly ISignalListen<CardHolderDeletedSignalArgs> _cardHolderDeletedEvent;

	private readonly HashSet<(uint ownerId, ZoneCountView view)> _zoneCounts = new HashSet<(uint, ZoneCountView)>();

	private readonly HashSet<IHoverableZone> _subscribed = new HashSet<IHoverableZone>();

	private const string ABILITYWORD_UNDERGROWTH = "Undergrowth";

	private const string ABILITYWORD_COLLECT_EVIDENCE = "CollectEvidenceCount";

	public ZoneCountViewMediator(IGameStateProvider gameStateProvider, ISignalListen<ZoneCountCreatedSignalArgs> zoneCountCreated, ISignalListen<ZoneCountDeletedSignalArgs> zoneCountDeleted, ISignalListen<CardHolderCreatedSignalArgs> cardHolderCreatedEvent, ISignalListen<CardHolderDeletedSignalArgs> cardHolderDeletedEvent)
	{
		_gameStateProvider = gameStateProvider;
		_zoneCountCreated = zoneCountCreated;
		_zoneCountDeleted = zoneCountDeleted;
		_cardHolderCreatedEvent = cardHolderCreatedEvent;
		_cardHolderDeletedEvent = cardHolderDeletedEvent;
		_gameStateProvider.CurrentGameState.ValueUpdated += OnGameStateUpdated;
		_zoneCountCreated.Listeners += OnZoneCountCreated;
		_zoneCountDeleted.Listeners += OnZoneCountDeleted;
		_cardHolderCreatedEvent.Listeners += OnCardHolderCreated;
		_cardHolderDeletedEvent.Listeners += OnCardHolderDeleted;
	}

	private void OnZoneHovered(MtgZone zone)
	{
		uint num = zone?.OwnerId ?? 0;
		ZoneType zoneType = zone?.Type ?? ZoneType.None;
		foreach (var zoneCount in _zoneCounts)
		{
			zoneCount.view.SetVisibility(zone != null);
			if (num == zoneCount.ownerId || zoneType == ZoneType.Exile)
			{
				zoneCount.view.SetHighlights(zoneType);
			}
		}
	}

	private void OnGameStateUpdated(MtgGameState gameState)
	{
		foreach (var zoneCount in _zoneCounts)
		{
			SetZoneCounts(gameState, zoneCount.ownerId, zoneCount.view);
		}
	}

	private void SetZoneCounts(MtgGameState gameState, uint playerId, ZoneCountView zoneCountView)
	{
		zoneCountView.SetLibraryCount(gameState.GetZoneForPlayer(playerId, ZoneType.Library)?.CardIds.Count ?? 0);
		zoneCountView.SetExileCount(GetExileCountForPlayer(playerId, gameState));
		zoneCountView.SetHandCount(gameState.GetZoneForPlayer(playerId, ZoneType.Hand)?.CardIds.Count ?? 0);
		zoneCountView.SetHandSize(GetMaxHandSizeForPlayer(playerId, gameState, out var count), count);
		zoneCountView.SetGraveyardCount(gameState.GetZoneForPlayer(playerId, ZoneType.Graveyard)?.CardIds.Count ?? 0);
		int count2;
		bool abilityWordForPlayer = GetAbilityWordForPlayer(playerId, gameState, "Undergrowth", out count2);
		zoneCountView.SetUndergrowth(abilityWordForPlayer, count2);
		int count3;
		bool abilityWordForPlayer2 = GetAbilityWordForPlayer(playerId, gameState, "CollectEvidenceCount", out count3);
		zoneCountView.SetCollectEvidenceCount(abilityWordForPlayer2 && !abilityWordForPlayer, count3);
	}

	private int GetExileCountForPlayer(uint playerId, MtgGameState gameState)
	{
		int num = 0;
		foreach (MtgCardInstance visibleCard in gameState.Exile.VisibleCards)
		{
			MtgPlayer owner = visibleCard.Owner;
			if (owner != null && owner.InstanceId == playerId)
			{
				num++;
			}
		}
		return num;
	}

	private bool GetMaxHandSizeForPlayer(uint playerId, MtgGameState gameState, out int count)
	{
		if (gameState.TryGetPlayer(playerId, out var player))
		{
			count = (int)player.MaxHandSize;
			return count != 7;
		}
		count = 0;
		return false;
	}

	private bool GetAbilityWordForPlayer(uint playerId, MtgGameState gameState, string abilityWord, out int count)
	{
		if (gameState.PlayerToActiveAbilityWords.TryGetValue(playerId, out var value) && value.TryGetValue(abilityWord, out count))
		{
			return true;
		}
		count = 0;
		return false;
	}

	private void OnZoneCountCreated(ZoneCountCreatedSignalArgs args)
	{
		_zoneCounts.Add((args.PlayerId, args.ZoneCount));
		SetZoneCounts(_gameStateProvider.CurrentGameState, args.PlayerId, args.ZoneCount);
	}

	private void OnZoneCountDeleted(ZoneCountDeletedSignalArgs deletedSignalArgs)
	{
		(uint, ZoneCountView) item = default((uint, ZoneCountView));
		foreach (var zoneCount in _zoneCounts)
		{
			if (!(zoneCount.view != deletedSignalArgs.ZoneCount))
			{
				item = zoneCount;
				break;
			}
		}
		_zoneCounts.Remove(item);
	}

	private void OnCardHolderCreated(CardHolderCreatedSignalArgs createdSignalArgs)
	{
		if (createdSignalArgs.CardHolder is IHoverableZone hoverableZone && _subscribed.Add(hoverableZone))
		{
			hoverableZone.Hovered += OnZoneHovered;
		}
	}

	private void OnCardHolderDeleted(CardHolderDeletedSignalArgs deletedSignalArgs)
	{
		if (deletedSignalArgs.CardHolder is IHoverableZone hoverableZone && _subscribed.Remove(hoverableZone))
		{
			hoverableZone.Hovered -= OnZoneHovered;
		}
	}

	public void Dispose()
	{
		foreach (IHoverableZone item in _subscribed)
		{
			item.Hovered -= OnZoneHovered;
		}
		_subscribed.Clear();
		_zoneCounts.Clear();
		_cardHolderDeletedEvent.Listeners -= OnCardHolderDeleted;
		_cardHolderCreatedEvent.Listeners -= OnCardHolderCreated;
		_zoneCountDeleted.Listeners -= OnZoneCountDeleted;
		_zoneCountCreated.Listeners -= OnZoneCountCreated;
		_gameStateProvider.CurrentGameState.ValueUpdated -= OnGameStateUpdated;
	}
}
