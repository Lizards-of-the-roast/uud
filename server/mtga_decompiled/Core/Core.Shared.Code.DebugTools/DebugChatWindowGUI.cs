using Core.Meta.MainNavigation.SocialV2;
using Discord.Sdk;
using UnityEngine;
using _3rdParty.ExternalChat;

namespace Core.Shared.Code.DebugTools;

public class DebugChatWindowGUI : IDebugGUIPage
{
	public string messageText = "";

	public string lobbyIdToCheck = "";

	public string lobbyMemberids = "";

	public ChatController chatController;

	private DebugInfoIMGUIOnGui _gui;

	private ulong _lobbyKey;

	public DebugInfoIMGUIOnGui.DebugTab TabType => DebugInfoIMGUIOnGui.DebugTab.DebugChat;

	public string TabName => "External Chat";

	public bool HiddenInTab => false;

	public DebugChatWindowGUI(IExternalChatManager externalChatManager)
	{
		chatController = new ChatController(externalChatManager);
	}

	public void Init(DebugInfoIMGUIOnGui gui)
	{
		_gui = gui;
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
		GUILayout.BeginVertical();
		if (!chatController.IsConnected() && GUILayout.Button("LOGIN TO DISCORD"))
		{
			chatController.Login();
		}
		if (chatController.IsConnected())
		{
			DisplayDiscordTools();
		}
		DisplayLobbyList();
		DisplayChatLog();
		GUILayout.EndVertical();
	}

	private string UlongToStr(ulong[] array)
	{
		if (array == null || array.Length == 0)
		{
			return "No Values in array";
		}
		return "[" + string.Join("\n", array) + "]";
	}

	private void DisplayDiscordTools()
	{
		GUILayout.Label("Discord Tools:");
		GUILayout.BeginHorizontal();
		GUILayout.Label("My Discord ID: ");
		GUILayout.TextArea(chatController.GetChatUserId().ToString());
		GUILayout.EndHorizontal();
		lobbyIdToCheck = GUILayout.TextField(lobbyIdToCheck, GUILayout.Width(300f));
		GUILayout.Label("lobby member ids:" + lobbyMemberids);
		if (GUILayout.Button("Get Members for LobbyId", GUILayout.Width(200f)))
		{
			LobbyHandle lobbyHandle = chatController.DISCORD_GetLobbyHandle(ulong.Parse(lobbyIdToCheck));
			lobbyMemberids = UlongToStr(lobbyHandle.LobbyMemberIds());
		}
	}

	private void DisplayChatLog()
	{
		GUILayout.Label($"Displaying Chat for : {_lobbyKey}");
		GUILayout.TextArea(chatController.PrintMessage(), GUILayout.Width(400f), GUILayout.Height(400f));
		GUILayout.BeginHorizontal();
		if (chatController.GetLobbyIds().Count > 0)
		{
			messageText = GUILayout.TextField(messageText, GUILayout.Width(300f));
			if (GUILayout.Button("Send ->", GUILayout.Width(75f)))
			{
				chatController.SendMessage(_lobbyKey, messageText);
				messageText = "";
			}
		}
		GUILayout.EndHorizontal();
	}

	private void DisplayLobbyList()
	{
		GUILayout.Label("Lobbies:");
		foreach (ulong lobbyId in chatController.GetLobbyIds())
		{
			GUILayout.BeginHorizontal();
			GUILayout.TextArea(chatController.GetLobbyNameFromSecret(lobbyId));
			GUILayout.Label(" : ");
			GUILayout.TextArea(lobbyId.ToString());
			if (GUILayout.Button("Open Chat"))
			{
				_lobbyKey = lobbyId;
				chatController.UpdateMessages(_lobbyKey);
			}
			GUILayout.EndHorizontal();
		}
	}
}
