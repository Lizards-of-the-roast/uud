using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public readonly struct MtgTurnInfo
{
	public readonly Phase Phase;

	public readonly Phase NextPhase;

	public readonly Step Step;

	public readonly Step NextStep;

	public readonly uint TurnNumber;

	public readonly uint ActivePlayerId;

	public readonly uint DecisionPlayerId;

	public MtgTurnInfo(Phase phase, Phase nextPhase, Step step, Step nextStep, uint turnNumber, uint activePlayerId, uint decisionPlayerId)
	{
		Phase = phase;
		NextPhase = nextPhase;
		Step = step;
		NextStep = nextStep;
		TurnNumber = turnNumber;
		ActivePlayerId = activePlayerId;
		DecisionPlayerId = decisionPlayerId;
	}

	public MtgPlayer ActivePlayer(MtgGameState gameState)
	{
		if (!gameState.TryGetPlayer(ActivePlayerId, out var player))
		{
			return null;
		}
		return player;
	}

	public MtgPlayer DecisionPlayer(MtgGameState gameState)
	{
		if (!gameState.TryGetPlayer(DecisionPlayerId, out var player))
		{
			return null;
		}
		return player;
	}

	public bool PhaseChanged(MtgTurnInfo other)
	{
		if (Phase == other.Phase)
		{
			return Step != other.Step;
		}
		return true;
	}

	public MtgTurnInfo Modify(Phase? phase = null, Phase? nextPhase = null, Step? step = null, Step? nextStep = null, uint? turnNumber = null, uint? activePlayerId = null, uint? decisionPlayerId = null)
	{
		return new MtgTurnInfo(phase ?? Phase, nextPhase ?? NextPhase, step ?? Step, nextStep ?? NextStep, turnNumber ?? TurnNumber, activePlayerId ?? ActivePlayerId, decisionPlayerId ?? DecisionPlayerId);
	}
}
