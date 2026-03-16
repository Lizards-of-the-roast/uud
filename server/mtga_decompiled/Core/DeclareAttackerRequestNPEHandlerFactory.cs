using System;
using System.Collections.Generic;
using GreClient.Rules;
using Pooling;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

public class DeclareAttackerRequestNPEHandlerFactory : RequestHandlerFactory<DeclareAttackerRequest>
{
	private readonly IObjectPool _objectPool = new ObjectPool();

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly DeckHeuristic _aiConfig;

	private readonly List<uint> _turnsToNotAttack = new List<uint>();

	private readonly List<uint> _turnsToAttackAll = new List<uint>();

	private readonly Dictionary<uint, List<uint>> _turnsToAttackWithCreature = new Dictionary<uint, List<uint>>();

	private List<uint> _attacksToMake;

	private bool _submitAttackers;

	private DeclareAttackerRequest _prvRequest;

	private MtgGameState _gameState;

	public DeclareAttackerRequestNPEHandlerFactory(ICardDatabaseAdapter cardDatabase, DeckHeuristic aiConfig, NPE_Game game)
	{
		_cardDatabase = cardDatabase;
		_aiConfig = aiConfig;
		_turnsToNotAttack = new List<uint>(game._turnsToNotAttack);
		_turnsToAttackAll = new List<uint>(game._turnsToAttackAll);
		_turnsToAttackWithCreature = new Dictionary<uint, List<uint>>(game._turnsToAttackWithCreature);
	}

	public override BaseUserRequestHandler GetHandlerForRequest(DeclareAttackerRequest request)
	{
		if (_prvRequest != null)
		{
			DeclareAttackerRequest prvRequest = _prvRequest;
			prvRequest.OnSubmit = (Action<ClientToGREMessage>)Delegate.Remove(prvRequest.OnSubmit, new Action<ClientToGREMessage>(OnRequestHandled));
			_prvRequest = null;
		}
		_prvRequest = request;
		request.OnSubmit = (Action<ClientToGREMessage>)Delegate.Combine(request.OnSubmit, new Action<ClientToGREMessage>(OnRequestHandled));
		if (_turnsToNotAttack.Contains(_gameState.GameWideTurn) || _submitAttackers)
		{
			return new SubmitAttackerRequestHandler(request);
		}
		if (_turnsToAttackAll.Remove(_gameState.GameWideTurn))
		{
			_submitAttackers = true;
			return new AttackAllRequestHandler(request, _objectPool);
		}
		if (_attacksToMake == null)
		{
			_attacksToMake = AttackingAI.GetBestAttackConfiguration(_aiConfig, _gameState, request, _cardDatabase);
		}
		return new DeclareAttackerRequestNPEHandler(request, _gameState, _attacksToMake, _turnsToAttackWithCreature);
	}

	public override void SetGameState(MtgGameState state)
	{
		_gameState = state;
	}

	private void OnRequestHandled(ClientToGREMessage outMsg)
	{
		if (outMsg.Type == ClientMessageType.CancelActionReq || outMsg.Type == ClientMessageType.SubmitAttackersReq)
		{
			_attacksToMake = null;
			_submitAttackers = false;
		}
		else if (outMsg.Type == ClientMessageType.DeclareAttackersResp && outMsg.DeclareAttackersResp != null && _attacksToMake != null)
		{
			foreach (Attacker selectedAttacker in outMsg.DeclareAttackersResp.SelectedAttackers)
			{
				_attacksToMake.Remove(selectedAttacker.AttackerInstanceId);
			}
		}
		DeclareAttackerRequest prvRequest = _prvRequest;
		prvRequest.OnSubmit = (Action<ClientToGREMessage>)Delegate.Remove(prvRequest.OnSubmit, new Action<ClientToGREMessage>(OnRequestHandled));
		_prvRequest = null;
	}
}
