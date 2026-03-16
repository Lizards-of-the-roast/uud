using System;
using System.Collections.Generic;
using Core.Code.ClientFeatureToggle;
using Core.Meta.Social;
using Core.Shared.Code.WrapperFactories;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using SharedClientCore.SharedClientCore.Code.ClientModels;
using UnityEngine;
using Wizards.Arena.Models.Network;
using Wizards.Arena.Promises;
using Wizards.Mtga;

namespace Core.Shared.Code.DebugTools;

public class LobbyTablePageGUI : IDebugGUIPage
{
	private enum DropdownOptionState
	{
		None,
		InviteFriend,
		KickPlayer
	}

	private DebugInfoIMGUIOnGui _GUI;

	private static GUIStyle _textInputStyleCache;

	private static GUIStyle _guiStyleWordWrapWhite;

	private DropdownOptionState _dropDownStatus;

	private DropdownOptionState _kickDropdownStatus;

	private Vector2 _currentDropdownScrollPos = Vector2.zero;

	private Vector2 _currentKickDropdownScrollPos = Vector2.zero;

	private Dictionary<string, Vector2> _lobbyJsonScrollPos = new Dictionary<string, Vector2>();

	private Vector2 _currentScrollPos = Vector2.zero;

	private string _lobbyPassword = "TestPassword";

	private IReadOnlyDictionary<string, Client_Lobby> _lobbies;

	private IReadOnlyDictionary<string, Client_LobbyInvite> _lobbyInvites;

	private ILobbyController _lobbyController;

	private string _createLobbyName = "TestLobby";

	private string _joinLobbyName = "TestLobby";

	private string _lobbyMessage = "TestLobbyMessage";

	private Promise<Client_Lobby> _clientLobbyPromise;

	private Promise<LobbyMessage> _lobbyMessagePromise;

	private string _promiseOutcome = "";

	private string _lobbyIdMessageNotification;

	private string _lobbyIdUpdateNotification;

	private DateTime _lobbyNotificationTime;

	private bool _lobbyFeatureToggle;

	private string _focusedLobby = "";

	public DebugInfoIMGUIOnGui.DebugTab TabType => DebugInfoIMGUIOnGui.DebugTab.Lobbies;

	public string TabName => "Lobbies";

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
		_lobbyController = LobbyWrapperFactory.CreateController();
		if (_lobbyController != null)
		{
			_lobbyController.LobbyUpdated += HandleLobbyUpdated;
			_lobbyController.HistoryUpdated += HandleHistoryUpdated;
		}
	}

	public void Destroy()
	{
	}

	public void OnQuit()
	{
		_lobbyController.LobbyUpdated -= HandleLobbyUpdated;
		_lobbyController.HistoryUpdated -= HandleHistoryUpdated;
	}

	public bool OnUpdate()
	{
		ClientFeatureToggleDataProvider clientFeatureToggleDataProvider = Pantry.Get<ClientFeatureToggleDataProvider>();
		if (clientFeatureToggleDataProvider != null)
		{
			_lobbyFeatureToggle = clientFeatureToggleDataProvider.GetToggleValueById("Lobby");
		}
		return true;
	}

	public void OnGUI()
	{
		if (!_lobbyFeatureToggle)
		{
			GUILayout.Label("Client feature toggle for Lobby is disabled");
			return;
		}
		_lobbies = _lobbyController.GetLobbies();
		_lobbyInvites = _lobbyController.GetLobbyInvites();
		GUILayout.BeginVertical();
		using (new GUILayout.HorizontalScope())
		{
			if (_GUI.ShowDebugButton("Create Lobby", 200f))
			{
				ClearStatus();
				_clientLobbyPromise = _lobbyController.CreateLobby(_createLobbyName, _lobbyPassword).Then(delegate(Promise<Client_Lobby> p)
				{
					if (p.Successful)
					{
						_promiseOutcome = "Successfully created lobby.";
					}
					else
					{
						_promiseOutcome = "Failed to create lobby: " + p.Error.Message;
					}
				});
			}
			_createLobbyName = GUILayout.TextField(_createLobbyName, _textInputStyle);
		}
		using (new GUILayout.HorizontalScope())
		{
			if (_GUI.ShowDebugButton("Join Lobby", 200f))
			{
				ClearStatus();
				_clientLobbyPromise = _lobbyController.JoinLobby("", _joinLobbyName).Then(delegate(Promise<Client_Lobby> p)
				{
					if (p.Successful)
					{
						_promiseOutcome = "Successfully joined lobby.";
					}
					else
					{
						_promiseOutcome = "Failed to join lobby: " + p.Error.Message;
					}
				});
			}
			_joinLobbyName = GUILayout.TextField(_joinLobbyName, _textInputStyle);
		}
		using (new GUILayout.HorizontalScope())
		{
			if (_GUI.ShowDebugButton("Reconnect Lobbies", 200f))
			{
				ClearStatus();
				_lobbyController.ReconnectLobbies().Then(delegate(Promise<BoolValue> p)
				{
					if (p.Successful)
					{
						_promiseOutcome = "Successfully reconnected lobbies.";
					}
					else
					{
						_promiseOutcome = "Failed to reconnect lobbies: " + p.Error.Message;
					}
				});
			}
		}
		GUILayout.Label("----------- Notifications -----------");
		GUILayout.Label("----------- Lobby Update Notifications -----------");
		if (_lobbyIdUpdateNotification != null)
		{
			GUILayout.Label(_lobbyIdUpdateNotification + " " + _lobbyNotificationTime);
		}
		GUILayout.Label("----------- Lobby History Update Notifications -----------");
		if (_lobbyIdMessageNotification != null)
		{
			GUILayout.Label(_lobbyIdMessageNotification + " " + _lobbyNotificationTime);
		}
		GUILayout.Label("----------- Promise States -----------");
		GUILayout.Label("----------- Promise<Client_Lobby> -----------");
		GUILayout.Label(GetPromiseState(_clientLobbyPromise));
		GUILayout.Label("----------- Promise<LobbyMessage> -----------");
		GUILayout.Label(GetPromiseState(_lobbyMessagePromise));
		GUILayout.Label("----------- Promise Outcome -----------");
		GUILayout.Label(_promiseOutcome);
		GUILayout.Label("----------- Lobbies -----------");
		using GUILayout.ScrollViewScope scrollViewScope = new GUILayout.ScrollViewScope(_currentScrollPos, GUILayout.ExpandHeight(expand: true));
		GUILayout.Label("----------- Lobby invites -----------");
		_currentScrollPos = scrollViewScope.scrollPosition;
		foreach (KeyValuePair<string, Client_LobbyInvite> lobbyInvite in _lobbyInvites)
		{
			GUILayout.Label(lobbyInvite.Value.InviterName + " invited you to " + lobbyInvite.Value.LobbyName);
			if (_GUI.ShowDebugButton("Accept", 200f))
			{
				_lobbyController.JoinLobby(lobbyInvite.Key);
				break;
			}
			if (_GUI.ShowDebugButton("Decline", 200f))
			{
				_lobbyController.RemoveInvite(lobbyInvite.Key);
				break;
			}
		}
		foreach (KeyValuePair<string, Client_Lobby> lobby in _lobbies)
		{
			GUILayout.Label(lobby.Value.Lobby.LobbyName);
			using (new GUILayout.HorizontalScope(GUILayout.MinHeight(300f), GUILayout.MaxHeight(500f)))
			{
				if (!_lobbyJsonScrollPos.TryGetValue(lobby.Key, out var value))
				{
					value = Vector2.zero;
				}
				using (new GUILayout.HorizontalScope(GUILayout.MaxWidth(500f)))
				{
					using GUILayout.ScrollViewScope scrollViewScope2 = new GUILayout.ScrollViewScope(value, GUILayout.ExpandHeight(expand: true));
					_lobbyJsonScrollPos[lobby.Key] = scrollViewScope2.scrollPosition;
					GUILayout.TextField(GetLobbyJson(lobby.Value), _textInputStyle);
				}
				using (new GUILayout.VerticalScope())
				{
					using (new GUILayout.HorizontalScope())
					{
						if (_GUI.ShowDebugButton("Send Lobby Message", 200f))
						{
							ClearStatus();
							_lobbyMessagePromise = _lobbyController.SendLobbyMessage(lobby.Key, _lobbyMessage).Then(delegate(Promise<LobbyMessage> p)
							{
								if (p.Successful)
								{
									_promiseOutcome = "Successfully sent message to lobby .";
								}
								else
								{
									_promiseOutcome = "Failed to send message to lobby: " + p.Error.Message;
								}
							});
						}
						_lobbyMessage = GUILayout.TextField(_lobbyMessage, _textInputStyle, GUILayout.MaxWidth(300f));
					}
					using (new GUILayout.HorizontalScope())
					{
						if (_GUI.ShowDebugButton("Exit", 200f))
						{
							ClearStatus();
							_lobbyController.ExitLobby(lobby.Key);
						}
						List<string> invitableFriends = _lobbyController.GetInvitableFriends(lobby.Value.Lobby.LobbyId);
						if (_GUI.ShowDebugButton("Invite", 200f))
						{
							_focusedLobby = lobby.Key;
							if (_dropDownStatus != DropdownOptionState.InviteFriend)
							{
								_dropDownStatus = DropdownOptionState.InviteFriend;
							}
							else
							{
								_dropDownStatus = DropdownOptionState.None;
							}
						}
						if (invitableFriends.Count > 0)
						{
							if (_dropDownStatus == DropdownOptionState.InviteFriend && _focusedLobby == lobby.Key)
							{
								_currentDropdownScrollPos = GUILayout.BeginScrollView(_currentDropdownScrollPos, false, true, GUILayout.MaxWidth(201f));
								if (_dropDownStatus == DropdownOptionState.InviteFriend)
								{
									for (int num = 0; num < invitableFriends.Count; num++)
									{
										if (_GUI.ShowDebugButton(invitableFriends[num], 200f))
										{
											_lobbyController.SendLobbyInvite(lobby.Key, invitableFriends[num]);
											_dropDownStatus = DropdownOptionState.None;
										}
									}
								}
								GUILayout.EndScrollView();
							}
						}
						else
						{
							GUILayout.Label("No friends to invite");
						}
					}
					using (new GUILayout.HorizontalScope())
					{
						if (_GUI.ShowDebugButton("Send Lobby Message", 200f))
						{
							ClearStatus();
							_lobbyMessagePromise = _lobbyController.SendLobbyMessage(lobby.Key, _lobbyMessage).Then(delegate(Promise<LobbyMessage> p)
							{
								if (p.Successful)
								{
									_promiseOutcome = "Successfully sent message to lobby .";
								}
								else
								{
									_promiseOutcome = "Failed to send message to lobby: " + p.Error.Message;
								}
							});
						}
						_lobbyMessage = GUILayout.TextField(_lobbyMessage, _textInputStyle, GUILayout.MaxWidth(300f));
					}
					using (new GUILayout.HorizontalScope())
					{
						List<LobbyPlayer> lobbyMembers = _lobbyController.GetLobbyMembers(lobby.Value.Lobby.LobbyId);
						if (_GUI.ShowDebugButton("Kick", 200f))
						{
							_focusedLobby = lobby.Key;
							if (_kickDropdownStatus != DropdownOptionState.KickPlayer)
							{
								_kickDropdownStatus = DropdownOptionState.KickPlayer;
							}
							else
							{
								_kickDropdownStatus = DropdownOptionState.None;
							}
						}
						if (lobbyMembers.Count > 0)
						{
							if (_kickDropdownStatus != DropdownOptionState.KickPlayer || !(_focusedLobby == lobby.Key))
							{
								continue;
							}
							_currentKickDropdownScrollPos = GUILayout.BeginScrollView(_currentKickDropdownScrollPos, false, true, GUILayout.MaxWidth(201f));
							if (_kickDropdownStatus == DropdownOptionState.KickPlayer)
							{
								for (int num2 = 0; num2 < lobbyMembers.Count; num2++)
								{
									if (_GUI.ShowDebugButton(lobbyMembers[num2].DisplayName, 200f))
									{
										_lobbyController.KickPlayer(lobby.Key, lobbyMembers[num2].PlayerId);
										_kickDropdownStatus = DropdownOptionState.None;
									}
								}
							}
							GUILayout.EndScrollView();
							continue;
						}
						GUILayout.Label("No members to kick");
					}
				}
			}
		}
		GUILayout.EndVertical();
	}

	private string GetLobbyJson(Client_Lobby lobbyValue)
	{
		return JsonConvert.SerializeObject(lobbyValue, Formatting.Indented);
	}

	private string GetPromiseState(Promise<Client_Lobby> lobbyPromise)
	{
		return lobbyPromise?.State.ToString();
	}

	private void ClearStatus()
	{
		_clientLobbyPromise = null;
		_promiseOutcome = null;
		_lobbyIdMessageNotification = null;
		_lobbyIdUpdateNotification = null;
	}

	private string GetPromiseState(Promise<LobbyMessage> lobbyMessagePromise)
	{
		return lobbyMessagePromise?.State.ToString();
	}

	public void HandleLobbyUpdated(Client_Lobby lobby)
	{
		_lobbyIdUpdateNotification = lobby?.Lobby.LobbyId;
		_lobbyNotificationTime = DateTime.Now;
	}

	public void HandleHistoryUpdated(string lobbyId, List<LobbyMessage> messageHistory)
	{
		_lobbyIdMessageNotification = lobbyId;
		_lobbyNotificationTime = DateTime.Now;
	}
}
