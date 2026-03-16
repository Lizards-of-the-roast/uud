using System;
using GreClient.Rules;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.DuelScene.Interactions.Mulligan;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class GameStartController : IGameStartController, IDisposable
{
	public enum GameStage
	{
		None,
		Start,
		Start_StartingPlayer,
		Start_OpeningHand,
		Playing
	}

	private readonly BrowserManager _browserManager;

	private readonly IGameStateProvider _gameStateProvider;

	private IBrowser _openedBrowser;

	public GameStartController(IGameStateProvider gameStateProvider, BrowserManager browserManager)
	{
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_browserManager = browserManager;
		_gameStateProvider.CurrentGameState.ValueUpdated += UpdateCurrentGameState;
	}

	public void UpdateCurrentGameState(MtgGameState gameState)
	{
		UpdateGameStage(GameStageForGameState(gameState));
	}

	public void UpdateCurrentWorkflow(WorkflowBase workflow)
	{
		UpdateGameStage(GameStageForWorkflow(workflow));
	}

	private void UpdateGameStage(GameStage gameStage)
	{
		switch (gameStage)
		{
		case GameStage.Start_OpeningHand:
			ShowOpeningHandBrowser();
			break;
		case GameStage.Playing:
			CloseBrowser();
			break;
		case GameStage.Start_StartingPlayer:
			break;
		}
	}

	private void ShowOpeningHandBrowser()
	{
		IBrowser openedBrowser;
		if (!(_browserManager.CurrentBrowser is OpeningHandBrowser))
		{
			openedBrowser = _browserManager.OpenBrowser(new OpeningHandBrowserProvider());
		}
		else
		{
			IBrowser currentBrowser = _browserManager.CurrentBrowser;
			openedBrowser = currentBrowser;
		}
		_openedBrowser = openedBrowser;
	}

	private void CloseBrowser()
	{
		if (_openedBrowser != null)
		{
			_openedBrowser.Close();
			_openedBrowser = null;
		}
	}

	public static GameStage GameStageForGameState(MtgGameState gameState)
	{
		if (IsGameStart(gameState))
		{
			if (IsOpeningHandStage(gameState))
			{
				return GameStage.Start_OpeningHand;
			}
			return GameStage.Start;
		}
		if (IsGamePlaying(gameState))
		{
			return GameStage.Playing;
		}
		return GameStage.None;
	}

	private static bool IsGameStart(MtgGameState gameState)
	{
		if (gameState != null)
		{
			return gameState.Stage == Wotc.Mtgo.Gre.External.Messaging.GameStage.Start;
		}
		return false;
	}

	private static bool IsOpeningHandStage(MtgGameState gameState)
	{
		if (gameState == null)
		{
			return false;
		}
		if (gameState.LocalPlayer == null)
		{
			return false;
		}
		if (gameState.Stage == Wotc.Mtgo.Gre.External.Messaging.GameStage.Start)
		{
			return gameState.LocalPlayerPendingMessageType == ClientMessageType.MulliganResp;
		}
		return false;
	}

	private static bool IsGamePlaying(MtgGameState gameState)
	{
		if (gameState != null)
		{
			return gameState.Stage == Wotc.Mtgo.Gre.External.Messaging.GameStage.Play;
		}
		return false;
	}

	public static GameStage GameStageForWorkflow(WorkflowBase workflowBase)
	{
		if (workflowBase is ChooseStartingPlayerWorkflow)
		{
			return GameStage.Start_StartingPlayer;
		}
		if (workflowBase is MulliganWorkflow)
		{
			return GameStage.Start_OpeningHand;
		}
		return GameStage.Playing;
	}

	public void Dispose()
	{
		_gameStateProvider.CurrentGameState.ValueUpdated -= UpdateCurrentGameState;
	}
}
