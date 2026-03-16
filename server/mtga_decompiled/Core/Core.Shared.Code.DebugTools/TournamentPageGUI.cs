using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Code.ClientFeatureToggle;
using Core.Meta.MainNavigation.Tournaments;
using MTGA.Social;
using SharedClientCore.SharedClientCore.Code.ClientModels;
using UnityEngine;
using Wizards.Arena.Enums.Tournament;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Mtga.Decks;

namespace Core.Shared.Code.DebugTools;

public class TournamentPageGUI : IDebugGUIPage
{
	private DebugInfoIMGUIOnGui _GUI;

	private static GUIStyle _textInputStyleCache;

	private static GUIStyle _guiStyleWordWrapWhite;

	private Vector2 _currentScrollPos = Vector2.zero;

	private ITournamentController _tournamentController;

	private Promise<Client_TournamentState> _clientTournamentStatePromise;

	private string _promiseOutcome = "";

	private string _tournamentId = "";

	private string _playerId = "";

	private List<Client_Deck> _availableDecks = new List<Client_Deck>();

	private List<string> _availableDeckNames = new List<string>();

	private Vector2 _currentDeckScrollPos = Vector2.zero;

	private List<SocialEntity> _availableFriends = new List<SocialEntity>();

	private List<string> _availableFriendNames = new List<string>();

	private List<string> _pendingFriendNames = new List<string>();

	private Vector2 _currentAvailableFriendsScrollPos = Vector2.zero;

	private Vector2 _currentPendingFriendsScrollPos = Vector2.zero;

	private List<string> _pushNotificationHistory = new List<string>();

	private bool _tournamentFeatureToggle;

	public DebugInfoIMGUIOnGui.DebugTab TabType => DebugInfoIMGUIOnGui.DebugTab.Tournaments;

	public string TabName => "Tournaments";

	public bool HiddenInTab => false;

	private static GUIStyle _textInputStyle
	{
		get
		{
			if (_textInputStyleCache == null)
			{
				_textInputStyleCache = new GUIStyle(GUI.skin.GetStyle("TextField"));
			}
			return _textInputStyleCache;
		}
	}

	public void Init(DebugInfoIMGUIOnGui gui)
	{
		_GUI = gui;
		_tournamentController = Pantry.Get<ITournamentController>();
		_tournamentController.TournamentRoundStarting += OnTournamentRoundStarting;
		_tournamentController.TournamentStarting += OnTournamentReady;
	}

	public void Destroy()
	{
		if (_tournamentController != null)
		{
			_tournamentController.TournamentRoundStarting -= OnTournamentRoundStarting;
			_tournamentController.TournamentStarting -= OnTournamentReady;
		}
	}

	public void OnQuit()
	{
	}

	public bool OnUpdate()
	{
		ClientFeatureToggleDataProvider clientFeatureToggleDataProvider = Pantry.Get<ClientFeatureToggleDataProvider>();
		if (clientFeatureToggleDataProvider != null)
		{
			_tournamentFeatureToggle = clientFeatureToggleDataProvider.GetToggleValueById("Tournament");
		}
		return true;
	}

	public void OnGUI()
	{
		if (!_tournamentFeatureToggle)
		{
			GUILayout.Label("Client feature toggle for Tournament is disabled");
			return;
		}
		using (new GUILayout.ScrollViewScope(_currentScrollPos, false, true))
		{
			using (new GUILayout.HorizontalScope(GUILayout.ExpandHeight(expand: true)))
			{
				if (GUILayout.Button("Create Tournament", GUILayout.MaxWidth(200f)))
				{
					List<string> list = (from entity in _availableFriends
						where _pendingFriendNames.Contains(entity.DisplayName)
						select entity.PlayerId).ToList();
					list.Add(_playerId);
					_tournamentId = Guid.NewGuid().ToString();
					_tournamentController.CreateTournament(_tournamentId, PairingType.Swiss, 5, list).Then(delegate(Promise<Client_TournamentState> p)
					{
						if (p.Successful)
						{
							_promiseOutcome = "Successfully created tournament.";
						}
						else
						{
							_promiseOutcome = "Failed to create tournament: " + p.Error.Message + "\nException: \n" + p.Error.Exception.ToString();
						}
					});
				}
				GUILayout.Label("Tournament Id: " + _tournamentId, GUILayout.MaxWidth(200f));
			}
			using (new GUILayout.HorizontalScope(GUILayout.MaxHeight(500f)))
			{
				int indexFromDeckId = GetIndexFromDeckId(_tournamentController.GetTournamentDeckId());
				int num = _GUI.SelectionListUtility("Select Deck", indexFromDeckId, _availableDeckNames.ToArray(), ref _currentDeckScrollPos, GUILayout.MaxWidth(200f));
				if (indexFromDeckId != num)
				{
					_tournamentController.SetTournamentDeckId(GetDeckIdFromIndex(num));
				}
				GUILayout.Space(25f);
				GUILayout.BeginVertical();
				int num2 = -1;
				int num3 = _GUI.SelectionListUtility("Available Friends", num2, _availableFriendNames.ToArray(), ref _currentAvailableFriendsScrollPos, GUILayout.MaxWidth(200f));
				if (num2 != num3)
				{
					_pendingFriendNames.Add(_availableFriendNames[num3]);
					_availableFriendNames.RemoveAt(num3);
				}
				int num4 = -1;
				int num5 = _GUI.SelectionListUtility("Pending Friends", num4, _pendingFriendNames.ToArray(), ref _currentPendingFriendsScrollPos, GUILayout.MaxWidth(200f));
				if (num4 != num5)
				{
					_availableFriendNames.Add(_pendingFriendNames[num5]);
					_pendingFriendNames.RemoveAt(num5);
				}
				GUILayout.EndVertical();
				GUILayout.Space(25f);
			}
			using (new GUILayout.HorizontalScope(GUILayout.MaxHeight(50f)))
			{
				if (GUILayout.Button("Refresh Decks", GUILayout.MaxWidth(200f)))
				{
					RefreshDecks();
				}
				GUILayout.Space(25f);
				if (GUILayout.Button("Refresh Friends", GUILayout.MaxWidth(200f)))
				{
					RefreshFriends();
				}
			}
			using (new GUILayout.HorizontalScope(GUILayout.MaxHeight(100f)))
			{
				GUILayout.BeginVertical();
				GUILayout.Label("Latest PushNotification received:");
				if (_pushNotificationHistory.Count > 0)
				{
					GUILayout.Label(_pushNotificationHistory.Last());
				}
				GUILayout.EndVertical();
			}
			using (new GUILayout.HorizontalScope(GUILayout.ExpandHeight(expand: true)))
			{
				if (_GUI.ShowDebugButton("Get Standings", 200f))
				{
					if (!string.IsNullOrEmpty(_tournamentController.GetLatestTournamentId()))
					{
						_tournamentId = _tournamentController.GetLatestTournamentId();
					}
					_tournamentController.GetTournamentStandings(_tournamentId).Then(delegate(Promise<List<Client_TournamentPlayer>> p)
					{
						if (p.Successful)
						{
							_promiseOutcome = "Successfully got standings tournament.";
						}
						else
						{
							_promiseOutcome = "Failed to get standings: " + p.Error.Message + "\nException: \n" + p.Error.Exception.ToString();
						}
					});
				}
			}
			using (new GUILayout.HorizontalScope(GUILayout.ExpandHeight(expand: true)))
			{
				if (_GUI.ShowDebugButton("Ready Start", 200f))
				{
					string deckId = "62d466e6-ca2d-4d7f-b9b7-1e9b45082454";
					if (GetIndexFromDeckId(_tournamentController.GetTournamentDeckId()) != -1)
					{
						deckId = _tournamentController.GetTournamentDeckId();
					}
					if (!string.IsNullOrEmpty(_tournamentController.GetLatestTournamentId()))
					{
						_tournamentId = _tournamentController.GetLatestTournamentId();
					}
					_tournamentController.TournamentPlayerReadyStart(_tournamentId, "", deckId, _playerId).Then(delegate(Promise<Client_TournamentState> p)
					{
						if (p.Successful)
						{
							_promiseOutcome = "Successfully readied for tournament start.";
						}
						else
						{
							_promiseOutcome = "Failed to ready for tournament start: " + p.Error.Message + "\nException: \n" + p.Error.Exception.ToString();
						}
					});
				}
			}
			using (new GUILayout.HorizontalScope(GUILayout.ExpandHeight(expand: true)))
			{
				if (_GUI.ShowDebugButton("Ready Round", 200f))
				{
					if (!string.IsNullOrEmpty(_tournamentController.GetLatestTournamentId()))
					{
						_tournamentId = _tournamentController.GetLatestTournamentId();
					}
					_tournamentController.TournamentPlayerReadyRound(_tournamentId, "", _playerId).Then(delegate(Promise<Client_TournamentState> p)
					{
						if (p.Successful)
						{
							_promiseOutcome = "Successfully readied for tournament round.";
						}
						else
						{
							_promiseOutcome = "Failed to ready for tournament round: " + p.Error.Message + "\nException: \n" + p.Error.Exception.ToString();
						}
					});
				}
			}
			using (new GUILayout.HorizontalScope(GUILayout.ExpandHeight(expand: true)))
			{
				if (_GUI.ShowDebugButton("Drop from Tournament", 200f))
				{
					_tournamentController.TournamentDropPlayer(_tournamentId, _playerId).Then(delegate(Promise<Client_TournamentState> p)
					{
						if (p.Successful)
						{
							_promiseOutcome = "Successfully dropped from tournament.";
						}
						else
						{
							_promiseOutcome = "Failed to drop from tournament : " + p.Error.Message + "\nException: \n" + p.Error.Exception.ToString();
						}
					});
				}
			}
		}
		GUILayout.Label("----------- Promise States -----------");
		GUILayout.Label("----------- Promise<Client_TournamentState> -----------");
		using (new GUILayout.HorizontalScope(GUILayout.ExpandHeight(expand: true)))
		{
			GUILayout.Label(GetPromiseState(_clientTournamentStatePromise));
		}
		GUILayout.Label("----------- Promise Outcome -----------");
		using (new GUILayout.HorizontalScope(GUILayout.ExpandHeight(expand: true)))
		{
			GUILayout.Label(_promiseOutcome);
		}
	}

	private string GetPromiseState(Promise<Client_TournamentState> tournamentPromise)
	{
		return tournamentPromise?.State.ToString();
	}

	private void OnTournamentRoundStarting(Client_TournamentRoundIsReady notification)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Tournament round started at " + notification.TournamentId + " with:");
		foreach (string opponentDisplayName in notification.OpponentDisplayNames)
		{
			stringBuilder.AppendLine("\t" + opponentDisplayName);
		}
		_pushNotificationHistory.Add(stringBuilder.ToString());
	}

	private void OnTournamentReady(Client_TournamentIsReady notification)
	{
		_pushNotificationHistory.Add("Tournament ready: " + notification.TournamentId);
	}

	private void RefreshFriends()
	{
		IAccountClient accountClient = Pantry.Get<IAccountClient>();
		_playerId = accountClient.AccountInformation.PersonaID;
		ISocialManager socialManager = Pantry.Get<ISocialManager>();
		_availableFriends = socialManager.Friends;
		_availableFriendNames.Clear();
		_pendingFriendNames.Clear();
		foreach (SocialEntity availableFriend in _availableFriends)
		{
			_availableFriendNames.Add(availableFriend.DisplayName);
		}
	}

	private void RefreshDecks()
	{
		DeckDataProvider deckDataProvider = Pantry.Get<DeckDataProvider>();
		_availableDecks = deckDataProvider.GetCachedDecks();
		_availableDeckNames.Clear();
		foreach (Client_Deck availableDeck in _availableDecks)
		{
			_availableDeckNames.Add(availableDeck.Summary.Name);
		}
	}

	private int GetIndexFromDeckId(string deckId)
	{
		return _availableDecks.FindIndex((Client_Deck x) => x.Id.ToString() == deckId);
	}

	private string GetDeckIdFromIndex(int deckIndex)
	{
		return _availableDecks.ElementAtOrDefault(deckIndex)?.Id.ToString();
	}
}
