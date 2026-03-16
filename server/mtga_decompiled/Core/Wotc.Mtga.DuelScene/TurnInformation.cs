using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class TurnInformation
{
	public enum ActivePlayer
	{
		Any,
		Player,
		AI,
		None
	}

	public int numberOfCreaturesCasted;

	public int numberOfSorceryCasted;

	public int numberOfInstantCasted;

	private BaseUserRequest _previousRequest;

	private int _requestCounter;

	private const int MAX_REQUEST_COUNTER = 5;

	public int numberOfTurnsWithoutAttacks;

	public int numberOfCallsToGREAutoResponder;

	public bool attackersDeclared;

	public int numberOfAttemptsSubmitAttackers;

	public List<uint> attackersToDeclare = new List<uint>();

	public bool blockersDeclared;

	public int numberOfAttemptsSubmitBlockers;

	public List<BlockToMake> blocksToAssign = new List<BlockToMake>();

	public readonly List<Action> ActionsTaken = new List<Action>();

	public ActivePlayer activePlayer { get; private set; }

	public Phase phase { get; private set; }

	public Step step { get; private set; }

	private void Reset()
	{
		step = Step.None;
		numberOfCreaturesCasted = 0;
		numberOfSorceryCasted = 0;
		numberOfInstantCasted = 0;
		numberOfTurnsWithoutAttacks = 0;
		numberOfCallsToGREAutoResponder = 0;
		attackersDeclared = false;
		numberOfAttemptsSubmitAttackers = 0;
		attackersToDeclare.Clear();
		blockersDeclared = false;
		numberOfAttemptsSubmitBlockers = 0;
		blocksToAssign.Clear();
		ActionsTaken.Clear();
		ClearRequest();
	}

	public void ClearRequest()
	{
		_previousRequest = null;
		_requestCounter = 0;
	}

	public bool IsOverMaxRequestCounter(BaseUserRequest request)
	{
		if (_previousRequest == null)
		{
			_previousRequest = request;
			_requestCounter++;
			return false;
		}
		if (_previousRequest.Prompt.PromptId == request.Prompt.PromptId)
		{
			_requestCounter++;
		}
		else
		{
			_previousRequest = request;
			_requestCounter = 0;
		}
		return _requestCounter > 5;
	}

	public void SetActivePlayer(MtgPlayer player)
	{
		SetActivePlayer(ConvertFromMtgPlayer(player));
	}

	private void SetActivePlayer(ActivePlayer newActivePlayer)
	{
		if (activePlayer != newActivePlayer)
		{
			activePlayer = newActivePlayer;
			Reset();
		}
	}

	public void SetPhase(Phase newPhase)
	{
		if (phase != newPhase)
		{
			phase = newPhase;
			Reset();
		}
	}

	public void SetStep(Step newStep)
	{
		step = newStep;
	}

	private static ActivePlayer ConvertFromMtgPlayer(MtgPlayer player)
	{
		if (player == null)
		{
			return ActivePlayer.None;
		}
		return player.ClientPlayerEnum switch
		{
			GREPlayerNum.LocalPlayer => ActivePlayer.AI, 
			GREPlayerNum.Opponent => ActivePlayer.Player, 
			_ => ActivePlayer.None, 
		};
	}
}
