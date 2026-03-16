using System;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.DuelScene;

namespace Wotc.Mtga.Battlefield;

public class GameStatePlaybackCompleteEvent : MonoBehaviour
{
	public Action<MtgGameState> GameStatePlaybackComplete;

	private ISignalListen<GameStatePlaybackCompleteSignalArgs> _signalListener;

	public void SetListener(ISignalListen<GameStatePlaybackCompleteSignalArgs> signalListener)
	{
		ClearListener();
		_signalListener = signalListener;
		if (_signalListener != null)
		{
			_signalListener.Listeners += OnGameStatePlaybackComplete;
		}
	}

	private void OnGameStatePlaybackComplete(GameStatePlaybackCompleteSignalArgs args)
	{
		OnGameStatePlaybackComplete(args.GameState);
	}

	private void OnGameStatePlaybackComplete(MtgGameState gameState)
	{
		GameStatePlaybackComplete?.Invoke(gameState);
	}

	private void ClearListener()
	{
		if (_signalListener != null)
		{
			_signalListener.Listeners -= OnGameStatePlaybackComplete;
			_signalListener = null;
		}
	}

	private void OnDestroy()
	{
		ClearListener();
		GameStatePlaybackComplete = null;
	}
}
