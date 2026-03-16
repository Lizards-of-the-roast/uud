using System;
using Wotc.Mtga.DuelScene.Interactions;

namespace Wotc.Mtga.DuelScene;

public class InteractionAppliedMediator : IDisposable
{
	private readonly WorkflowController _workflowController;

	private readonly IGameStartController _gameStartController;

	private readonly DuelSceneLogger _logger;

	public InteractionAppliedMediator(WorkflowController workflowController, IGameStartController gameStartController, DuelSceneLogger logger)
	{
		_workflowController = workflowController;
		_gameStartController = gameStartController;
		_logger = logger;
		_workflowController.InteractionApplied += _logger.OnInteractionApplied;
		_workflowController.InteractionApplied += _gameStartController.UpdateCurrentWorkflow;
	}

	public void Dispose()
	{
		_workflowController.InteractionApplied -= _logger.OnInteractionApplied;
		_workflowController.InteractionApplied -= _gameStartController.UpdateCurrentWorkflow;
	}
}
