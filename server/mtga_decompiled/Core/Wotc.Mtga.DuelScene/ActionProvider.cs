using System;
using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class ActionProvider : IActionProvider
{
	private readonly IGameStateProvider _gameStateProvider;

	private readonly IWorkflowProvider _workflowProvider;

	public ActionProvider(IGameStateProvider gameStateProvider, IWorkflowProvider workflowProvider)
	{
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_workflowProvider = workflowProvider ?? NullWorkflowProvider.Default;
	}

	public IReadOnlyList<ActionInfo> GetGameStateActions(uint instanceId)
	{
		MtgGameState mtgGameState = _gameStateProvider.CurrentGameState;
		if (mtgGameState == null)
		{
			return Array.Empty<ActionInfo>();
		}
		return mtgGameState.GetActionsForCardId(instanceId);
	}

	public IReadOnlyList<GreInteraction> GetRequestActions(uint instanceId)
	{
		WorkflowBase currentWorkflow = _workflowProvider.GetCurrentWorkflow();
		if (currentWorkflow == null)
		{
			return Array.Empty<GreInteraction>();
		}
		return ActionsAvailableWorkflow.GetInteractionsForId(instanceId, currentWorkflow);
	}
}
