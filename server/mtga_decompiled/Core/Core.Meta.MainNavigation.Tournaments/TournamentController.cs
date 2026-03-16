using System;
using System.Collections.Generic;
using Core.Code.Promises;
using Core.Shared.Code.WrapperFactories;
using MTGA.Social;
using SharedClientCore.SharedClientCore.Code.ClientModels;
using SharedClientCore.SharedClientCore.Code.Providers;
using Wizards.Arena.Enums.Tournament;
using Wizards.Arena.Promises;
using Wizards.Mtga;

namespace Core.Meta.MainNavigation.Tournaments;

public class TournamentController : ITournamentController
{
	private ITournamentDataProvider _tournamentDataProvider;

	private IBILogger _biLogger;

	private ITournamentController _tournamentControllerImplementation;

	private ISocialManager _socialManager;

	private Matchmaking _matchmaking;

	public event Action<Client_TournamentIsReady> TournamentStarting;

	public event Action<Client_TournamentRoundIsReady> TournamentRoundStarting;

	public TournamentController(ITournamentDataProvider tournamentDataProvider = null, ISocialManager socialManager = null, IBILogger biLogger = null)
	{
		_tournamentDataProvider = tournamentDataProvider ?? TournamentWrapperFactory.CreateDataProvider();
		_biLogger = biLogger;
		_socialManager = socialManager;
		_matchmaking = Pantry.Get<Matchmaking>();
		_tournamentDataProvider.NewTournamentIsReadyNotification += HandleTournamentIsReadyNotification;
		_tournamentDataProvider.NewTournamentRoundIsReadyNotification += HandleTournamentRoundIsReadyNotification;
	}

	public Promise<Client_TournamentState> CreateTournament(string tournamentId, PairingType pairingType, int maxRounds, List<string> playerIds)
	{
		return _tournamentDataProvider.CreateTournament(tournamentId, pairingType, maxRounds, playerIds);
	}

	public Promise<List<Client_TournamentPlayer>> GetTournamentStandings(string tournamentId)
	{
		return _tournamentDataProvider.GetTournamentStandings(tournamentId);
	}

	public Promise<Client_TournamentState> TournamentPlayerReadyStart(string tournamentId, string avatarId, string deckId, string playerId)
	{
		return _tournamentDataProvider.TournamentPlayerReadyStart(tournamentId, avatarId, deckId, playerId).ThenOnMainThread(delegate(Promise<Client_TournamentState> result)
		{
			if (!result.Error.IsError)
			{
				_matchmaking.SetupTournamentMatch();
			}
		});
	}

	public Promise<Client_TournamentState> TournamentPlayerReadyRound(string tournamentId, string avatarId, string playerId)
	{
		return _tournamentDataProvider.TournamentPlayerReadyRound(tournamentId, avatarId, playerId).ThenOnMainThread(delegate(Promise<Client_TournamentState> result)
		{
			if (!result.Error.IsError)
			{
				_matchmaking.SetupTournamentMatch();
			}
		});
	}

	public Promise<Client_TournamentState> TournamentDropPlayer(string tournamentId, string playerId)
	{
		return _tournamentDataProvider.TournamentDropPlayer(tournamentId, playerId);
	}

	public string GetLatestTournamentId()
	{
		return _tournamentDataProvider.LatestTournamentId;
	}

	public string GetTournamentDeckId()
	{
		return _tournamentDataProvider.TournamentDeckId;
	}

	public void SetTournamentDeckId(string deckId)
	{
		_tournamentDataProvider.UpdateTournamentDeckId(deckId);
	}

	private void HandleTournamentIsReadyNotification(Client_TournamentIsReady notification)
	{
		this.TournamentStarting?.Invoke(notification);
		_socialManager?.ForwardNotificationAlert(new SocialMessage(notification));
	}

	private void HandleTournamentRoundIsReadyNotification(Client_TournamentRoundIsReady notification)
	{
		this.TournamentRoundStarting?.Invoke(notification);
		_socialManager?.ForwardNotificationAlert(new SocialMessage(notification));
	}
}
