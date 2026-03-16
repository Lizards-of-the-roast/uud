using System;
using System.Collections.Generic;
using System.Linq;
using Core.Code.Promises;
using Google.Protobuf.WellKnownTypes;
using MTGA.Social;
using SharedClientCore.SharedClientCore.Code.ClientModels;
using SharedClientCore.SharedClientCore.Code.Providers;
using Wizards.Arena.Models.Network;
using Wizards.Arena.Promises;
using Wizards.Mtga;

namespace Core.Meta.Social;

public class LobbyController : ILobbyController, IDisposable
{
	private ISocialManager _socialManager;

	private ILobbyDataProvider _lobbyDataProvider;

	private IBILogger _biLogger;

	public event Action<Client_Lobby> LobbyUpdated;

	public event Action<string> LobbyClosed;

	public event Action<string> LobbyExited;

	public event Action<string, List<LobbyMessage>> HistoryUpdated;

	public event Action<Client_LobbyInvite> NewLobbyInvite;

	public event Action<string> LobbyInviteRemoved;

	public event Action<Client_LobbyKick> LobbyKickPlayer;

	public LobbyController(ILobbyDataProvider lobbyDataProvider, ISocialManager socialManager, IBILogger biLogger = null)
	{
		_socialManager = socialManager;
		_lobbyDataProvider = lobbyDataProvider;
		_biLogger = biLogger;
		_lobbyDataProvider.LobbyUpdated += HandleLobbyUpdated;
		_lobbyDataProvider.HistoryUpdated += HandleHistoryUpdated;
		_lobbyDataProvider.NewLobbyInvite += HandleNewLobbyInvite;
		_lobbyDataProvider.LobbyInviteRemoved += HandleLobbyInviteRemoved;
		_lobbyDataProvider.LobbyKickPlayer += HandleLobbyKicked;
		_lobbyDataProvider.LobbyClosed += HandleLobbyClosed;
		_lobbyDataProvider.LobbyExited += HandleLobbyExited;
	}

	public Promise<Client_Lobby> CreateLobby(string lobbyName, string password)
	{
		return _lobbyDataProvider.SendCreateLobby(lobbyName, password);
	}

	public Promise<Client_Lobby> JoinLobby(string lobbyId, string lobbyName = "")
	{
		return _lobbyDataProvider.SendJoinLobby(lobbyId, lobbyName).Then(delegate(Promise<Client_Lobby> promise)
		{
			if (promise.Successful)
			{
				_lobbyDataProvider.RemoveInvite(lobbyId);
			}
		});
	}

	public Promise<BoolValue> ReconnectLobbies()
	{
		return _lobbyDataProvider.SendReconnectAllLobbies();
	}

	public void ExitLobby(string lobbyId)
	{
		if (!_lobbyDataProvider.Lobbies.TryGetValue(lobbyId, out var value) || value == null)
		{
			SimpleLog.LogError("Attempted to leave an unknown lobby. LobbyId: " + lobbyId);
			return;
		}
		string personaID = Pantry.Get<IAccountClient>().AccountInformation.PersonaID;
		if (value.Lobby.OwnerPlayerId == personaID)
		{
			_lobbyDataProvider.SendCloseLobby(lobbyId).ThenOnMainThread(delegate(Promise<Client_Lobby> promise)
			{
				if (!promise.Successful)
				{
					SystemMessageManager.Instance.ShowOk("Lobby Close Failed", promise.Error.Message);
				}
			});
		}
		else
		{
			_lobbyDataProvider.SendExitLobby(lobbyId);
		}
	}

	public Promise<LobbyMessage> SendLobbyMessage(string lobbyId, string message)
	{
		return _lobbyDataProvider.SendMessage(lobbyId, message);
	}

	public Promise<BoolValue> SendLobbyInvite(string lobbyId, string playerName)
	{
		string playerIdFromDisplayName = GetPlayerIdFromDisplayName(playerName);
		return _lobbyDataProvider.SendLobbyInvite(lobbyId, playerIdFromDisplayName).ThenOnMainThread(delegate(Promise<BoolValue> promise)
		{
			if (promise.Successful)
			{
				SendLobbyMessage(lobbyId, "I invited " + playerName);
			}
			else
			{
				SystemMessageManager.Instance.ShowOk("Invite Failed", promise.Error.Message);
			}
		});
	}

	public Promise<Client_Lobby> KickPlayer(string lobbyId, string playerName)
	{
		return _lobbyDataProvider.SendLobbyKickPlayer(lobbyId, playerName);
	}

	public void ForwardNotificationAlert(SocialMessage socialMessage)
	{
		_socialManager.ForwardNotificationAlert(socialMessage);
	}

	public IReadOnlyDictionary<string, Client_Lobby> GetLobbies()
	{
		return _lobbyDataProvider.Lobbies;
	}

	public IReadOnlyDictionary<string, Client_LobbyInvite> GetLobbyInvites()
	{
		return _lobbyDataProvider.LobbyInvites;
	}

	public List<string> GetInvitableFriends(string lobbyId)
	{
		List<string> list = new List<string>();
		if (!GetLobbies().TryGetValue(lobbyId, out var value) || value == null)
		{
			return list;
		}
		List<SocialEntity> friends = _socialManager.Friends;
		List<LobbyPlayer> source = GetLobbies()[lobbyId].Lobby.Players.ToList();
		foreach (SocialEntity friend in friends)
		{
			if (friend.Status == PresenceStatus.Available && source.Count((LobbyPlayer player) => player.PlayerId == friend.PlayerId) == 0)
			{
				list.Add(friend.FullName);
			}
		}
		return list;
	}

	public List<LobbyPlayer> GetLobbyMembers(string lobbyId)
	{
		if (!GetLobbies().TryGetValue(lobbyId, out var value) || value == null)
		{
			return new List<LobbyPlayer>();
		}
		return value.Lobby.Players.ToList();
	}

	public void RemoveInvite(string lobbyId)
	{
		_lobbyDataProvider.RemoveInvite(lobbyId);
	}

	public void HandleLobbyUpdated(Client_Lobby lobby)
	{
		this.LobbyUpdated?.Invoke(lobby);
	}

	public void HandleHistoryUpdated(string lobbyId, List<LobbyMessage> messageHistory)
	{
		this.HistoryUpdated?.Invoke(lobbyId, messageHistory);
	}

	public void HandleNewLobbyInvite(Client_LobbyInvite lobbyInvite)
	{
		this.NewLobbyInvite?.Invoke(lobbyInvite);
	}

	public void HandleLobbyClosed(string lobbyId)
	{
		this.LobbyClosed?.Invoke(lobbyId);
	}

	public void HandleLobbyExited(string lobbyId)
	{
		this.LobbyExited?.Invoke(lobbyId);
	}

	public void HandleLobbyKicked(Client_LobbyKick lobbyKick)
	{
		this.LobbyKickPlayer?.Invoke(lobbyKick);
	}

	public void HandleLobbyInviteRemoved(string lobbyInvite)
	{
		this.LobbyInviteRemoved?.Invoke(lobbyInvite);
	}

	private string GetPlayerIdFromDisplayName(string displayName)
	{
		string result = "";
		foreach (SocialEntity friend in Pantry.Get<ISocialManager>().Friends)
		{
			if (friend.FullName == displayName)
			{
				return friend.PlayerId;
			}
		}
		return result;
	}

	public void Dispose()
	{
		_lobbyDataProvider.LobbyUpdated -= HandleLobbyUpdated;
		_lobbyDataProvider.HistoryUpdated -= HandleHistoryUpdated;
		_lobbyDataProvider.NewLobbyInvite -= HandleNewLobbyInvite;
		_lobbyDataProvider.LobbyInviteRemoved -= HandleLobbyInviteRemoved;
		_lobbyDataProvider.LobbyKickPlayer -= HandleLobbyKicked;
		_lobbyDataProvider.LobbyClosed -= HandleLobbyClosed;
		_lobbyDataProvider.LobbyExited -= HandleLobbyExited;
	}
}
