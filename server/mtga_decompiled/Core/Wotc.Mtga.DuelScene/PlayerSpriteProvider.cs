using System;
using AssetLookupTree;
using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public class PlayerSpriteProvider : IPlayerSpriteProvider, IDisposable
{
	private readonly AssetLoader.AssetTracker<Sprite> _spriteTracker = new AssetLoader.AssetTracker<Sprite>("PortraitSpriteTracker");

	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly IPlayerInfoProvider _playerInfoProvider;

	public PlayerSpriteProvider(AssetLookupSystem assetLookupSystem, IGameStateProvider gameStateProvider, IPlayerInfoProvider playerInfoProvider)
	{
		_assetLookupSystem = assetLookupSystem;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_playerInfoProvider = playerInfoProvider ?? NullPlayerInfoProvider.Default;
	}

	public Sprite GetPlayerSprite(uint playerId)
	{
		if (!_gameStateProvider.CurrentGameState.Value.TryGetPlayer(playerId, out var player))
		{
			return null;
		}
		return GetPlayerSprite(player);
	}

	private Sprite GetPlayerSprite(MtgPlayer player)
	{
		_spriteTracker.Cleanup();
		string avatarSelectionForPlayer = _playerInfoProvider.GetAvatarSelectionForPlayer(player.InstanceId);
		string avatarBustImagePath = ProfileUtilities.GetAvatarBustImagePath(_assetLookupSystem, avatarSelectionForPlayer);
		return _spriteTracker.Acquire(avatarBustImagePath);
	}

	public void Dispose()
	{
		_spriteTracker.Cleanup();
	}
}
