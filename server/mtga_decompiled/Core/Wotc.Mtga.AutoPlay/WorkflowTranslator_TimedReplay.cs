using GreClient.Rules;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.AutoPlay;

public class WorkflowTranslator_TimedReplay : IWorkflowTranslator
{
	private readonly IWorkflowTranslator _baseTranslator;

	private TimedReplayWorkflow _currentWorkflow;

	public WorkflowTranslator_TimedReplay(IWorkflowTranslator original)
	{
		_baseTranslator = original;
	}

	public WorkflowBase Translate(BaseUserRequest req)
	{
		WorkflowBase nested = _baseTranslator.Translate(req);
		_currentWorkflow = new TimedReplayWorkflow(nested);
		return _currentWorkflow;
	}

	public void SendResponse(ClientToGREMessage replayMessage)
	{
		if (_currentWorkflow == null)
		{
			SimpleLog.LogError($"A GRE message was attempted to be sent before a workflow was created. Message: {replayMessage}");
		}
		else
		{
			_currentWorkflow.SubmitResponse(replayMessage);
		}
	}
}
