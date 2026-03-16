using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.NPE;

public class RequestHandlerFactory_NPE_Game1 : RequestHandlerFactory<BaseUserRequest>
{
	private class SelectTargetsHandler : BaseUserRequestHandler<SelectTargetsRequest>
	{
		public SelectTargetsHandler(SelectTargetsRequest request)
			: base(request)
		{
		}

		public override void HandleRequest()
		{
			if (CanSubmit(_request.TargetSelections))
			{
				_request.SubmitTargets();
				return;
			}
			foreach (TargetSelection targetSelection in _request.TargetSelections)
			{
				if (!targetSelection.CanSubmit() && TryGetTargetToUpdate(targetSelection.SelectableTargets(), out var result))
				{
					_request.UpdateTarget(result, targetSelection.TargetIdx);
					break;
				}
			}
		}

		private static bool CanSubmit(IReadOnlyList<TargetSelection> targetSelections)
		{
			foreach (TargetSelection targetSelection in targetSelections)
			{
				if (!targetSelection.CanSubmit())
				{
					return false;
				}
			}
			return true;
		}

		private static bool TryGetTargetToUpdate(IEnumerable<Target> targets, out Target result)
		{
			result = null;
			foreach (Target target in targets)
			{
				if (target.Highlight == Wotc.Mtgo.Gre.External.Messaging.HighlightType.Hot)
				{
					result = target;
					return true;
				}
				if (result == null)
				{
					result = target;
				}
			}
			return result != null;
		}
	}

	private class DeclareBlockersHandler : BaseUserRequestHandler<DeclareBlockersRequest>
	{
		public DeclareBlockersHandler(DeclareBlockersRequest request)
			: base(request)
		{
		}

		public override void HandleRequest()
		{
			if (TryGetBlockerToUpdate(_request.AllBlockers, out var result))
			{
				result.SelectedAttackerInstanceIds.Add(result.AttackerInstanceIds[0]);
				_request.UpdateBlockers(result);
			}
			else
			{
				_request.SubmitBlockers();
			}
		}

		private bool TryGetBlockerToUpdate(IEnumerable<Blocker> blockers, out Blocker result)
		{
			result = null;
			foreach (Blocker blocker in blockers)
			{
				if (blocker.SelectedAttackerInstanceIds.Count <= 0)
				{
					result = blocker;
					break;
				}
			}
			return result != null;
		}
	}

	private class DeclareAttackersHandler : BaseUserRequestHandler<DeclareAttackerRequest>
	{
		private readonly DeclareAttackersState _state;

		public DeclareAttackersHandler(DeclareAttackerRequest request, DeclareAttackersState state)
			: base(request)
		{
			_state = state;
		}

		public override void HandleRequest()
		{
			if (_state.DeclareAttackers)
			{
				_state.DeclareAttackers = false;
				_request.DeclareAllAttackers(_request.Attackers[0].LegalDamageRecipients[0]);
			}
			else
			{
				_request.SubmitAttackers();
			}
		}
	}

	private class DeclareAttackersState
	{
		public bool DeclareAttackers = true;
	}

	private readonly List<Action> _actionsToTake;

	private readonly DeclareAttackersState _declareAttackersState = new DeclareAttackersState();

	private readonly RequestHandlerFactory<BaseUserRequest> _defaultHandler;

	public RequestHandlerFactory_NPE_Game1(RequestHandlerFactory<BaseUserRequest> defaultHandler)
	{
		_actionsToTake = new List<Action>(NPEActions.GetGame1Actions());
		_defaultHandler = defaultHandler;
	}

	public override BaseUserRequestHandler GetHandlerForRequest(BaseUserRequest request)
	{
		if (!(request is ActionsAvailableRequest request2))
		{
			if (!(request is DeclareAttackerRequest request3))
			{
				if (!(request is DeclareBlockersRequest request4))
				{
					if (request is SelectTargetsRequest request5)
					{
						return new SelectTargetsHandler(request5);
					}
					return _defaultHandler.GetHandlerForRequest(request);
				}
				return new DeclareBlockersHandler(request4);
			}
			return new DeclareAttackersHandler(request3, _declareAttackersState);
		}
		return new ActionsAvailableQueueHandler(request2, _actionsToTake);
	}

	public override void SetGameState(MtgGameState state)
	{
		_defaultHandler.SetGameState(state);
	}
}
