using System.Collections.Generic;
using System.Linq;
using Core.Code.Promises;
using SharedClientCore.SharedClientCore.Code.ClientModels;
using SharedClientCore.SharedClientCore.Code.Providers;
using TMPro;
using UnityEngine;
using Wizards.Arena.Models.Network;
using Wizards.Mtga;

namespace Core.Meta.Social.Tables;

public class TablesPopupUI : PopupBase
{
	[SerializeField]
	private TMP_Text TableNameText;

	[SerializeField]
	private TablesPopupChatWindow ChatWindow;

	[SerializeField]
	private TablesPopupPlayersList PlayersList;

	[SerializeField]
	private LobbyInviteUI PlayerInviteDropdown;

	private TablesCornerUI _tablesCornerUI;

	private Client_Lobby _currentLobby;

	private ILobbyDataProvider LobbyProvider => Pantry.Get<ILobbyDataProvider>();

	public void OnEnable()
	{
		LobbyProvider.LobbyUpdated += OnLobbyUpdated;
		LobbyProvider.LobbyClosed += OnLobbyClosed;
		LobbyProvider.LobbyExited += OnLobbyExited;
	}

	public void OnDisable()
	{
		LobbyProvider.LobbyUpdated -= OnLobbyUpdated;
		LobbyProvider.LobbyClosed -= OnLobbyClosed;
		LobbyProvider.LobbyExited -= OnLobbyExited;
		_currentLobby = null;
	}

	public void SetTableInfo(TablesCornerUI tablesCornerUI, Client_Lobby lobby)
	{
		_tablesCornerUI = tablesCornerUI;
		OnLobbyUpdated(lobby);
	}

	public void OnLobbyUpdated(Client_Lobby lobby)
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			OnLobbyUpdatedMainThread(lobby);
		});
	}

	public void OnLobbyUpdatedMainThread(Client_Lobby lobby)
	{
		if (_currentLobby == null || !(_currentLobby.Lobby.LobbyId != lobby.Lobby.LobbyId))
		{
			_currentLobby = new Client_Lobby
			{
				Lobby = (lobby.Lobby ?? _currentLobby?.Lobby),
				History = (lobby.History ?? _currentLobby?.History)
			};
			TableNameText.text = _currentLobby.Lobby?.LobbyName ?? "Unknown Table";
			PlayersList.UpdatePlayerListViews(_tablesCornerUI, _currentLobby.Lobby?.Players.ToList() ?? new List<LobbyPlayer>(), lobby.Lobby.LobbyId);
			ChatWindow.UpdateLobbyInfo(_currentLobby.Lobby?.LobbyId, _currentLobby.Lobby?.Players.ToList() ?? new List<LobbyPlayer>());
			PlayerInviteDropdown.UpdateLobbyInfo(_currentLobby.Lobby?.LobbyId);
			if (_currentLobby.History != null)
			{
				ChatWindow.OnLobbyHistoryUpdated(_currentLobby.Lobby?.LobbyId, _currentLobby?.History);
			}
		}
	}

	public void OnLobbyClosed(string lobbyId)
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			OnLobbyClosedMainThread(lobbyId);
		});
	}

	public void OnLobbyClosedMainThread(string lobbyId)
	{
		if (_currentLobby != null && _currentLobby.Lobby.LobbyId == lobbyId)
		{
			OnInternalExit();
			_currentLobby = null;
		}
	}

	public void OnLobbyExited(string lobbyId)
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			OnLobbyExitedMainThread(lobbyId);
		});
	}

	public void OnLobbyExitedMainThread(string lobbyId)
	{
		if (_currentLobby != null && _currentLobby.Lobby.LobbyId == lobbyId)
		{
			OnInternalExit();
			_currentLobby = null;
		}
	}

	public override void OnEscape()
	{
		OnInternalExit();
	}

	public override void OnEnter()
	{
	}

	public void OnInternalExit()
	{
		Activate(activate: false);
	}
}
