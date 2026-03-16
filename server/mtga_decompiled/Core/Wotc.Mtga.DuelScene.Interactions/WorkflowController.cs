using System;
using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;
using WorkflowVisuals;
using Wotc.Mtga.DuelScene.UXEvents;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions;

public class WorkflowController : IWorkflowProvider, IClickableWorkflowProvider, IDisposable
{
	private readonly IWorkflowTranslator _workflowTranslation;

	private readonly MutableWorkflowProvider _provider;

	public WorkflowBase CurrentWorkflow => _provider.GetCurrentWorkflow();

	public WorkflowBase PendingWorkflow => _provider.GetPendingWorkflow();

	public event Action<Highlights> OnHighlightsUpdated;

	public event Action<Dimming> OnDimmingUpdated;

	public event Action<Arrows> OnArrowsUpdated;

	public event Action<Buttons> OnButtonsUpdated;

	public event Action<WorkflowPrompt> OnPromptUpdated;

	public event Action<WorkflowBase> InteractionApplied;

	public event System.Action InteractionCleared;

	public WorkflowBase GetCurrentWorkflow()
	{
		return _provider.GetCurrentWorkflow();
	}

	public WorkflowBase GetPendingWorkflow()
	{
		return _provider.GetPendingWorkflow();
	}

	public bool CanClick(IEntityView entity, SimpleInteractionType clickType)
	{
		return _provider.CanClick(entity, clickType);
	}

	public void OnClick(IEntityView entity, SimpleInteractionType clickType)
	{
		_provider.OnClick(entity, clickType);
	}

	public WorkflowController(IWorkflowTranslator workflowTranslation, MutableWorkflowProvider provider)
	{
		_workflowTranslation = workflowTranslation;
		_provider = provider ?? new MutableWorkflowProvider();
	}

	public void EnqueueRequest(BaseUserRequest req)
	{
		req.OnSubmit = (Action<ClientToGREMessage>)Delegate.Combine(req.OnSubmit, new Action<ClientToGREMessage>(OnRequestSubmit));
		if (!CanUpdateRoundTripWorkflow(req))
		{
			WorkflowBase workflowBase = _workflowTranslation.Translate(req);
			_provider.Workflow = workflowBase;
			if (workflowBase != null)
			{
				workflowBase.UpdateArrows += this.OnArrowsUpdated;
				workflowBase.UpdateHighlights += this.OnHighlightsUpdated;
				workflowBase.UpdateDimming += this.OnDimmingUpdated;
				workflowBase.UpdateButtons += this.OnButtonsUpdated;
				workflowBase.UpdatePrompt += this.OnPromptUpdated;
			}
		}
	}

	private void OnRequestSubmit(ClientToGREMessage outMsg)
	{
		if (outMsg.Type == ClientMessageType.CancelActionReq)
		{
			CleanUpCurrentWorkflow();
		}
		else if (_provider.Workflow is IRoundTripWorkflow roundTripWorkflow)
		{
			if (roundTripWorkflow.CanCleanupAfterOutboundMessage(outMsg))
			{
				CleanUpCurrentWorkflow();
			}
		}
		else if (_provider.Workflow != null)
		{
			CleanUpCurrentWorkflow();
		}
	}

	private bool CanUpdateRoundTripWorkflow(BaseUserRequest req)
	{
		if (_provider.Workflow is IRoundTripWorkflow roundTripWorkflow)
		{
			if (roundTripWorkflow.CanHandleRequest(req))
			{
				roundTripWorkflow.OnRoundTrip(req);
				return true;
			}
			Debug.LogErrorFormat("ROUND TRIP MISMATCH of request {0} and workflow {1}", req.ToString(), _provider.Workflow.ToString());
			CleanUpCurrentWorkflow();
		}
		return false;
	}

	public void Update(List<UXEvent> events)
	{
		WorkflowBase workflow = _provider.Workflow;
		if (workflow == null)
		{
			return;
		}
		if (workflow.AppliedState == InteractionAppliedState.Applied)
		{
			if (workflow is IUpdateWorkflow updateWorkflow)
			{
				updateWorkflow.Update();
			}
		}
		else if (workflow.CanApply(events))
		{
			this.InteractionApplied?.Invoke(workflow);
			if (!(workflow is IAutoRespondWorkflow autoRespondWorkflow) || !autoRespondWorkflow.TryAutoRespond())
			{
				TaskbarFlash.Flash();
				workflow.ApplyInteraction();
			}
		}
	}

	public void CleanUpCurrentWorkflow()
	{
		WorkflowBase workflow = _provider.Workflow;
		if (workflow != null)
		{
			workflow.CleanUp();
			workflow.UpdateArrows -= this.OnArrowsUpdated;
			workflow.UpdateHighlights -= this.OnHighlightsUpdated;
			workflow.UpdateDimming -= this.OnDimmingUpdated;
			workflow.UpdateButtons -= this.OnButtonsUpdated;
			workflow.UpdatePrompt -= this.OnPromptUpdated;
			_provider.Workflow = null;
			this.OnArrowsUpdated?.Invoke(Arrows.GetDefault());
			this.OnHighlightsUpdated?.Invoke(new Highlights());
			this.OnDimmingUpdated?.Invoke(new Dimming
			{
				WorkflowActive = false
			});
			this.OnButtonsUpdated?.Invoke(new Buttons());
			this.OnPromptUpdated?.Invoke(new WorkflowPrompt());
			this.InteractionCleared?.Invoke();
		}
	}

	public void Dispose()
	{
		if (_workflowTranslation is IDisposable disposable)
		{
			disposable.Dispose();
		}
		this.OnHighlightsUpdated = null;
		this.OnDimmingUpdated = null;
		this.OnArrowsUpdated = null;
		this.OnButtonsUpdated = null;
		this.InteractionApplied = null;
		this.InteractionCleared = null;
	}
}
