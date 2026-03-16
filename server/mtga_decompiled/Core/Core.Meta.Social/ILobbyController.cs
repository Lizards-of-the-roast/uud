using System;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using MTGA.Social;
using SharedClientCore.SharedClientCore.Code.ClientModels;
using Wizards.Arena.Models.Network;
using Wizards.Arena.Promises;

namespace Core.Meta.Social;

public interface ILobbyController
{
	event Action<Client_Lobby> LobbyUpdated;

	event Action<string, List<LobbyMessage>> HistoryUpdated;

	event Action<Client_LobbyInvite> NewLobbyInvite;

	event Action<string> LobbyClosed;

	event Action<string> LobbyExited;

	event Action<string> LobbyInviteRemoved;

	event Action<Client_LobbyKick> LobbyKickPlayer;

	Promise<Client_Lobby> CreateLobby(string lobbyName, string password);

	Promise<Client_Lobby> JoinLobby(string lobbyId, string lobbyName = "");

	Promise<BoolValue> ReconnectLobbies();

	Promise<LobbyMessage> SendLobbyMessage(string lobbyId, string message);

	Promise<BoolValue> SendLobbyInvite(string lobbyId, string playerName);

	Promise<Client_Lobby> KickPlayer(string lobbyId, string playerName);

	void RemoveInvite(string lobbyId);

	IReadOnlyDictionary<string, Client_Lobby> GetLobbies();

	IReadOnlyDictionary<string, Client_LobbyInvite> GetLobbyInvites();

	void ExitLobby(string lobbyId);

	List<string> GetInvitableFriends(string lobbyId);

	List<LobbyPlayer> GetLobbyMembers(string lobbyId);

	void ForwardNotificationAlert(SocialMessage socialMessage);
}
