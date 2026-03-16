using System;
using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.Battlefield;

public class LifeRatioUpdatedEvent : MonoBehaviour
{
	[SerializeField]
	private GameStatePlaybackCompleteEvent _playbackCompleteEvent;

	[SerializeField]
	private GameStateUpdatedEvent _gameStateUpdatedEvent;

	[SerializeField]
	private IncrementPlayerLifeEvent _incrementPlayerLifeEvent;

	private MtgGameState _gameState;

	private int _localPlayerLife;

	private uint _localPlayerStartingLife;

	private int _opponentTotalLife;

	private uint _opponentStartingLife;

	private float _previousLifeRatio;

	public event Action<float> LifeRatioUpdated;

	private void Awake()
	{
		_gameStateUpdatedEvent.GameStateUpdated += OnGameStateUpdated;
		GameStatePlaybackCompleteEvent playbackCompleteEvent = _playbackCompleteEvent;
		playbackCompleteEvent.GameStatePlaybackComplete = (Action<MtgGameState>)Delegate.Combine(playbackCompleteEvent.GameStatePlaybackComplete, new Action<MtgGameState>(OnGameStatePlaybackComplete));
		_incrementPlayerLifeEvent.LifeTotalUpdated += OnLifeTotalUpdated;
	}

	private void OnGameStateUpdated(MtgGameState gameState)
	{
		if (_gameState == null)
		{
			_opponentStartingLife = 0u;
			foreach (MtgPlayer player in gameState.Players)
			{
				if (player.IsLocalPlayer)
				{
					_localPlayerLife = player.LifeTotal;
					_localPlayerStartingLife = player.StartingLifeTotal;
				}
				else if (player.ClientPlayerEnum == GREPlayerNum.Opponent)
				{
					_opponentTotalLife += player.LifeTotal;
					_opponentStartingLife += player.StartingLifeTotal;
				}
			}
			DispatchLifeChangeEvent();
		}
		_gameState = gameState;
	}

	private void OnGameStatePlaybackComplete(MtgGameState gameState)
	{
		int localPlayerLife = _localPlayerLife;
		int opponentTotalLife = _opponentTotalLife;
		_opponentTotalLife = 0;
		foreach (MtgPlayer player in gameState.Players)
		{
			if (player.IsLocalPlayer)
			{
				_localPlayerLife = player.LifeTotal;
			}
			else if (player.ClientPlayerEnum == GREPlayerNum.Opponent)
			{
				_opponentTotalLife += player.LifeTotal;
			}
		}
		if (localPlayerLife != _localPlayerLife || opponentTotalLife != _opponentTotalLife)
		{
			DispatchLifeChangeEvent();
		}
	}

	private void OnLifeTotalUpdated(uint playerId, int amount)
	{
		if (_gameState.TryGetPlayer(playerId, out var player))
		{
			int localPlayerLife = _localPlayerLife;
			int opponentTotalLife = _opponentTotalLife;
			if (player.IsLocalPlayer)
			{
				_localPlayerLife += amount;
			}
			else if (player.ClientPlayerEnum == GREPlayerNum.Opponent)
			{
				_opponentTotalLife += amount;
			}
			if (localPlayerLife != _localPlayerLife || opponentTotalLife != _opponentTotalLife)
			{
				DispatchLifeChangeEvent();
			}
		}
	}

	private void DispatchLifeChangeEvent()
	{
		float num = CalculateRatio(_localPlayerLife, _localPlayerStartingLife, _opponentTotalLife, _opponentStartingLife);
		if (!Mathf.Approximately(_previousLifeRatio, num))
		{
			_previousLifeRatio = num;
			this.LifeRatioUpdated?.Invoke(num);
		}
	}

	public static float CalculateRatio(int playerLife, uint startingPlayerLife, int opponentLife, uint startingOpponentLife)
	{
		if (startingPlayerLife == 0 || startingOpponentLife == 0 || (playerLife <= 0 && opponentLife <= 0))
		{
			return 0.5f;
		}
		if (playerLife <= 0)
		{
			return 0f;
		}
		if (opponentLife <= 0)
		{
			return 1f;
		}
		float num = (float)playerLife / (float)startingPlayerLife;
		float num2 = (float)opponentLife / (float)startingOpponentLife;
		return Mathf.Floor(num / (num + num2) * 1000f) / 1000f;
	}

	private void OnDestroy()
	{
		_gameStateUpdatedEvent.GameStateUpdated -= OnGameStateUpdated;
		GameStatePlaybackCompleteEvent playbackCompleteEvent = _playbackCompleteEvent;
		playbackCompleteEvent.GameStatePlaybackComplete = (Action<MtgGameState>)Delegate.Remove(playbackCompleteEvent.GameStatePlaybackComplete, new Action<MtgGameState>(OnGameStatePlaybackComplete));
		_incrementPlayerLifeEvent.LifeTotalUpdated -= OnLifeTotalUpdated;
		_gameState = null;
	}
}
