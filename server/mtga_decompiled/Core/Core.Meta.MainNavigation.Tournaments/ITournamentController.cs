using System;
using System.Collections.Generic;
using SharedClientCore.SharedClientCore.Code.ClientModels;
using Wizards.Arena.Enums.Tournament;
using Wizards.Arena.Promises;

namespace Core.Meta.MainNavigation.Tournaments;

public interface ITournamentController
{
	event Action<Client_TournamentIsReady> TournamentStarting;

	event Action<Client_TournamentRoundIsReady> TournamentRoundStarting;

	Promise<Client_TournamentState> CreateTournament(string tournamentId, PairingType pairingType, int maxRounds, List<string> playerIds);

	Promise<List<Client_TournamentPlayer>> GetTournamentStandings(string tournamentId);

	Promise<Client_TournamentState> TournamentPlayerReadyStart(string tournamentId, string avatarId, string deckId, string playerId);

	Promise<Client_TournamentState> TournamentPlayerReadyRound(string tournamentId, string avatarId, string playerId);

	Promise<Client_TournamentState> TournamentDropPlayer(string tournamentId, string playerId);

	string GetLatestTournamentId();

	string GetTournamentDeckId();

	void SetTournamentDeckId(string deckId);
}
