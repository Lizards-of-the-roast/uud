using System;
using System.Collections.Generic;
using Pooling;

namespace Wotc.Mtga.DuelScene;

public class CompanionEmoteMediator : IDisposable
{
	private readonly IObjectPool _objectPool;

	private readonly IEntityDialogControllerProvider _dialogControllerProvider;

	private readonly ISignalListen<PlayerCreatedSignalArgs> _playerCreatedEvent;

	private readonly ISignalListen<CompanionCreatedSignalArgs> _companionCreatedEvent;

	private readonly List<uint> _playerIds;

	private readonly Dictionary<uint, AccessoryController> _companions;

	public CompanionEmoteMediator(IObjectPool objectPool, IEntityDialogControllerProvider dialogControllerProvider, ISignalListen<PlayerCreatedSignalArgs> playerCreatedEvent, ISignalListen<CompanionCreatedSignalArgs> companionCreatedEvent)
	{
		_objectPool = objectPool;
		_dialogControllerProvider = dialogControllerProvider;
		_playerCreatedEvent = playerCreatedEvent;
		_companionCreatedEvent = companionCreatedEvent;
		_playerCreatedEvent.Listeners += OnPlayerCreated;
		_companionCreatedEvent.Listeners += OnCompanionCreated;
		_playerIds = _objectPool.PopObject<List<uint>>();
		_companions = _objectPool.PopObject<Dictionary<uint, AccessoryController>>();
	}

	private void OnPlayerCreated(PlayerCreatedSignalArgs args)
	{
		OnPlayerCreated(args.Player);
	}

	private void OnPlayerCreated(DuelScene_AvatarView avatar)
	{
		_playerIds.Add(avatar.InstanceId);
	}

	private void OnCompanionCreated(CompanionCreatedSignalArgs args)
	{
		OnCompanionCreated(args.PlayerId, args.Companion);
	}

	private void OnCompanionCreated(uint playerId, AccessoryController companion)
	{
		if (_dialogControllerProvider.TryGetDialogControllerById(playerId, out var dialogController))
		{
			dialogController.EmotePresented += companion.HandleLocalEmote;
			foreach (KeyValuePair<uint, AccessoryController> companion2 in _companions)
			{
				dialogController.EmotePresented += companion2.Value.HandleOpponentEmote;
			}
		}
		foreach (uint playerId2 in _playerIds)
		{
			if (_dialogControllerProvider.TryGetDialogControllerById(playerId2, out var dialogController2))
			{
				dialogController2.EmotePresented += companion.HandleOpponentEmote;
			}
		}
		_playerIds.Add(playerId);
		_companions[playerId] = companion;
	}

	public void Dispose()
	{
		_companionCreatedEvent.Listeners -= OnCompanionCreated;
		_playerCreatedEvent.Listeners -= OnPlayerCreated;
		foreach (uint playerId in _playerIds)
		{
			if (!_dialogControllerProvider.TryGetDialogControllerById(playerId, out var dialogController))
			{
				continue;
			}
			foreach (KeyValuePair<uint, AccessoryController> companion in _companions)
			{
				uint key = companion.Key;
				AccessoryController value = companion.Value;
				if (playerId == key)
				{
					dialogController.EmotePresented -= value.HandleLocalEmote;
				}
				else
				{
					dialogController.EmotePresented -= value.HandleOpponentEmote;
				}
			}
		}
		_playerIds.Clear();
		_objectPool.PushObject(_playerIds, tryClear: false);
		_companions.Clear();
		_objectPool.PushObject(_companions, tryClear: false);
	}
}
