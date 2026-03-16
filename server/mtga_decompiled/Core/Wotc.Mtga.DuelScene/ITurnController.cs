using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public interface ITurnController
{
	void SetExtraTurns(IReadOnlyList<ExtraTurn> extraTurns);

	void SetEventTranslationTurnNumber(uint number);

	void SetCurrentTurn(uint number, uint activePlayerId);

	void SetDecidingPlayer(uint decisionPlayerId);

	void SetPhase(Phase phase, Step step);
}
