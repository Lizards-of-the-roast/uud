using System;
using System.Collections.Generic;
using System.Linq;
using Core.Code.Promises;
using Core.Meta.MainNavigation.Challenge;
using Newtonsoft.Json;
using SharedClientCore.SharedClientCore.Code.PVPChallenge;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;
using UnityEngine;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Mtga.Decks;

namespace Core.Shared.Code.DebugTools;

public class PVPChallengePageGUI : IDebugGUIPage
{
	private enum ScrollListOptionState
	{
		None,
		DeckSelect,
		ModeSelect,
		InviteSelect,
		KickSelect,
		BlockSelect
	}

	private DebugInfoIMGUIOnGui _GUI;

	private static GUIStyle _textInputStyleCache;

	private static GUIStyle _guiStyleWordWrapWhite;

	private IChallengeCommunicationWrapper _challengeInterface;

	private Dictionary<string, string> _players = new Dictionary<string, string>();

	private string _challengeJson;

	private PVPChallengeController _challengeController;

	private ScrollListOptionState _scrollListStatus;

	private Vector2 _currentScrollPos = Vector2.zero;

	private Guid _editingChallengeId = Guid.Empty;

	private string _toInvite = "";

	public DebugInfoIMGUIOnGui.DebugTab TabType => DebugInfoIMGUIOnGui.DebugTab.PVPChallenges;

	public string TabName => "Challenges";

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
	}

	public void Destroy()
	{
	}

	public void OnQuit()
	{
	}

	public bool OnUpdate()
	{
		return true;
	}

	public void OnGUI()
	{
		if (_challengeController == null)
		{
			_challengeController = Pantry.Get<PVPChallengeController>();
		}
		if (_challengeInterface == null)
		{
			_challengeInterface = Pantry.Get<IChallengeCommunicationWrapper>();
		}
		_challengeInterface.GetChallengeableFriendDisplayNames().IfSuccess(delegate(Promise<Dictionary<string, string>> promise)
		{
			_players = promise.Result;
		});
		if (_challengeController == null)
		{
			return;
		}
		GUILayout.BeginHorizontal();
		if (_GUI.ShowDebugButton("Create New Challenge", 200f))
		{
			_challengeController.CreateAndCacheChallenge();
		}
		if (_GUI.ShowDebugButton("Force Create New Challenge", 200f))
		{
			_challengeController.CreateAndCacheChallenge(forceCreate: true);
		}
		if (_GUI.ShowDebugButton("Reconnect all", 200f))
		{
			_challengeController.ReconnectAndCleanupOldChallenges();
		}
		GUILayout.EndHorizontal();
		GUILayout.Label("----------- Active Challenges -----------");
		foreach (KeyValuePair<Guid, PVPChallengeData> challenge in new Dictionary<Guid, PVPChallengeData>(_challengeController.GetAllChallenges()))
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("----------- " + challenge.Key.ToString() + " -----------");
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.TextField(GetChallengeJson(challenge.Value), _textInputStyle);
			GUILayout.BeginVertical();
			if (challenge.Value.Status == ChallengeStatus.Setup && _GUI.ShowDebugButton("Invite By Name", 200f))
			{
				if (!_players.TryGetValue(_toInvite, out var playerId))
				{
					_challengeInterface.GetPlayerIdFromFullPlayerName(_toInvite).ThenOnMainThread(delegate(Promise<string> promise)
					{
						if (promise.Successful)
						{
							playerId = promise.Result;
							_challengeController.AddChallengeInvite(challenge.Key, _toInvite, playerId);
							_challengeController.SendChallengeInvites(challenge.Key);
						}
					});
				}
				else
				{
					_challengeController.AddChallengeInvite(challenge.Key, _toInvite, playerId);
					_challengeController.SendChallengeInvites(challenge.Key);
				}
			}
			if (challenge.Value.Invites.ContainsKey(challenge.Value.LocalPlayerId) && (challenge.Value.Invites[challenge.Value.LocalPlayerId].Status != InviteStatus.Accepted || (challenge.Value.Invites[challenge.Value.LocalPlayerId].Status == InviteStatus.Accepted && !challenge.Value.ChallengePlayers.ContainsKey(challenge.Value.LocalPlayerId))))
			{
				if (_GUI.ShowDebugButton("Accept Challenge", 200f))
				{
					_challengeController.AcceptChallengeInvite(challenge.Value.ChallengeId);
				}
				if (_GUI.ShowDebugButton("Reject Challenge", 200f))
				{
					_challengeController.RejectChallengeInvite(challenge.Value.ChallengeId);
				}
			}
			if (challenge.Value.LocalPlayer != null)
			{
				if (_GUI.ShowDebugButton("Leave Challenge", 200f))
				{
					_challengeController.LeaveChallenge(challenge.Value.ChallengeId);
				}
				if (_GUI.ShowDebugButton("Remove Challenge Cache", 200f))
				{
					_challengeController.RemoveChallenge(challenge.Value.ChallengeId);
				}
				PlayerStatus playerStatus = challenge.Value.LocalPlayer.PlayerStatus;
				if (playerStatus == PlayerStatus.None || playerStatus == PlayerStatus.NotReady)
				{
					if (_GUI.ShowDebugButton("Ready", 200f))
					{
						_challengeController.SetLocalPlayerStatus(challenge.Value.ChallengeId, PlayerStatus.Ready);
					}
				}
				else if (_GUI.ShowDebugButton("Not Ready", 200f))
				{
					_challengeController.SetLocalPlayerStatus(challenge.Value.ChallengeId, PlayerStatus.NotReady);
				}
			}
			if (_challengeController.AllPlayersReady(challenge.Value) && challenge.Value.Status != ChallengeStatus.Starting && _GUI.ShowDebugButton("Launch Challenge", 200f))
			{
				_challengeController.LaunchChallenge(challenge.Value.ChallengeId);
			}
			if (challenge.Value.Status == ChallengeStatus.Starting && challenge.Value.LocalPlayer?.DeckId != Guid.Empty && _GUI.ShowDebugButton("Play", 200f))
			{
				_challengeController.JoinChallengeMatch(challenge.Value.ChallengeId);
			}
			GUILayout.EndVertical();
			GUILayout.BeginVertical();
			if (challenge.Value.Status == ChallengeStatus.Setup)
			{
				_toInvite = GUILayout.TextField(_toInvite, _textInputStyle);
			}
			if (challenge.Value.Status == ChallengeStatus.Setup)
			{
				if (_GUI.ShowDebugButton("Invite Friend", 200f))
				{
					_editingChallengeId = challenge.Key;
					if (_scrollListStatus != ScrollListOptionState.InviteSelect)
					{
						_scrollListStatus = ScrollListOptionState.InviteSelect;
					}
					else
					{
						_scrollListStatus = ScrollListOptionState.None;
					}
				}
				if (_GUI.ShowDebugButton("Edit Deck", 200f))
				{
					_editingChallengeId = challenge.Key;
					if (_scrollListStatus != ScrollListOptionState.DeckSelect)
					{
						_scrollListStatus = ScrollListOptionState.DeckSelect;
					}
					else
					{
						_scrollListStatus = ScrollListOptionState.None;
					}
				}
				if (_GUI.ShowDebugButton("Edit Mode", 200f))
				{
					_editingChallengeId = challenge.Key;
					if (_scrollListStatus != ScrollListOptionState.ModeSelect)
					{
						_scrollListStatus = ScrollListOptionState.ModeSelect;
					}
					else
					{
						_scrollListStatus = ScrollListOptionState.None;
					}
				}
				if (_GUI.ShowDebugButton("Kick Player", 200f))
				{
					_editingChallengeId = challenge.Key;
					if (_scrollListStatus != ScrollListOptionState.KickSelect)
					{
						_scrollListStatus = ScrollListOptionState.KickSelect;
					}
					else
					{
						_scrollListStatus = ScrollListOptionState.None;
					}
				}
				if (_GUI.ShowDebugButton("Block Player", 200f))
				{
					_editingChallengeId = challenge.Key;
					if (_scrollListStatus != ScrollListOptionState.BlockSelect)
					{
						_scrollListStatus = ScrollListOptionState.BlockSelect;
					}
					else
					{
						_scrollListStatus = ScrollListOptionState.None;
					}
				}
			}
			else
			{
				_scrollListStatus = ScrollListOptionState.None;
			}
			GUILayout.EndVertical();
			GUILayout.BeginVertical();
			if (_scrollListStatus != ScrollListOptionState.None && _editingChallengeId == challenge.Key)
			{
				_currentScrollPos = GUILayout.BeginScrollView(_currentScrollPos, false, true);
				switch (_scrollListStatus)
				{
				case ScrollListOptionState.DeckSelect:
				{
					List<Client_Deck> cachedDecks = Pantry.Get<DeckDataProvider>().GetCachedDecks();
					string[] array = cachedDecks.Select((Client_Deck deck) => deck.Summary.Name).ToArray();
					for (int num2 = 0; num2 < array.Length; num2++)
					{
						if (_GUI.ShowDebugButton(array[num2], 200f))
						{
							_challengeController.SetDeckForChallenge(challenge.Value.ChallengeId, cachedDecks[num2].Id);
							_scrollListStatus = ScrollListOptionState.None;
						}
					}
					break;
				}
				case ScrollListOptionState.ModeSelect:
				{
					string[] names = Enum.GetNames(typeof(ChallengeMatchTypes));
					foreach (string text in names)
					{
						if (_GUI.ShowDebugButton(text, 200f))
						{
							if (Enum.TryParse<ChallengeMatchTypes>(text, out var result))
							{
								_challengeController.SetGameSettings(challenge.Key, result, challenge.Value.StartingPlayer, challenge.Value.IsBestOf3);
							}
							_scrollListStatus = ScrollListOptionState.None;
						}
					}
					break;
				}
				case ScrollListOptionState.InviteSelect:
					foreach (KeyValuePair<string, string> player in _players)
					{
						if (challenge.Value.Invites.TryGetValue(player.Value, out var value) && value.Status == InviteStatus.Sent)
						{
							if (_GUI.ShowDebugButton(player.Key + " X", 200f))
							{
								_challengeController.CancelChallengeInvite(challenge.Key, player.Value);
							}
						}
						else if (_GUI.ShowDebugButton(player.Key, 200f))
						{
							_challengeController.AddChallengeInvite(challenge.Key, player.Key, player.Value);
							_challengeController.SendChallengeInvites(challenge.Key);
						}
					}
					break;
				case ScrollListOptionState.KickSelect:
					foreach (KeyValuePair<string, ChallengePlayer> challengePlayer in challenge.Value.ChallengePlayers)
					{
						if (_GUI.ShowDebugButton(challengePlayer.Value.FullDisplayName, 200f))
						{
							_challengeController.KickPlayer(challenge.Key, challengePlayer.Value.PlayerId);
						}
					}
					break;
				case ScrollListOptionState.BlockSelect:
					foreach (KeyValuePair<string, ChallengePlayer> challengePlayer2 in challenge.Value.ChallengePlayers)
					{
						if (_GUI.ShowDebugButton(challengePlayer2.Value.FullDisplayName, 200f))
						{
							_challengeController.BlockPlayer(challenge.Key, challengePlayer2.Value.PlayerId);
						}
					}
					break;
				}
				GUILayout.EndScrollView();
			}
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
	}

	private string GetChallengeJson(PVPChallengeData data)
	{
		return data.ToString();
	}

	private PVPChallengeData GetChallengeData(string data)
	{
		return JsonConvert.DeserializeObject<PVPChallengeData>(data);
	}
}
