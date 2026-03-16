using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.NPE;

public class ActionsAvailableQueueHandler : BaseUserRequestHandler<ActionsAvailableRequest>
{
	private readonly List<Action> _actionsToTake;

	public ActionsAvailableQueueHandler(ActionsAvailableRequest request, List<Action> actionsToTake)
		: base(request)
	{
		_actionsToTake = actionsToTake;
	}

	public override void HandleRequest()
	{
		if (TryGetNextActionToTake(_actionsToTake, _request.Actions, out var result))
		{
			_actionsToTake.RemoveAt(0);
			_request.SubmitAction(result);
		}
		else if (_request.CanPass)
		{
			_request.SubmitPass();
		}
		else if (_request.Actions.Count > 0)
		{
			_request.SubmitAction(_request.Actions[0]);
		}
	}

	private bool TryGetNextActionToTake(List<Action> actionsToTake, IReadOnlyList<Action> actions, out Action result)
	{
		if (actionsToTake.Count > 0 && actions.Count > 0)
		{
			Action action = actionsToTake[0];
			foreach (Action action2 in actions)
			{
				if (action2.GrpId == action.GrpId && action2.ActionType == action.ActionType)
				{
					result = action2;
					return true;
				}
			}
		}
		result = null;
		return false;
	}
}
