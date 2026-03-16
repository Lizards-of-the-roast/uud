using System.Collections.Generic;
using System.Linq;
using Core.Code.Promises;
using Core.Meta.MainNavigation.SocialV2.Models;
using Discord.Sdk;
using _3rdParty.Discord;
using _3rdParty.ExternalChat;

namespace Core.Meta.MainNavigation.SocialV2;

public class ChatController
{
	private List<ChatMessage> _messages = new List<ChatMessage>();

	private IExternalChatManager _externalChatManager;

	private const string LobbyName = "Test Lobby 1";

	public ChatController(IExternalChatManager externalChatManager)
	{
		_externalChatManager = externalChatManager;
		_externalChatManager.OnLobbyJoinedEvent += RefreshLobbyMessages;
		_externalChatManager.OnRefreshMessagesSuccess += UpdateMessages;
		_externalChatManager.OnClientReady += OnClientReady;
	}

	public List<ulong> GetLobbyIds()
	{
		return _externalChatManager.GetLobbyIds();
	}

	public void Login()
	{
		_externalChatManager.RequestAuth().ThenOnMainThreadIfSuccess(_externalChatManager.OnAuthResponse);
	}

	public void CreateOrJoinLobby(string lobbySecret)
	{
		_externalChatManager.CreateOrJoinLobby(lobbySecret);
	}

	public ulong GetFirstLobbyKey()
	{
		return GetLobbyIds().FirstOrDefault();
	}

	public void RefreshLobbyMessages(ulong lobbyId)
	{
		_externalChatManager.RefreshLobbyMessages(lobbyId);
	}

	public string GetLobbyNameFromSecret(ulong lobbyKey)
	{
		return _externalChatManager.GetLobbySecret(lobbyKey);
	}

	public void UpdateMessages(ulong lobbyKey)
	{
		List<ExternalChatMessage> lobbyMessages = _externalChatManager.GetLobbyMessages(lobbyKey);
		_messages.Clear();
		foreach (ExternalChatMessage item2 in lobbyMessages)
		{
			ChatMessage item = new ChatMessage
			{
				AuthorName = item2.SenderName,
				MessageContent = item2.Content
			};
			_messages.Add(item);
		}
	}

	public void SendMessage(ulong lobbyKey, string message)
	{
		_externalChatManager.SendLobbyMessage(lobbyKey, message);
		RefreshLobbyMessages(lobbyKey);
	}

	public string PrintMessage()
	{
		string text = "";
		foreach (ChatMessage message in _messages)
		{
			text = text + message.AuthorName + ": " + message.MessageContent + "\n";
		}
		return text;
	}

	private void OnClientReady()
	{
		CreateOrJoinLobby("Test Lobby 1");
	}

	public bool IsConnected()
	{
		return _externalChatManager.IsConnected();
	}

	public void OnDestroy()
	{
		_externalChatManager.OnLobbyJoinedEvent -= RefreshLobbyMessages;
		_externalChatManager.OnRefreshMessagesSuccess -= UpdateMessages;
	}

	public ulong GetChatUserId()
	{
		return _externalChatManager.GetLocalUserInfo().Id;
	}

	public LobbyHandle DISCORD_GetLobbyHandle(ulong lobbyId)
	{
		return ((DiscordManager)_externalChatManager).GetLobbyHandle(lobbyId);
	}
}
