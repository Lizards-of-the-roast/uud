using System.Collections.Generic;
using System.Linq;
using Core.Code.Promises;
using SharedClientCore.SharedClientCore.Code.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.Enums.Player;
using Wizards.Arena.Models.Network;
using Wizards.Mtga;
using Wotc.Mtga.Extensions;

namespace Core.Meta.Social.Tables;

public class TableListTile : MonoBehaviour
{
	[SerializeField]
	private TMP_Text TableNameText;

	[SerializeField]
	private TMP_Text PlayerCountText;

	[SerializeField]
	private Button JoinTableButton;

	[SerializeField]
	private Button OpenTableButton;

	[SerializeField]
	private Button ExitTableButton;

	[SerializeField]
	private GameObject NotificationHighlight;

	private TablesCornerUI _cornerUI;

	private string _lobbyId;

	private Client_Lobby _lobby;

	private Client_LobbyInvite _lobbyInvite;

	private bool _hasActiveNotification;

	private ILobbyController LobbyController => Pantry.Get<ILobbyController>();

	public void Awake()
	{
		JoinTableButton.onClick.AddListener(OnJoinTableClicked);
		OpenTableButton.onClick.AddListener(OnOpenTableClicked);
		ExitTableButton.onClick.AddListener(OnExitTableClicked);
		LobbyController.LobbyUpdated += SetLobbyData;
		LobbyController.HistoryUpdated += OnHistoryUpdated;
	}

	public void OnEnable()
	{
		SetNotifiedHighlight(_hasActiveNotification);
	}

	public void OnDestroy()
	{
		JoinTableButton.onClick.RemoveListener(OnJoinTableClicked);
		OpenTableButton.onClick.RemoveListener(OnOpenTableClicked);
		ExitTableButton.onClick.RemoveListener(OnExitTableClicked);
		LobbyController.LobbyUpdated -= SetLobbyData;
		LobbyController.HistoryUpdated -= OnHistoryUpdated;
	}

	public void Init(TablesCornerUI cornerUI, string lobbyId)
	{
		_cornerUI = cornerUI;
		_lobbyId = lobbyId;
	}

	public void SetLobbyData(Client_Lobby lobby)
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			SetLobbyDataMainThread(lobby);
		});
	}

	public void SetLobbyDataMainThread(Client_Lobby lobby)
	{
		if (!(lobby.Lobby.LobbyId != _lobbyId))
		{
			if (_lobbyInvite != null && lobby.Lobby.LobbyId == _lobbyInvite.LobbyId)
			{
				_lobbyInvite = null;
			}
			UpdateButtons();
			if (lobby.History != null)
			{
				OnHistoryUpdatedMainThread(lobby.Lobby.LobbyId, lobby.History, _lobby != null && !TableUtils.HistoriesMatch(lobby.History, _lobby.History));
			}
			_lobby = new Client_Lobby
			{
				Lobby = (lobby.Lobby ?? _lobby?.Lobby),
				History = (lobby.History ?? _lobby?.History)
			};
			UpdateLobbyText();
		}
	}

	public void SetLobbyInviteData(Client_LobbyInvite lobbyInvite)
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			SetLobbyInviteDataMainThread(lobbyInvite);
		});
	}

	public void SetLobbyInviteDataMainThread(Client_LobbyInvite lobbyInvite)
	{
		_lobbyInvite = lobbyInvite;
		UpdateLobbyInviteText();
		UpdateButtons();
	}

	public void UpdateLobbyText()
	{
		if (_lobby != null)
		{
			TableNameText.text = _lobby.Lobby.LobbyName;
			int num = _lobby.Lobby.Players.Count((LobbyPlayer p) => p.Presence != LobbyPlayerPresence.Unknown);
			int count = _lobby.Lobby.Players.Count;
			PlayerCountText.text = $"Players:  {num} / {count}";
		}
	}

	public void UpdateLobbyInviteText()
	{
		if (_lobbyInvite != null)
		{
			TableNameText.text = "Invited to: " + _lobbyInvite.LobbyName;
			PlayerCountText.text = $"Players: {_lobbyInvite.PlayerCount}";
		}
	}

	private void UpdateButtons()
	{
		if (_lobbyInvite != null)
		{
			JoinTableButton.gameObject.SetActive(value: true);
			OpenTableButton.gameObject.SetActive(value: false);
		}
		else
		{
			JoinTableButton.gameObject.SetActive(value: false);
			OpenTableButton.gameObject.SetActive(value: true);
		}
	}

	public void OnHistoryUpdated(string lobbyId, List<LobbyMessage> lobbyHistory)
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			OnHistoryUpdatedMainThread(lobbyId, lobbyHistory, updated: true);
		});
	}

	public void OnHistoryUpdatedMainThread(string lobbyId, List<LobbyMessage> lobbyHistory, bool updated)
	{
		if (updated && (_lobby == null || _lobby.Lobby.LobbyId == lobbyId) && !_cornerUI.SpecificTableViewVisible)
		{
			SetNotifiedHighlight(value: true);
		}
	}

	public void SetNotifiedHighlight(bool value)
	{
		_hasActiveNotification = value;
		NotificationHighlight.gameObject.UpdateActive(value);
	}

	public void OnJoinTableClicked()
	{
		SetNotifiedHighlight(value: false);
		_cornerUI.JoinTable(_lobbyInvite.LobbyId);
	}

	public void OnOpenTableClicked()
	{
		SetNotifiedHighlight(value: false);
		_cornerUI.OpenSpecificTableView(_lobby);
	}

	public void OnExitTableClicked()
	{
		if (_lobbyInvite != null)
		{
			LobbyController.RemoveInvite(_lobbyInvite.LobbyId);
		}
		else
		{
			LobbyController.ExitLobby(_lobby.Lobby.LobbyId);
		}
	}
}
