using System;
using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace GreClient.Rules;

public class SelectTargetsRequestNPEHandlerFactory : RequestHandlerFactory<SelectTargetsRequest>
{
	private readonly Stack<uint> _idxHistory = new Stack<uint>();

	private SelectTargetsRequest _prvRequest;

	private readonly NPE_Game _game;

	private readonly DeckHeuristic _aiConfig;

	private MtgGameState _gameState;

	public SelectTargetsRequestNPEHandlerFactory(NPE_Game game, DeckHeuristic aiConfig)
	{
		_game = game;
		_aiConfig = aiConfig;
	}

	public override BaseUserRequestHandler GetHandlerForRequest(SelectTargetsRequest request)
	{
		if (_prvRequest != null)
		{
			SelectTargetsRequest prvRequest = _prvRequest;
			prvRequest.OnSubmit = (Action<ClientToGREMessage>)Delegate.Remove(prvRequest.OnSubmit, new Action<ClientToGREMessage>(OnRequestHandled));
			_prvRequest = null;
			_idxHistory.Clear();
		}
		_prvRequest = request;
		request.OnSubmit = (Action<ClientToGREMessage>)Delegate.Combine(request.OnSubmit, new Action<ClientToGREMessage>(OnRequestHandled));
		uint result;
		return new SelectTargetsRequestNPEHandler(request, _game, _gameState, _aiConfig, TargetSelectIdx(_idxHistory.TryPeek(out result) ? result : 0u, request.TargetSelections));
	}

	public override void SetGameState(MtgGameState state)
	{
		_gameState = state;
	}

	private static int TargetSelectIdx(uint prvIdx, IReadOnlyList<TargetSelection> targetSelections)
	{
		for (int i = 0; i < targetSelections.Count; i++)
		{
			if (targetSelections[i].TargetIdx == prvIdx)
			{
				return i;
			}
		}
		return 0;
	}

	private void OnRequestHandled(ClientToGREMessage outMsg)
	{
		if (outMsg.Type == ClientMessageType.CancelActionReq || outMsg.Type == ClientMessageType.SubmitTargetsReq)
		{
			_idxHistory.Clear();
		}
		else if (outMsg.Type == ClientMessageType.UndoReq)
		{
			_idxHistory.Pop();
		}
		else if (outMsg.Type == ClientMessageType.SelectTargetsResp)
		{
			_idxHistory.Push(outMsg.SelectTargetsResp.Target.TargetIdx);
		}
		SelectTargetsRequest prvRequest = _prvRequest;
		prvRequest.OnSubmit = (Action<ClientToGREMessage>)Delegate.Remove(prvRequest.OnSubmit, new Action<ClientToGREMessage>(OnRequestHandled));
		_prvRequest = null;
	}
}
