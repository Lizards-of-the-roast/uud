using System;
using System.Collections;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.Input;
using Core.Code.Promises;
using MTGA.KeyboardManager;
using UnityEngine;
using UnityEngine.EventSystems;
using Wizards.Arena.Enums.System;
using Wizards.Arena.TcpConnection;
using Wizards.Mtga;
using Wizards.Mtga.Deeplink;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Wotc.Mtga.Login;

public class LoginScene : MonoBehaviour, IKeyDownSubscriber, IKeySubscriber
{
	[Space(5f)]
	[Header("Generic UI Elements")]
	[SerializeField]
	private Transform prefabParent;

	[SerializeField]
	private Transform settingsButtonParent;

	private SettingsButton _settingsButton;

	[Space(5f)]
	[Header("Panels")]
	private RegisterOrLoginPanel _welcomeGate;

	private LoginPanel _logIn;

	private RegistrationPanel _register;

	private ForgotCredentialsPanel _forgotCredentials;

	private HelpPanel _help;

	private BirthLanguagePanel _language;

	private BirthLanguagePanel _updateBirthLanguagePanel;

	private LoginQueuePanel _loginQueue;

	private GameObject _loadingIndicator;

	private UpdatePoliciesPanel _updatePoliciesPanel;

	private Dictionary<PanelType, Panel> _panelsByPanelType;

	public IAccountClient _accountClient;

	public IFrontDoorConnectionServiceWrapper _frontDoorConnection;

	public EnvironmentDescription _currentEnvironment;

	private AssetLookupSystem _assetLookupSystem;

	private MatchManager _matchManager;

	private SettingsMenuHost _settingsMenuHost;

	private ConnectionManager _connectionManager;

	private KeyboardManager _keyboardManager;

	private IBILogger _biLogger;

	private IActionSystem _actions;

	private Panel _currentPanel;

	private bool _exiting;

	private bool _prefabsInitiated;

	public bool IsLoading { get; private set; }

	public PriorityLevelEnum Priority => PriorityLevelEnum.BackButton;

	public string Birthday { get; set; }

	public string SelectedCountry { get; set; }

	public event Action LoggedIn;

	public void Inject(IFrontDoorConnectionServiceWrapper fd, IAccountClient accountClient, EnvironmentDescription currentEnvironment, ConnectionManager connectionManager, AssetLookupSystem assetLookupSystem, KeyboardManager keyboardManager, IActionSystem actions, IBILogger biLogger, MatchManager matchManager, SettingsMenuHost settingsMenuHost)
	{
		_accountClient = accountClient;
		_frontDoorConnection = fd;
		_currentEnvironment = currentEnvironment;
		_assetLookupSystem = assetLookupSystem;
		_connectionManager = connectionManager;
		_keyboardManager = keyboardManager;
		_biLogger = biLogger;
		_matchManager = matchManager;
		_settingsMenuHost = settingsMenuHost;
		_actions = actions;
		Init();
	}

	private void Init()
	{
		if (!_prefabsInitiated)
		{
			_welcomeGate = AssetLoader.Instantiate<RegisterOrLoginPanel>(_assetLookupSystem.GetPrefabPath<RegisterOrLoginPanelPrefab, RegisterOrLoginPanel>(), prefabParent);
			_logIn = AssetLoader.Instantiate<LoginPanel>(_assetLookupSystem.GetPrefabPath<LoginPanelPrefab, LoginPanel>(), prefabParent);
			_register = AssetLoader.Instantiate<RegistrationPanel>(_assetLookupSystem.GetPrefabPath<RegistrationPanelPrefab, RegistrationPanel>(), prefabParent);
			_forgotCredentials = AssetLoader.Instantiate<ForgotCredentialsPanel>(_assetLookupSystem.GetPrefabPath<ForgotCredentialsPanelPrefab, ForgotCredentialsPanel>(), prefabParent);
			_help = AssetLoader.Instantiate<HelpPanel>(_assetLookupSystem.GetPrefabPath<HelpPanelPrefab, HelpPanel>(), prefabParent);
			_language = AssetLoader.Instantiate<BirthLanguagePanel>(_assetLookupSystem.GetPrefabPath<BirthLanguagePanelPrefab, BirthLanguagePanel>(), prefabParent);
			_updateBirthLanguagePanel = AssetLoader.Instantiate<BirthLanguagePanel>(_assetLookupSystem.GetPrefabPath<UpdateBirthLanguagePanelPrefab, BirthLanguagePanel>(), prefabParent);
			_loginQueue = AssetLoader.Instantiate<LoginQueuePanel>(_assetLookupSystem.GetPrefabPath<LoginQueuePanelPrefab, LoginQueuePanel>(), prefabParent);
			_loadingIndicator = AssetLoader.Instantiate(_assetLookupSystem.GetPrefabPath<LoadingPanelPrefab, GameObject>(), prefabParent);
			_updatePoliciesPanel = AssetLoader.Instantiate<UpdatePoliciesPanel>(_assetLookupSystem.GetPrefabPath<UpdatePoliciesPrefab, UpdatePoliciesPanel>(), prefabParent);
			_settingsButton = SettingsButton.Instantiate(_assetLookupSystem, _settingsMenuHost, _matchManager, settingsButtonParent);
			_prefabsInitiated = true;
		}
		_panelsByPanelType = new Dictionary<PanelType, Panel>
		{
			{
				PanelType.WelcomeGate,
				_welcomeGate
			},
			{
				PanelType.LogIn,
				_logIn
			},
			{
				PanelType.Register,
				_register
			},
			{
				PanelType.ForgotCredentials,
				_forgotCredentials
			},
			{
				PanelType.Help,
				_help
			},
			{
				PanelType.BirthLanguage,
				_language
			},
			{
				PanelType.LoginQueue,
				_loginQueue
			},
			{
				PanelType.UpdateBirthLanguage,
				_updateBirthLanguagePanel
			},
			{
				PanelType.UpdatePolicies,
				_updatePoliciesPanel
			}
		};
		_frontDoorConnection.RegisterOnConnectedEvent(OnFrontDoorConnected);
		_frontDoorConnection.RegisterOnAuthFailedEvent(OnAuthFailed);
		SetLoadingState(enabled: false);
		_welcomeGate.Initialize(this, _actions, _keyboardManager, _biLogger);
		_welcomeGate.InjectSettings(_settingsMenuHost);
		_logIn.Initialize(this, _actions, _keyboardManager, _biLogger);
		_register.Initialize(this, _actions, _keyboardManager, _biLogger);
		_forgotCredentials.Initialize(this, _actions, _keyboardManager, _biLogger);
		_help.Initialize(this, _actions, _keyboardManager, _biLogger);
		_language.Initialize(this, _actions, _keyboardManager, _biLogger);
		_updateBirthLanguagePanel.Initialize(this, _actions, _keyboardManager, _biLogger);
		_loginQueue.Initialize(this, _actions, _keyboardManager, _biLogger);
		_updatePoliciesPanel.Initialize(this, _actions, _keyboardManager, _biLogger);
		if (!MDNPlayerPrefs.PLAYERPREFS_HasSelectedInitialLanguage)
		{
			LoadPanel(PanelType.BirthLanguage);
		}
		else
		{
			LoadNextPanelBasedOnLoginState();
		}
		AudioManager.PlayAmbiance();
		AudioManager.SetState("nav_amb", "main_nav");
		_keyboardManager?.Subscribe(this);
		Application.deepLinkActivated += OnDeepLink;
	}

	private void Update()
	{
		if (!_exiting && _accountClient != null && _frontDoorConnection != null)
		{
			bool num = _accountClient.CurrentLoginState == LoginState.FullyRegisteredLogin;
			bool connected = _frontDoorConnection.Connected;
			bool flag = !_frontDoorConnection.IsQueued;
			if (num && connected && flag)
			{
				_exiting = true;
				this.LoggedIn?.Invoke();
			}
		}
	}

	private void OnDestroy()
	{
		if (_frontDoorConnection != null)
		{
			_frontDoorConnection.UnRegisterOnConnectedEvent(OnFrontDoorConnected);
			_frontDoorConnection.UnRegisterAuthFailedEvent(OnAuthFailed);
		}
		_keyboardManager?.Unsubscribe(this);
		Application.deepLinkActivated -= OnDeepLink;
	}

	private void OnFrontDoorConnected()
	{
		LoadNextPanelBasedOnLoginState();
	}

	private void OnAuthFailed(ServerErrors error, string msg)
	{
		if (error == ServerErrors.FD_RequiresPolicyUpdate)
		{
			UpdatePoliciesPanel.NeedsToAcceptPolicy = true;
		}
		LoadNextPanelBasedOnLoginState();
	}

	internal void LoadPanel(PanelType panelType, PanelType panelToReturnTo = PanelType.WelcomeGate)
	{
		if (_panelsByPanelType.TryGetValue(panelType, out var value) && _currentPanel != value)
		{
			_currentPanel?.Hide();
			_currentPanel = value;
			_currentPanel?.Show();
			if (panelToReturnTo != PanelType.None)
			{
				_currentPanel._backButtonPanelType = panelToReturnTo;
			}
			else
			{
				_currentPanel._backButtonPanelType = PanelType.WelcomeGate;
			}
		}
	}

	internal void LoadNextPanelBasedOnLoginState()
	{
		AccountInformation accountInformation = _accountClient.AccountInformation;
		bool loadingState = false;
		if (UpdatePoliciesPanel.NeedsToAcceptPolicy)
		{
			LoadPanel(PanelType.UpdatePolicies);
		}
		else
		{
			switch (_accountClient.CurrentLoginState)
			{
			case LoginState.FullyRegisteredLogin:
				if (_frontDoorConnection.Connected && _frontDoorConnection.IsQueued)
				{
					LoadPanel(PanelType.LoginQueue);
				}
				else if (accountInformation != null && accountInformation.Roles.Length != 0)
				{
					if (_currentPanel != null)
					{
						_currentPanel.Hide();
					}
					loadingState = true;
				}
				else
				{
					LoadPanel(PanelType.WelcomeGate);
				}
				break;
			case LoginState.AnonymousLogin:
				LoadPanel(PanelType.Register);
				break;
			case LoginState.ResetPassword:
			{
				_currentPanel?.Hide();
				_currentPanel = _logIn;
				_logIn.Show(MDNPlayerPrefs.Accounts_LastLogin_Email);
				string localizedText = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/ResetPasswordTitle");
				string localizedText2 = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/ResetPasswordDescription");
				SystemMessageManager.Instance.ShowOk(localizedText, localizedText2);
				break;
			}
			case LoginState.UpdateAgeGateInfo:
				LoadPanel(PanelType.UpdateBirthLanguage);
				break;
			default:
				if (MDNPlayerPrefs.Accounts_LoggedOutReason == "Token Expired")
				{
					_currentPanel?.Hide();
					_currentPanel = _logIn;
					_currentPanel._backButtonPanelType = PanelType.WelcomeGate;
					_logIn.Show(MDNPlayerPrefs.Accounts_LastLogin_Email, "MainNav/Login/TokenExpiredMessage");
				}
				else if (!string.IsNullOrWhiteSpace(MDNPlayerPrefs.Accounts_LastLogin_Email))
				{
					_currentPanel?.Hide();
					_currentPanel = _logIn;
					_currentPanel._backButtonPanelType = PanelType.WelcomeGate;
					_logIn.Show(MDNPlayerPrefs.Accounts_LastLogin_Email);
				}
				else
				{
					LoadPanel(PanelType.WelcomeGate);
				}
				break;
			}
		}
		SetLoadingState(loadingState);
	}

	internal void HandleAccountError(AccountError error, UIWidget_InputField_Registration targetInputField, bool selectInputField)
	{
		Debug.Log("[Accounts - Controller] Account error: " + error.LocalizedErrorMessage);
		if (error.ErrorType == AccountError.ErrorTypes.UpdateRequired)
		{
			SetLoadingState(enabled: false);
			LoadPanel(PanelType.UpdateBirthLanguage);
			return;
		}
		if (error.ErrorType == AccountError.ErrorTypes.ResetPassword)
		{
			string localizedText = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/ResetPasswordTitle");
			string localizedText2 = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/ResetPasswordDescription");
			SystemMessageManager.Instance.ShowOk(localizedText, localizedText2, selectField);
		}
		else
		{
			selectField();
		}
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_invalid, base.gameObject);
		void selectField()
		{
			if (targetInputField != null)
			{
				if (selectInputField && PlatformUtils.GetCurrentDeviceType() != DeviceType.Handheld)
				{
					EventSystem.current.SetSelectedGameObject(targetInputField.InputField.gameObject);
				}
				targetInputField.SetFeedbackText(error.LocalizedErrorMessage);
			}
			SetLoadingState(enabled: false);
		}
	}

	internal void SetLoadingState(bool enabled)
	{
		IsLoading = enabled;
		MainThreadDispatcher.Dispatch(delegate
		{
			_loadingIndicator.UpdateActive(IsLoading);
		});
	}

	public IEnumerator RingDoorbell()
	{
		yield return _connectionManager.RingDoorbell();
	}

	public bool HandleKeyDown(KeyCode curr, Modifiers mods)
	{
		if (curr == KeyCode.Escape && PlatformUtils.IsHandheld())
		{
			return HandleBackButton();
		}
		return false;
	}

	private bool HandleBackButton()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
		SceneLoader.ApplicationQuit();
		return true;
	}

	public void OnDeepLink(string url)
	{
		DeepLinking.LogDeepLinkNotUsed(url, "In Login screen, DeepLink ignored", _biLogger);
	}

	public void ConnectToFrontDoor(AccountInformation accountInfo)
	{
		FDCConnectionParams parameters = new FDCConnectionParams
		{
			Host = _currentEnvironment.fdHost,
			Port = _currentEnvironment.fdPort,
			SessionTicket = accountInfo.Credentials.Jwt,
			ClientVersion = Global.VersionInfo.ContentVersion.ToString(),
			IsDebugAccount = (accountInfo.HasRole_Debugging() || Debug.isDebugBuild),
			AcceptsPolicy = () => RegistrationPanel.PolicyAcceptedThisSession || UpdatePoliciesPanel.PolicyAcceptedThisSession
		};
		_frontDoorConnection.Connect(parameters);
		LoadNextPanelBasedOnLoginState();
	}

	public void ReAttachToFrontDoor()
	{
		if (_frontDoorConnection.ConnectionState != FDConnectionState.Disconnected)
		{
			_frontDoorConnection.Close(TcpConnectionCloseType.Abnormal, "Duplicate Connection");
		}
		AccountInformation accountInformation = _accountClient.AccountInformation;
		ConnectToFrontDoor(accountInformation);
	}
}
