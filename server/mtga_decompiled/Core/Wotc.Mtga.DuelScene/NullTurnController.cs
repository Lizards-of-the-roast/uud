using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class NullTurnController : ITurnController
{
	public static readonly ITurnController Default = new NullTurnController();

	public void SetExtraTurns(IReadOnlyList<ExtraTurn> extraTurns)
	{
	}

	public void SetEventTranslationTurnNumber(uint number)
	{
	}

	public void SetCurrentTurn(uint number, uint activePlayerId)
	{
	}

	public void SetDecidingPlayer(uint decisionPlayerId)
	{
	}

	public void SetPhase(Phase phase, Step step)
	{
	}
}
