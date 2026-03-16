using System;
using WorkflowVisuals;
using Wotc.Mtga.DuelScene.Interactions;

namespace Wotc.Mtga.DuelScene;

public class WorkflowVisualsMediator : IDisposable
{
	private readonly WorkflowController _workflowController;

	private readonly IHighlightController _highlightController;

	private readonly IDimmingController _dimmingController;

	private readonly IntentionLineManager _intentionLineManager;

	private readonly IPromptTextController _promptTextController;

	private readonly UIManager _uiManager;

	public WorkflowVisualsMediator(WorkflowController workflowController, IHighlightController highlightController, IDimmingController dimmingController, IntentionLineManager intentionLineManager, IPromptTextController promptTextController, UIManager uiManager)
	{
		_workflowController = workflowController;
		_highlightController = highlightController;
		_dimmingController = dimmingController;
		_intentionLineManager = intentionLineManager;
		_promptTextController = promptTextController;
		_uiManager = uiManager;
		_workflowController.OnHighlightsUpdated += _highlightController.SetWorkflowHighlights;
		_workflowController.OnDimmingUpdated += _dimmingController.SetWorkflowDimming;
		_workflowController.OnArrowsUpdated += _intentionLineManager.SetWorkflowArrows;
		_workflowController.OnButtonsUpdated += _uiManager.SetWorkflowButtons;
		_workflowController.OnPromptUpdated += OnWorkflowPromptUpdated;
	}

	private void OnWorkflowPromptUpdated(WorkflowPrompt workflowPrompt)
	{
		if (!string.IsNullOrEmpty(workflowPrompt.LocKey))
		{
			_promptTextController.SetClientPrompt(workflowPrompt.LocKey, workflowPrompt.LocParams);
		}
		else
		{
			_promptTextController.SetPrompt(workflowPrompt.GrePrompt);
		}
	}

	public void Dispose()
	{
		_workflowController.OnHighlightsUpdated -= _highlightController.SetWorkflowHighlights;
		_workflowController.OnDimmingUpdated -= _dimmingController.SetWorkflowDimming;
		_workflowController.OnArrowsUpdated -= _intentionLineManager.SetWorkflowArrows;
		_workflowController.OnButtonsUpdated -= _uiManager.SetWorkflowButtons;
		_workflowController.OnPromptUpdated -= OnWorkflowPromptUpdated;
	}
}
