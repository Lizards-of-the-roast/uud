using Core.Code.Promises;
using UnityEngine;
using _3rdParty.ExternalChat;

namespace Core.Shared.Code.DebugTools;

public class ExternalChatDebugPageGUI : IDebugGUIPage
{
	private DebugInfoIMGUIOnGui _GUI;

	private IExternalChatManager _externalChatManager;

	private ulong _selectedLobbyKvP;

	private string _pendingLobbyName = string.Empty;

	private string _pendingLobbyMessage = string.Empty;

	private static GUIStyle _textInputStyleCache;

	public DebugInfoIMGUIOnGui.DebugTab TabType => DebugInfoIMGUIOnGui.DebugTab.ExternalChat;

	public string TabName => "ExternalChat Debug";

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

	public ExternalChatDebugPageGUI(IExternalChatManager externalChatManager)
	{
		_externalChatManager = externalChatManager;
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
		if (_externalChatManager == null)
		{
			GUILayout.Label("Failed to find valid External Chat Manager");
		}
		GUILayout.BeginHorizontal();
		GUILayout.Label("----------- Connected User Info -----------");
		ExternalChatUser localUserInfo = _externalChatManager.GetLocalUserInfo();
		GUILayout.BeginVertical(GUILayout.ExpandHeight(expand: true));
		GUILayout.Label("Name: " + ((!string.IsNullOrEmpty(localUserInfo.DisplayName)) ? localUserInfo.DisplayName : "unavailable") + " ");
		GUILayout.Label("Id: " + ((localUserInfo.Id != 0L) ? localUserInfo.Id.ToString() : "unavailable") + " ");
		GUILayout.EndVertical();
		if (_GUI.ShowDebugButton("Ask Services for AuthToken", 200f) && _externalChatManager != null)
		{
			_externalChatManager.RequestAuth().ThenOnMainThreadIfSuccess(_externalChatManager.OnAuthResponse);
		}
		GUILayout.EndHorizontal();
		if (!_externalChatManager.IsConnected())
		{
			return;
		}
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical(GUILayout.ExpandHeight(expand: true), GUILayout.MinWidth((float)(int)Screen.safeArea.width / 4f));
		GUILayout.Label("----------- Lobbies -----------");
		foreach (ulong lobbyId in _externalChatManager.GetLobbyIds())
		{
			string lobbySecret = _externalChatManager.GetLobbySecret(lobbyId);
			if (_GUI.ShowDebugButton(lobbySecret, 200f))
			{
				_selectedLobbyKvP = lobbyId;
				_externalChatManager.RefreshLobbyMessages(lobbyId);
				ClearInputFields();
			}
		}
		GUILayout.BeginHorizontal();
		_pendingLobbyName = GUILayout.TextField(_pendingLobbyName, _textInputStyle);
		if (_GUI.ShowDebugButton("Create", 200f))
		{
			_externalChatManager.CreateOrJoinLobby(_pendingLobbyName);
			ClearInputFields();
		}
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		GUILayout.BeginVertical(GUILayout.ExpandHeight(expand: true), GUILayout.MinWidth((float)(int)Screen.safeArea.width / 4f));
		GUILayout.Label("----------- Messages -----------");
		if (_selectedLobbyKvP != 0L)
		{
			GUILayout.Label("----------- " + _externalChatManager.GetLobbySecret(_selectedLobbyKvP) + " -----------");
			foreach (ExternalChatMessage lobbyMessage in _externalChatManager.GetLobbyMessages(_selectedLobbyKvP))
			{
				GUILayout.Label(lobbyMessage.SenderName + lobbyMessage.Content);
			}
		}
		else
		{
			GUILayout.Label("----------- No Lobby Selected -----------");
		}
		GUILayout.BeginHorizontal();
		_pendingLobbyMessage = GUILayout.TextField(_pendingLobbyMessage, _textInputStyle);
		if (_GUI.ShowDebugButton("Send", 200f))
		{
			_externalChatManager.SendLobbyMessage(_selectedLobbyKvP, _pendingLobbyMessage);
			ClearInputFields();
		}
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
	}

	private void ClearInputFields()
	{
		_pendingLobbyName = string.Empty;
		_pendingLobbyMessage = string.Empty;
	}
}
