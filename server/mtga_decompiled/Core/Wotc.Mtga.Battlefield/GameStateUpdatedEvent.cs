using System;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.DuelScene;

namespace Wotc.Mtga.Battlefield;

public class GameStateUpdatedEvent : MonoBehaviour
{
	private IGameStateProvider _gameStateProvider = NullGameStateProvider.Default;

	public event Action<MtgGameState> GameStateUpdated;

	public void SetGameStateProvider(IGameStateProvider gameStateProvider)
	{
		if (_gameStateProvider != gameStateProvider)
		{
			_gameStateProvider.CurrentGameState.ValueUpdated -= OnGameStateUpdated;
		}
		_gameStateProvider = gameStateProvider;
		_gameStateProvider.CurrentGameState.ValueUpdated += OnGameStateUpdated;
	}

	private void OnGameStateUpdated(MtgGameState gameState)
	{
		this.GameStateUpdated?.Invoke(gameState);
	}

	private void OnDestroy()
	{
		_gameStateProvider.CurrentGameState.ValueUpdated -= OnGameStateUpdated;
	}
}
