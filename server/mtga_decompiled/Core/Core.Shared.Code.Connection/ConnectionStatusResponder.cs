using System;
using System.Collections.Generic;
using Assets.Core.Meta.Utilities;
using UnityEngine;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Loc;

namespace Core.Shared.Code.Connection;

public class ConnectionStatusResponder : MonoBehaviour, IConnectionStatusResponder, IDisposable
{
	private IClientLocProvider _localizationManager;

	private SystemMessageManager _systemMessageManager;

	private ConnectionManager _connectionManager;

	private FrontDoorConnectionManager _frontDoorConnectionManager;

	private bool _disposed;

	public static ConnectionStatusResponder Create()
	{
		GameObject obj = new GameObject("ConnectionStatusResponder");
		UnityEngine.Object.DontDestroyOnLoad(obj);
		return obj.AddComponent<ConnectionStatusResponder>();
	}

	public void Initialize(ConnectionManager connectionManager, FrontDoorConnectionManager frontDoorConnectionManager, IClientLocProvider localizationManager, SystemMessageManager systemMessageManager)
	{
		_connectionManager = connectionManager;
		_frontDoorConnectionManager = frontDoorConnectionManager;
		_localizationManager = localizationManager;
		_systemMessageManager = systemMessageManager;
	}

	public void Dispose()
	{
		if (!_disposed && base.gameObject != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		_disposed = true;
	}

	public void OnDoorbellError()
	{
		SystemMessageManager.SystemMessageButtonData item = new SystemMessageManager.SystemMessageButtonData
		{
			Text = _localizationManager.GetLocalizedText("DuelScene/EscapeMenu/CheckStatus"),
			Callback = delegate
			{
				UrlOpener.OpenURL(_localizationManager.GetLocalizedText("MainNav/WebLink/StatusPage"));
			},
			HideOnClick = false
		};
		SystemMessageManager.SystemMessageButtonData item2 = new SystemMessageManager.SystemMessageButtonData
		{
			Text = _localizationManager.GetLocalizedText("Boot/BootScene_Button_Retry"),
			Callback = delegate
			{
				_frontDoorConnectionManager.RestartGame("Doorbell Error");
			}
		};
		SystemMessageManager.SystemMessageButtonData item3 = new SystemMessageManager.SystemMessageButtonData
		{
			Text = _localizationManager.GetLocalizedText("DuelScene/EscapeMenu/Exit_Button_Text"),
			Callback = SceneLoader.ApplicationQuit
		};
		List<SystemMessageManager.SystemMessageButtonData> list = new List<SystemMessageManager.SystemMessageButtonData>();
		list.Add(item);
		list.Add(item2);
		if (!PlatformUtils.IsHandheld())
		{
			list.Add(item3);
		}
		_systemMessageManager.ShowMessage(_localizationManager.GetLocalizedText("MainNav/General/ErrorTitle"), _localizationManager.GetLocalizedText("Boot/BootScene_Error"), list);
	}

	public void OnConnectionClosedByIdleTimeout()
	{
		ShowUserIdleMessage();
	}

	public void OnConnectionClosedByServer()
	{
		if (!(MatchSceneManager.Instance != null) || MatchSceneManager.Instance.Current == MatchSceneManager.SubScene.None)
		{
			ShowReconnectFailedMessage();
		}
	}

	public void OnReconnectFailed()
	{
		ShowReconnectFailedMessage();
	}

	public void NoActiveMatchFound()
	{
		if (MatchSceneManager.Instance != null)
		{
			MatchSceneManager.Instance.ExitMatchScene();
		}
	}

	public void OnMatchReconnectFailed()
	{
		_systemMessageManager.ShowOk(_localizationManager.GetLocalizedText("SystemMessage/System_Network_Error_Title"), _localizationManager.GetLocalizedText("SystemMessage/System_ClientMessageParseError"), onMatchReconnectFailedConfirmed);
	}

	private void onMatchReconnectFailedConfirmed()
	{
		MatchSceneManager.Instance.ExitMatchScene();
	}

	private void ShowReconnectFailedMessage()
	{
		string localizedText = _localizationManager.GetLocalizedText("SystemMessage/System_Connection_Lost_Title");
		string localizedText2 = _localizationManager.GetLocalizedText("SystemMessage/System_Connection_Lost_Text");
		SystemMessageManager.SystemMessageButtonData item = new SystemMessageManager.SystemMessageButtonData
		{
			Text = _localizationManager.GetLocalizedText("DuelScene/EscapeMenu/CheckStatus"),
			Callback = delegate
			{
				UrlOpener.OpenURL(_localizationManager.GetLocalizedText("MainNav/WebLink/StatusPage"));
			},
			HideOnClick = false
		};
		SystemMessageManager.SystemMessageButtonData item2 = new SystemMessageManager.SystemMessageButtonData
		{
			Text = _localizationManager.GetLocalizedText("SystemMessage/System_Connection_Lost_Reconnect_Button"),
			Callback = OnReconnectClicked
		};
		SystemMessageManager.SystemMessageButtonData systemMessageButtonData = new SystemMessageManager.SystemMessageButtonData
		{
			Text = _localizationManager.GetLocalizedText("DuelScene/EscapeMenu/Exit_Button_Text"),
			Callback = SceneLoader.ApplicationQuit
		};
		SystemMessageManager.SystemMessageButtonData systemMessageButtonData2 = new SystemMessageManager.SystemMessageButtonData
		{
			Text = _localizationManager.GetLocalizedText("MainNav/Settings/LogOut_Button"),
			Callback = delegate
			{
				_frontDoorConnectionManager.LogoutAndRestartGame("Connection failed log out button");
			}
		};
		List<SystemMessageManager.SystemMessageButtonData> list = new List<SystemMessageManager.SystemMessageButtonData>();
		list.Add(item);
		list.Add(item2);
		list.Add(PlatformUtils.IsHandheld() ? systemMessageButtonData2 : systemMessageButtonData);
		_systemMessageManager.ShowMessage(localizedText, localizedText2, list);
	}

	private void ShowUserIdleMessage()
	{
		_systemMessageManager.ShowMessage(_localizationManager.GetLocalizedText("SystemMessage/System_Network_IdleDisconnect_Title"), _localizationManager.GetLocalizedText("SystemMessage/System_Network_IdleDisconnect_Text"), _localizationManager.GetLocalizedText("SystemMessage/System_Network_IdleDisconnect_ReconnectButton_Text"), OnReconnectClicked);
	}

	private void OnReconnectClicked()
	{
		_connectionManager.Reconnect();
	}
}
