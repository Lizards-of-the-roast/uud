using System.Collections;
using Assets.Core.Meta.Utilities;
using Core.BI;
using Core.Code.Input;
using MTGA.KeyboardManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WAS;
using Wizards.Arena.Promises;
using Wizards.Arena.TcpConnection;
using Wizards.Mtga;
using Wizards.Mtga.BI;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Wotc.Mtga.Login;

public class LoginPanel : Panel
{
	private const string FORGET_EMAIL = "";

	[SerializeField]
	private UIWidget_InputField_Registration _email_inputField;

	[SerializeField]
	private Animator _email_animator;

	[SerializeField]
	private TMP_InputField _email_TMPInput;

	[SerializeField]
	private UIWidget_InputField_Registration _password_inputField;

	[SerializeField]
	private Toggle _rememberMe_Toggle;

	[SerializeField]
	private Localize _loginContextLabel;

	[SerializeField]
	private GameObject _backButton;

	[SerializeField]
	private CustomButton _privacyPolicyButton;

	private Color _enabledInputColor = new Color(0.8f, 0.93f, 0.98f);

	private bool _submitting;

	private bool ForceHideRememberMe => PlatformUtils.IsHandheld();

	protected override GameObject SelectOnLoad
	{
		get
		{
			if (!_email_TMPInput.enabled)
			{
				return _password_inputField.InputField.gameObject;
			}
			return _email_inputField.InputField.gameObject;
		}
	}

	public override void Initialize(LoginScene controllerLogin, IActionSystem actions, KeyboardManager keyboardManager, IBILogger biLogger)
	{
		_email_inputField.InputField.onSelect.AddListener(base.onInputField);
		_password_inputField.InputField.onSelect.AddListener(base.onInputField);
		_email_inputField.InputField.onSelect.AddListener(_email_select);
		_password_inputField.InputField.onSelect.AddListener(_password_select);
		_email_inputField.InputField.onEndEdit.AddListener(_email_endEdit);
		_password_inputField.InputField.onEndEdit.AddListener(_password_endEdit);
		_privacyPolicyButton.OnClick.AddListener(OnPrivacyPolicyClicked);
		_email_inputField.Initialize(actions);
		_password_inputField.Initialize(actions);
		base.Initialize(controllerLogin, actions, keyboardManager, biLogger);
		AttemptEditorQuickLogin();
	}

	private void OnDestroy()
	{
		_email_inputField.InputField.onSelect.RemoveListener(base.onInputField);
		_password_inputField.InputField.onSelect.RemoveListener(base.onInputField);
		_email_inputField.InputField.onSelect.RemoveListener(_email_select);
		_password_inputField.InputField.onSelect.RemoveListener(_password_select);
		_email_inputField.InputField.onEndEdit.RemoveListener(_email_endEdit);
		_password_inputField.InputField.onEndEdit.RemoveListener(_password_endEdit);
		_privacyPolicyButton.OnClick.RemoveListener(OnPrivacyPolicyClicked);
	}

	private void _password_select(string arg0)
	{
		_password_inputField.ClearFeedbackText();
	}

	private void _email_select(string arg0)
	{
		_email_inputField.ClearFeedbackText();
	}

	private void _email_endEdit(string arg0)
	{
		if (string.IsNullOrWhiteSpace(_email_inputField.InputField.text))
		{
			_email_inputField.SetFeedbackText(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Email_Required"));
			EnableButton(enabled: false);
		}
		else
		{
			LoginFlowAnalytics.SendEvent_Login("login_email_entered");
			_email_inputField.ClearFeedbackText();
		}
	}

	private void _password_endEdit(string arg0)
	{
		if (string.IsNullOrWhiteSpace(_password_inputField.InputField.text))
		{
			_password_inputField.SetFeedbackText(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Password_Required"));
			EnableButton(enabled: false);
		}
		else
		{
			LoginFlowAnalytics.SendEvent_Login("login_password_entered");
		}
	}

	public void Show(params string[] values)
	{
		_backButton.SetActive(_loginScene._accountClient.AllowAccountCreation);
		LoginFlowAnalytics.SendEvent_Login("login_begin");
		_email_inputField.InputField.text = string.Empty;
		_password_inputField.InputField.text = string.Empty;
		_password_inputField.ClearFeedbackText();
		_email_inputField.ClearFeedbackText();
		if (values == null || values.Length == 0)
		{
			_rememberMe_Toggle.transform.parent.gameObject.UpdateActive(!ForceHideRememberMe);
			_email_animator.enabled = true;
			_email_TMPInput.enabled = true;
			_email_inputField.InputField.textComponent.color = _enabledInputColor;
			_loginContextLabel.gameObject.UpdateActive(active: false);
		}
		else
		{
			if (values[0] == "")
			{
				_rememberMe_Toggle.isOn = false;
			}
			else
			{
				_email_inputField.InputField.text = values[0];
				_rememberMe_Toggle.isOn = true;
			}
			if (values.Length > 1 && !string.IsNullOrEmpty(values[1]))
			{
				_loginContextLabel.SetText(values[1]);
			}
		}
		base.Show();
	}

	public override void Show()
	{
		_backButton.SetActive(_loginScene._accountClient.AllowAccountCreation);
		LoginFlowAnalytics.SendEvent_Login("login_begin");
		_email_inputField.InputField.text = string.Empty;
		_password_inputField.InputField.text = string.Empty;
		_password_inputField.ClearFeedbackText();
		_email_inputField.ClearFeedbackText();
		_rememberMe_Toggle.transform.parent.gameObject.UpdateActive(!ForceHideRememberMe);
		_email_animator.enabled = true;
		_email_TMPInput.enabled = true;
		_email_inputField.InputField.textComponent.color = _enabledInputColor;
		_loginContextLabel.gameObject.UpdateActive(active: false);
		base.Show();
	}

	public void OnButton_LogInWithCredentials()
	{
		_email_inputField.ClearFeedbackText();
		_password_inputField.ClearFeedbackText();
		string text = _email_inputField.InputField.text;
		string text2 = _password_inputField.InputField.text;
		bool isOn = _rememberMe_Toggle.isOn;
		_loginScene.SetLoadingState(enabled: true);
		StartCoroutine(Coroutine_LogInWithCredentials(text, text2, ForceHideRememberMe || isOn));
		EnableButton(enabled: false);
		LoginFlowAnalytics.SendEvent_Login("login_attempted");
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, base.gameObject);
	}

	private void OnPrivacyPolicyClicked()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		UrlOpener.OpenURL(Languages.ActiveLocProvider.GetLocalizedText("MainNav/WebLink/PrivacyPolicy"));
	}

	public void AttemptEditorQuickLogin()
	{
	}

	private IEnumerator Coroutine_LogInWithCredentials(string email, string password, bool rememberLogin)
	{
		Debug.Log("[Accounts - Login] Attempting Login with credentials.");
		_submitting = true;
		yield return _loginScene.RingDoorbell();
		Promise<AccountInformation> loginPromise = _loginScene._accountClient.LogIn_Credentials(email, password);
		yield return loginPromise.AsCoroutine();
		if (loginPromise.Error.IsError)
		{
			OnAccountLoginError(WASUtils.ToAccountError(loginPromise.Error));
		}
		else
		{
			OnAccountLoginSuccess(loginPromise.Result, email, rememberLogin);
		}
		_submitting = false;
	}

	private void OnAccountLoginSuccess(AccountInformation accountInfoResult, string email, bool rememberLogin)
	{
		LoginFlowAnalytics.SendEvent_LoginCompleted();
		Debug.LogFormat("[Accounts - Login] Logged in successfully. Display Name: {0}", accountInfoResult.DisplayName);
		_loginScene._accountClient.RememberMe = rememberLogin;
		MDNPlayerPrefs.Accounts_LastLogin_Email = (rememberLogin ? email : "");
		MDNPlayerPrefs.Accounts_LoggedOutReason = "INVALID";
		MDNPlayerPrefs.PLAYERPREFS_HasSelectedInitialLanguage = true;
		BIEventTracker.TrackEvent(EBiEvent.PlayerLogin, accountInfoResult.PersonaID);
		EnvironmentDescription currentEnvironment = _loginScene._currentEnvironment;
		if (currentEnvironment == null)
		{
			SendEvent_FrontDoorConnectionError("", "Current environment is null");
			return;
		}
		IFrontDoorConnectionServiceWrapper frontDoorConnection = _loginScene._frontDoorConnection;
		if (frontDoorConnection == null)
		{
			SendEvent_FrontDoorConnectionError(currentEnvironment.GetFullUri(), "Tried to connect with a null FrontDoor.");
			return;
		}
		Debug.Log("Connecting to Front Door: " + currentEnvironment.GetFullUri());
		if (frontDoorConnection.ConnectionState != FDConnectionState.Disconnected)
		{
			SendEvent_FrontDoorConnectionError(currentEnvironment.GetFullUri(), "FrontDoor connection request with an unclosed connection.");
			frontDoorConnection.Close(TcpConnectionCloseType.Abnormal, "Duplicate Connection");
		}
		_loginScene.ConnectToFrontDoor(accountInfoResult);
	}

	private void SendEvent_FrontDoorConnectionError(string uri, string error)
	{
		BIEventType.FrontDoorConnectionError.SendWithDefaults(("Uri", uri), ("Error", error));
	}

	private void OnAccountLoginError(AccountError error)
	{
		bool flag = error.ErrorType == AccountError.ErrorTypes.Password || error.ErrorType == AccountError.ErrorTypes.ResetPassword || !_email_TMPInput.enabled;
		_loginScene.HandleAccountError(error, flag ? _password_inputField : _email_inputField, selectInputField: true);
	}

	private void genericOnHover()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, base.gameObject);
	}

	private void _checkFields()
	{
		if (string.IsNullOrWhiteSpace(_email_inputField.InputField.text))
		{
			EnableButton(enabled: false);
		}
		else if (string.IsNullOrWhiteSpace(_password_inputField.InputField.text))
		{
			EnableButton(enabled: false);
		}
		else if (_submitting)
		{
			EnableButton(enabled: false);
		}
		else
		{
			EnableButton(enabled: true);
		}
	}

	protected void Update()
	{
		_checkFields();
	}

	public override bool HandleKeyDown(KeyCode curr, Modifiers mods)
	{
		if (curr == KeyCode.Escape)
		{
			if (!_submitting)
			{
				OnButton_GoBack();
			}
			return true;
		}
		return false;
	}

	public override void OnBack(ActionContext context)
	{
		if (!_submitting)
		{
			OnButton_GoBack();
		}
	}
}
