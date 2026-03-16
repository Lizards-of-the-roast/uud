using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Core.Meta.Utilities;
using Core.BI;
using Core.Code.Promises;
using Core.Shared.Code.DebugTools;
using UnityEngine;
using UnityEngine.CrashReportHandler;
using Wizards.Arena.Client.Logging;
using Wizards.Arena.Enums.System;
using Wizards.Arena.Promises;
using Wizards.Arena.TcpConnection;
using Wizards.Mtga;
using Wizards.Mtga.BI;
using Wizards.Mtga.Installation;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.CustomInput;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Login;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code.Connection;

public class FrontDoorConnectionManager
{
	private int _lastInputTime;

	private Vector2 _prevPointerPos;

	private bool _idleTimerActive = true;

	private double _idleTimeoutSec = 1200.0;

	private IClientLocProvider _localizationManager;

	private MatchManager _matchManager;

	private LoggingConfig _loggingConfig;

	private Wizards.Arena.Client.Logging.ILogger _crossThreadLogger;

	private IFrontDoorConnectionServiceWrapper _connectionServiceWrapper;

	private IAccountClient _accountClient;

	private EnvironmentDescription _currentEnvironment;

	private bool _disconnecting;

	private AutoLoginState _autoLoginState;

	public double IdleTimeoutSec
	{
		get
		{
			return _idleTimeoutSec;
		}
		set
		{
			_idleTimeoutSec = value;
		}
	}

	public int LastInputTime => _lastInputTime;

	public bool IdleTimerActive
	{
		get
		{
			return _idleTimerActive;
		}
		set
		{
			_idleTimerActive = value;
		}
	}

	public EnvironmentDescription CurrentEnvironment => _currentEnvironment;

	public event Action OnConnected;

	public event Action<TcpConnectionCloseType> OnConnectionLost;

	public event Action<string> RestartRequested;

	public event Action<string> LogoutAndRestartRequested;

	public static FrontDoorConnectionManager Create()
	{
		return new FrontDoorConnectionManager();
	}

	public void Initialize(IClientLocProvider localizationManager, MatchManager matchManager, LoggingConfig loggingConfig, Wizards.Arena.Client.Logging.ILogger crossThreadLogger)
	{
		_localizationManager = localizationManager;
		_matchManager = matchManager;
		_loggingConfig = loggingConfig;
		_crossThreadLogger = crossThreadLogger;
		_lastInputTime = Environment.TickCount;
	}

	private void createFdConnection()
	{
		_connectionServiceWrapper = Pantry.Get<IFrontDoorConnectionServiceWrapper>();
		FDCConnectionConfig config = new FDCConnectionConfig
		{
			ApplicationVersion = Application.version,
			InactivityTimeoutMs = PlayerPrefsExt.GetInt("InactivityTimeoutMs", 60000),
			IsEditor = Application.isEditor,
			MessageCountLimit = 20,
			ClientInfo = new BIClientInfo
			{
				DeviceId = SystemInfo.deviceUniqueIdentifier,
				DeviceModel = SystemInfo.deviceModel,
				OS = SystemInfo.operatingSystem,
				Platform = PlatformUtils.GetClientPlatform()
			}
		};
		_connectionServiceWrapper.CreateFDConnection(config, RecordHistoryUtils.ShouldRecordHistory, _loggingConfig, _crossThreadLogger, OnAuthFailed, OnConnectFailed, OnFrontDoorConnected, OnFrontDoorForceClosed);
	}

	public void SetEnvironment(EnvironmentDescription env, IAccountClient accountClient)
	{
		MDNPlayerPrefs.PreviousFDServer = env.name;
		BILoggingUtils.SetSupplementalInstallID(MDNPlayerPrefs.SupplementalInstallID);
		_matchManager?.Reset();
		CrashReportHandler.SetUserMetadata("environment", _currentEnvironment?.name);
		_accountClient = accountClient;
		_currentEnvironment = env;
		Debug.Log($"Environment set to {env.name}, {env.HostPlatform}");
		createFdConnection();
	}

	public IEnumerator ReconnectYield(Action<AutoLoginState> onComplete)
	{
		_connectionServiceWrapper.RegisterConnectionEvents(this.OnConnected, OnConnectFailed, OnAuthFailed);
		yield return TryFastLogIn(onComplete);
		_connectionServiceWrapper.UnregisterConnectionEvents(this.OnConnected, OnConnectFailed, OnAuthFailed);
	}

	public IEnumerator TryFastLogIn(Action<AutoLoginState> onComplete)
	{
		_autoLoginState = AutoLoginState.None;
		_disconnecting = false;
		_connectionServiceWrapper.RegisterConnectionEvents(onConnected, onConnectFailed, onAuthFailed);
		yield return _accountClient.LogIn_Fast().ThenOnMainThreadIfSuccess(onAccountLoginSuccess).IfError(delegate(Promise<AccountInformation> p)
		{
			onAccountLoginError(p.Error);
		})
			.AsCoroutine();
		while (_autoLoginState == AutoLoginState.None)
		{
			yield return null;
		}
		_connectionServiceWrapper.UnregisterConnectionEvents(onConnected, onConnectFailed, onAuthFailed);
		onComplete?.Invoke(_autoLoginState);
	}

	private void onAccountLoginSuccess(AccountInformation accountInfoResult)
	{
		BIEventTracker.InitializeFirstPartyTracking(accountInfoResult.Email);
		BIEventTracker.TrackEvent(EBiEvent.PlayerLogin, accountInfoResult.PersonaID);
		LoginFlowAnalytics.SendEvent_LoginCompleted();
		MDNPlayerPrefs.Accounts_LoggedOutReason = "INVALID";
		Debug.Log("Connecting to Front Door: " + _currentEnvironment.GetFullUri());
		if (_connectionServiceWrapper.ConnectionState != FDConnectionState.Disconnected)
		{
			BIEventType.FrontDoorConnectionError.SendWithDefaults(("Uri", _currentEnvironment.GetFullUri()), ("Error", "FrontDoor connection request with an unclosed connection."));
			_connectionServiceWrapper.Close(TcpConnectionCloseType.Abnormal, "Duplicate Connection");
		}
		FDCConnectionParams parameters = new FDCConnectionParams
		{
			Host = _currentEnvironment.fdHost,
			Port = _currentEnvironment.fdPort,
			SessionTicket = accountInfoResult.Credentials.Jwt,
			IsDebugAccount = (accountInfoResult.HasRole_Debugging() || Debug.isDebugBuild),
			AcceptsPolicy = () => RegistrationPanel.PolicyAcceptedThisSession || UpdatePoliciesPanel.PolicyAcceptedThisSession
		};
		_connectionServiceWrapper.Connect(parameters);
	}

	private void onAccountLoginError(Error error)
	{
		_autoLoginState = AutoLoginState.Error;
		PromiseExtensions.Logger.Info($"[Accounts - Startup] Fast login error: {error.Code} | {error.Message}\nSending player back to Login screen.");
	}

	private void onConnected()
	{
		_autoLoginState = (_connectionServiceWrapper.IsQueued ? AutoLoginState.InQueue : AutoLoginState.Connected);
	}

	private void onAuthFailed(ServerErrors errorCode, string debugErrorText)
	{
		if (errorCode == ServerErrors.FD_RequiresPolicyUpdate)
		{
			UpdatePoliciesPanel.NeedsToAcceptPolicy = true;
		}
		_autoLoginState = AutoLoginState.Error;
	}

	private void onConnectFailed(string reason)
	{
		_autoLoginState = AutoLoginState.Error;
	}

	public void RestartGame(string context)
	{
		this.RestartRequested?.Invoke(context);
	}

	public void ClearActionsOnRestart()
	{
		this.OnConnected = null;
		this.OnConnectionLost = null;
		this.RestartRequested = null;
		this.LogoutAndRestartRequested = null;
	}

	public void LogoutAndRestartGame(string context)
	{
		this.LogoutAndRestartRequested?.Invoke(context);
	}

	public void Update()
	{
		if (_idleTimerActive && _connectionServiceWrapper.Connected)
		{
			int tickCount = Environment.TickCount;
			if (CustomInputModule.IsAnyInputPressed() || CustomInputModule.GetMouseScroll() != Vector2.zero || CustomInputModule.GetPointerPosition() != _prevPointerPos)
			{
				_lastInputTime = tickCount;
			}
			_prevPointerPos = CustomInputModule.GetPointerPosition();
			if ((double)(tickCount - _lastInputTime) * 0.001 > _idleTimeoutSec)
			{
				_connectionServiceWrapper.Close(TcpConnectionCloseType.ClientSideIdle, "User Idle");
			}
		}
	}

	public void SetIdleTimerActive(bool active)
	{
		_idleTimerActive = active;
		_lastInputTime = Environment.TickCount;
	}

	public void ShowConnectionFailedMessage(string title, string message, bool allowRetry = true, bool exitInsteadOfLogout = false)
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
			Text = _localizationManager.GetLocalizedText("SystemMessage/System_Connection_Lost_Reconnect_Button"),
			Callback = delegate
			{
				this.RestartRequested?.Invoke("Connection failed retry");
			}
		};
		SystemMessageManager.SystemMessageButtonData item3 = new SystemMessageManager.SystemMessageButtonData
		{
			Text = _localizationManager.GetLocalizedText("DuelScene/EscapeMenu/Exit_Button_Text"),
			Callback = SceneLoader.ApplicationQuit
		};
		SystemMessageManager.SystemMessageButtonData item4 = new SystemMessageManager.SystemMessageButtonData
		{
			Text = _localizationManager.GetLocalizedText("MainNav/Settings/LogOut_Button"),
			Callback = delegate
			{
				this.LogoutAndRestartRequested?.Invoke("Connection failed log out button");
			}
		};
		List<SystemMessageManager.SystemMessageButtonData> list = new List<SystemMessageManager.SystemMessageButtonData>();
		list.Add(item);
		if (allowRetry)
		{
			list.Add(item2);
		}
		if (exitInsteadOfLogout)
		{
			if (!PlatformUtils.IsHandheld())
			{
				list.Add(item3);
			}
		}
		else
		{
			list.Add(item4);
		}
		SystemMessageManager.Instance.ShowMessage(title, message, list);
	}

	private void OnAuthFailed(ServerErrors errorCode, string debugErrorText)
	{
		MainThreadDispatcher.Dispatch(delegate
		{
			OnAuthFailedImpl(errorCode, debugErrorText);
		});
	}

	private void OnAuthFailedImpl(ServerErrors errorCode, string debugErrorText)
	{
		_connectionServiceWrapper.Close(TcpConnectionCloseType.NormalClosure, debugErrorText);
		switch (errorCode)
		{
		case ServerErrors.FD_RequiresPolicyUpdate:
			UpdatePoliciesPanel.NeedsToAcceptPolicy = true;
			return;
		case ServerErrors.FD_InvalidClientVersion:
			ShowInvalidClientMessage();
			return;
		case ServerErrors.FD_SystemDown:
			ShowConnectionFailedMessage(_localizationManager.GetLocalizedText("SystemMessage/System_DownForMaintenance"), "", allowRetry: false, exitInsteadOfLogout: true);
			return;
		case ServerErrors.FD_AccessDenied:
			ShowConnectionFailedMessage(_localizationManager.GetLocalizedText("SystemMessage/System_Insufficient_Roles"), _localizationManager.GetLocalizedText("SystemMessage/System_Access_Denied_Permission_Text"), allowRetry: false);
			return;
		case ServerErrors.FD_AlreadyConnected:
			ShowConnectionFailedMessage(_localizationManager.GetLocalizedText("SystemMessage/System_Login_Unable_Title"), _localizationManager.GetLocalizedText("SystemMessage/System_AlreadyConnected"));
			return;
		}
		AccountInformation accountInformation = _accountClient.AccountInformation;
		string item = accountInformation?.DisplayName;
		int num = accountInformation?.DisplayName.LastIndexOf('#') ?? (-1);
		if (num != -1)
		{
			item = accountInformation?.DisplayName.Substring(0, num);
		}
		string localizedText = _localizationManager.GetLocalizedText("SystemMessage/System_Auth_Failure_Text", ("displayName", item), ("error", debugErrorText));
		ShowConnectionFailedMessage(_localizationManager.GetLocalizedText("SystemMessage/System_Login_Unable_Title"), localizedText);
	}

	private void ShowInvalidClientMessage()
	{
		List<SystemMessageManager.SystemMessageButtonData> list = new List<SystemMessageManager.SystemMessageButtonData>();
		SystemMessageManager.SystemMessageButtonData item = new SystemMessageManager.SystemMessageButtonData
		{
			Text = Languages.ActiveLocProvider.GetLocalizedText("DuelScene/EscapeMenu/CheckStatus"),
			Callback = delegate
			{
				UrlOpener.OpenURL(Languages.ActiveLocProvider.GetLocalizedText("MainNav/WebLink/StatusPage"));
			},
			HideOnClick = false
		};
		list.Add(item);
		IInstallationController _installationController = PlatformContext.GetInstallationController();
		if (_installationController.CanForceStartExternalUpdate)
		{
			SystemMessageManager.SystemMessageButtonData item2 = new SystemMessageManager.SystemMessageButtonData
			{
				Text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Popups/Modal/ModalOptions_OK"),
				Callback = delegate
				{
					_installationController.StartExternalUpdate();
				}
			};
			list.Add(item2);
		}
		SystemMessageManager.Instance.ShowMessage(_localizationManager.GetLocalizedText("SystemMessage/System_Invalid_Client_Version_Title"), PlatformContext.GetDistributionServiceString(), list);
	}

	private void OnConnectFailed(string error)
	{
		Debug.Log("ON CONNECT FAILED : " + error);
		_connectionServiceWrapper.Close(TcpConnectionCloseType.Abnormal, error);
		string localizedText = _localizationManager.GetLocalizedText("SystemMessage/System_Environment_Connection_Failure_Text", ("error", error), ("host", _currentEnvironment.fdHost), ("port", _currentEnvironment.fdPort.ToString()));
		ShowConnectionFailedMessage(_localizationManager.GetLocalizedText("SystemMessage/System_Connect_Unable_Title"), localizedText);
	}

	private void OnFrontDoorConnected()
	{
		this.OnConnected?.Invoke();
	}

	private void OnFrontDoorForceClosed(TcpConnectionCloseType closeType, string reason)
	{
		if (!_disconnecting)
		{
			this.OnConnectionLost?.Invoke(closeType);
			_disconnecting = true;
		}
	}
}
