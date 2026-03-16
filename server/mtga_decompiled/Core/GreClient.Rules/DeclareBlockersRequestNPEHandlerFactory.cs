using System;
using System.Collections.Generic;
using System.Linq;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace GreClient.Rules;

public class DeclareBlockersRequestNPEHandlerFactory : RequestHandlerFactory<DeclareBlockersRequest>
{
	private readonly DeckHeuristic _aiConfig;

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly NPE_Game _npeGame;

	private readonly float _delayInMs;

	private List<BlockToMake> _blocksToMake;

	private Blocker _blocker;

	private DeclareBlockersRequest _prvRequest;

	private MtgGameState _gameState;

	public DeclareBlockersRequestNPEHandlerFactory(DeckHeuristic aiConfig, ICardDatabaseAdapter cardDatabase, NPE_Game npeGame, float delayInMs)
	{
		_aiConfig = aiConfig;
		_cardDatabase = cardDatabase;
		_npeGame = npeGame;
		_delayInMs = delayInMs;
		_blocksToMake = null;
	}

	public override BaseUserRequestHandler GetHandlerForRequest(DeclareBlockersRequest request)
	{
		if (_prvRequest != null)
		{
			DeclareBlockersRequest prvRequest = _prvRequest;
			prvRequest.OnSubmit = (Action<ClientToGREMessage>)Delegate.Remove(prvRequest.OnSubmit, new Action<ClientToGREMessage>(OnRequestHandled));
			_prvRequest = null;
		}
		_prvRequest = request;
		request.OnSubmit = (Action<ClientToGREMessage>)Delegate.Combine(request.OnSubmit, new Action<ClientToGREMessage>(OnRequestHandled));
		if (_blocksToMake == null)
		{
			_blocksToMake = BlockingAI.GetBestBlockConfiguration(new SimpleGameStateConstruction(_aiConfig, _gameState.LocalPlayer.InstanceId, _gameState, _cardDatabase.AbilityDataProvider, _npeGame.InjectedNpeDirector), request, _cardDatabase).GetListOfBlocksToMake();
		}
		if (_blocksToMake.Count > 0)
		{
			BlockToMake blockToMake = _blocksToMake.First();
			foreach (Blocker allBlocker in request.AllBlockers)
			{
				if (allBlocker.BlockerInstanceId == blockToMake.BlockerId && allBlocker.AttackerInstanceIds.Contains(blockToMake.AttackerId))
				{
					_blocker = allBlocker;
					break;
				}
			}
			if (_blocker == null)
			{
				return new SubmitBlockerRequestHandler(request);
			}
			return new DelayedRequestHandler(new DeclareBlockersRequestNPEStrategyHandler(request, _blocksToMake, _blocker), _delayInMs);
		}
		return new DelayedRequestHandler(new SubmitBlockerRequestHandler(request), _delayInMs);
	}

	public override void SetGameState(MtgGameState state)
	{
		_gameState = state;
	}

	private void OnRequestHandled(ClientToGREMessage outMsg)
	{
		if (outMsg.Type == ClientMessageType.CancelActionReq || outMsg.Type == ClientMessageType.SubmitBlockersReq)
		{
			_blocksToMake = null;
			_blocker = null;
		}
		else if (outMsg.Type == ClientMessageType.DeclareBlockersResp && outMsg.DeclareBlockersResp != null && _blocksToMake != null)
		{
			foreach (Blocker blocker in outMsg.DeclareBlockersResp.SelectedBlockers)
			{
				_blocksToMake.RemoveAll((BlockToMake x) => x.BlockerId == blocker.BlockerInstanceId);
			}
		}
		DeclareBlockersRequest prvRequest = _prvRequest;
		prvRequest.OnSubmit = (Action<ClientToGREMessage>)Delegate.Remove(prvRequest.OnSubmit, new Action<ClientToGREMessage>(OnRequestHandled));
		_prvRequest = null;
	}
}
