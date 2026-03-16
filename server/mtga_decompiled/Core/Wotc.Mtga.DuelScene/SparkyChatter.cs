using System;
using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtga.DuelScene.UXEvents;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class SparkyChatter : IUpdate, IDisposable
{
	private readonly IClientLocProvider _clientLocProvider;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly IEntityDialogControllerProvider _dialogControllerProvider;

	private readonly UXEventQueue _eventQueue;

	private readonly UIMessageHandler _uiMessageHandler;

	private readonly ChatterConfig _chatterConfig;

	private float _idleTimer;

	private ChatterPair _previousChatterPair;

	private MtgGameState _previousGameState;

	public SparkyChatter(ChatterConfig chatterConfig, IClientLocProvider clientLocProvider, IGameStateProvider gameStateProvider, IEntityDialogControllerProvider dialogControllerProvider, UXEventQueue eventQueue, UIMessageHandler uiMessageHandler)
	{
		_chatterConfig = chatterConfig;
		_clientLocProvider = clientLocProvider;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_dialogControllerProvider = dialogControllerProvider ?? NullEntityDialogControllerProvider.Default;
		_eventQueue = eventQueue ?? new UXEventQueue();
		_uiMessageHandler = uiMessageHandler ?? new UIMessageHandler();
		_gameStateProvider.CurrentGameState.ValueUpdated += OnCurrentGameStateUpdated;
		_uiMessageHandler.EmoteSentCallback += OnEmoteSent;
	}

	private ChatterPair GetRandomChatterPair(IReadOnlyList<ChatterPair> chatterPairs)
	{
		if (chatterPairs == null || chatterPairs.Count == 0)
		{
			return new ChatterPair();
		}
		ChatterPair chatterPair = chatterPairs.SelectRandom();
		while (_previousChatterPair != null && chatterPair?.audioEvent?.WwiseEventName == _previousChatterPair?.audioEvent?.WwiseEventName)
		{
			chatterPair = chatterPairs.SelectRandom();
		}
		_previousChatterPair = chatterPair;
		return chatterPair;
	}

	private bool IsSimilarGameState(MtgGameState gameStateA, MtgGameState gameStateB)
	{
		if (gameStateA == null && gameStateB == null)
		{
			return true;
		}
		if (gameStateA == null || gameStateB == null)
		{
			return false;
		}
		if (gameStateA.CurrentPhase != gameStateB.CurrentPhase || gameStateA.CurrentStep != gameStateB.CurrentStep || gameStateA.Stack.TotalCardCount != gameStateB.Stack.TotalCardCount || gameStateA.LocalHand.TotalCardCount != gameStateB.LocalHand.TotalCardCount || gameStateA.OpponentHand.TotalCardCount != gameStateB.OpponentHand.TotalCardCount)
		{
			return false;
		}
		MtgPlayer decidingPlayer = gameStateA.DecidingPlayer;
		MtgPlayer decidingPlayer2 = gameStateB.DecidingPlayer;
		if (decidingPlayer != null && decidingPlayer2 != null && decidingPlayer.InstanceId != decidingPlayer2.InstanceId)
		{
			return false;
		}
		if (decidingPlayer == null != (decidingPlayer2 == null))
		{
			return false;
		}
		int num = 0;
		int num2 = 0;
		foreach (MtgCardInstance visibleCard in gameStateA.Battlefield.VisibleCards)
		{
			if (visibleCard.Controller == gameStateA.LocalPlayer)
			{
				num++;
			}
			if (visibleCard.Controller == gameStateA.Opponent)
			{
				num++;
			}
		}
		int num3 = 0;
		int num4 = 0;
		foreach (MtgCardInstance visibleCard2 in gameStateB.Battlefield.VisibleCards)
		{
			if (visibleCard2.Controller == gameStateB.LocalPlayer)
			{
				num3++;
			}
			if (visibleCard2.Controller == gameStateB.Opponent)
			{
				num3++;
			}
		}
		if (num == num3)
		{
			return num2 == num4;
		}
		return false;
	}

	public void OnUpdate(float time)
	{
		MtgGameState mtgGameState = _gameStateProvider.CurrentGameState;
		if (mtgGameState == null)
		{
			return;
		}
		if (_eventQueue.IsRunning || mtgGameState.Stage != GameStage.Play)
		{
			_idleTimer = 0f;
			return;
		}
		MtgPlayer decidingPlayer = mtgGameState.DecidingPlayer;
		if (decidingPlayer != null)
		{
			if (_idleTimer > _chatterConfig.IdleTimer && decidingPlayer.IsLocalPlayer)
			{
				PlayChatter(_chatterConfig.IdleChatterOptions.SelectRandom());
			}
			else if (_idleTimer > _chatterConfig.ThinkingTimer && !decidingPlayer.IsLocalPlayer)
			{
				PlayChatter(_chatterConfig.ThinkingChatterOptions.SelectRandom());
			}
			else
			{
				_idleTimer += time;
			}
		}
	}

	private void OnCurrentGameStateUpdated(MtgGameState gameState)
	{
		if (!IsSimilarGameState(gameState, _previousGameState))
		{
			_idleTimer = 0f;
		}
		if (_previousGameState != null)
		{
			IReadOnlyList<ChatterPair> options;
			if (gameState.CurrentPhase != Phase.Combat && gameState.OpponentBattlefieldCards.Count < _previousGameState.OpponentBattlefieldCards.Count)
			{
				PlayChatter(GetRandomChatterPair(_chatterConfig.CreatureDeathChatterOptions));
			}
			else if (_previousGameState.Stage != GameStage.Play && gameState.Stage == GameStage.Play && TryGetGameStartAudio(gameState.LocalHand.TotalCardCount, out options))
			{
				PlayChatter(GetRandomChatterPair(options));
			}
		}
		_previousGameState = gameState;
	}

	private bool TryGetGameStartAudio(uint handCount, out IReadOnlyList<ChatterPair> options)
	{
		foreach (var gameStartChatterOption in _chatterConfig.GameStartChatterOptions)
		{
			if (handCount >= gameStartChatterOption.Item1)
			{
				options = gameStartChatterOption.Item2;
				return true;
			}
		}
		options = null;
		return false;
	}

	private void OnEmoteSent(string messageSent)
	{
		foreach (var emoteReplyChatterOption in _chatterConfig.EmoteReplyChatterOptions)
		{
			if (messageSent.Contains(emoteReplyChatterOption.Item1))
			{
				PlayChatter(GetRandomChatterPair(emoteReplyChatterOption.Item2));
				break;
			}
		}
	}

	private void PlayChatter(ChatterPair chatterPair)
	{
		if (!MDNPlayerPrefs.DisableEmotes && _dialogControllerProvider.TryGetDialogControllerByPlayerType(GREPlayerNum.Opponent, out var dialogController))
		{
			_eventQueue.EnqueuePending(new SparkyChatterUXEvent(_clientLocProvider, dialogController, chatterPair));
			_idleTimer = 0f;
		}
	}

	public void Dispose()
	{
		_uiMessageHandler.EmoteSentCallback -= OnEmoteSent;
		_gameStateProvider.CurrentGameState.ValueUpdated -= OnCurrentGameStateUpdated;
		_previousGameState = null;
	}
}
