using System;
using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.DuelScene.UXEvents;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.AutoPlay;

public class TimedReplayWorkflow : WorkflowBase, IUpdateWorkflow
{
	private readonly WorkflowBase _nestedWorkflow;

	private readonly Action<ClientToGREMessage> _submitAction;

	public override BaseUserRequest BaseRequest => _nestedWorkflow.BaseRequest;

	public override RequestType Type => _nestedWorkflow.Type;

	public override uint SourceId => _nestedWorkflow.SourceId;

	public override Prompt Prompt => _nestedWorkflow.Prompt;

	public TimedReplayWorkflow(WorkflowBase nested)
	{
		_nestedWorkflow = nested;
		_submitAction = nested.BaseRequest.OnSubmit;
		nested.BaseRequest.OnSubmit = null;
	}

	public override void TryUndo()
	{
		_nestedWorkflow.TryUndo();
	}

	public override bool CanApply(List<UXEvent> events)
	{
		return _nestedWorkflow.CanApply(events);
	}

	public void Update()
	{
		if (_nestedWorkflow is IUpdateWorkflow updateWorkflow)
		{
			updateWorkflow.Update();
		}
	}

	public override void CleanUp()
	{
		base.CleanUp();
		_nestedWorkflow.CleanUp();
	}

	protected override void ApplyInteractionInternal()
	{
		_nestedWorkflow.ApplyInteraction();
	}

	public void SubmitResponse(ClientToGREMessage replayMessage)
	{
		_submitAction?.Invoke(replayMessage);
	}
}
