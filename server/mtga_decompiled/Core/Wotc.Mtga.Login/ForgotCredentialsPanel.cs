using System.Collections;
using Core.Code.Input;
using MTGA.KeyboardManager;
using TMPro;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Login;

public class ForgotCredentialsPanel : Panel
{
	[SerializeField]
	private UIWidget_InputField_Registration _email_inputField;

	[SerializeField]
	private TextMeshProUGUI _successLabel_text;

	protected override GameObject SelectOnLoad => _email_inputField.InputField.gameObject;

	public override void Initialize(LoginScene loginScene, IActionSystem actions, KeyboardManager keyboardManager, IBILogger biLogger)
	{
		_email_inputField.InputField.onSelect.AddListener(base.onInputField);
		_email_inputField.InputField.onSelect.AddListener(_email_select);
		_email_inputField.InputField.onEndEdit.AddListener(_email_endEdit);
		_email_inputField.Initialize(actions);
		base.Initialize(loginScene, actions, keyboardManager, biLogger);
	}

	private void OnDestroy()
	{
		_email_inputField.InputField.onSelect.RemoveListener(base.onInputField);
		_email_inputField.InputField.onSelect.RemoveListener(_email_select);
		_email_inputField.InputField.onEndEdit.RemoveListener(_email_endEdit);
	}

	private void _email_select(string arg0)
	{
		_email_inputField.ClearFeedbackText();
	}

	private void _email_endEdit(string arg0)
	{
		if (string.IsNullOrWhiteSpace(_email_inputField.InputField.text))
		{
			_email_inputField.SetFeedbackText(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Enter_Email"));
			EnableButton(enabled: false);
		}
		else
		{
			_email_inputField.ClearFeedbackText();
		}
	}

	public void Show(params string[] values)
	{
		_email_inputField.ClearFeedbackText();
		if (values.Length != 0)
		{
			_email_inputField.InputField.text = values[0];
		}
		else
		{
			_email_inputField.InputField.text = string.Empty;
		}
		_successLabel_text.text = string.Empty;
		_successLabel_text.gameObject.SetActive(value: false);
		base.Show();
	}

	public override void Show()
	{
		_email_inputField.ClearFeedbackText();
		_email_inputField.InputField.text = string.Empty;
		_successLabel_text.text = string.Empty;
		_successLabel_text.gameObject.SetActive(value: false);
		base.Show();
	}

	public void OnButton_SubmitPasswordRecovery()
	{
		_email_inputField.ClearFeedbackText();
		StartCoroutine(Coroutine_SubmitWaccPasswordRecovery(_email_inputField.InputField.text));
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
		EnableButton(enabled: false);
		LoginFlowAnalytics.SendEvent_Login("forgotPassword_attempted");
	}

	private IEnumerator Coroutine_SubmitWaccPasswordRecovery(string email)
	{
		_loginScene.SetLoadingState(enabled: true);
		yield return _loginScene._accountClient.SendPasswordRecoveryEmail(email).AsCoroutine();
		LoginFlowAnalytics.SendEvent_Login("forgotPassword_success");
		_successLabel_text.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Recovery_Email_Sent");
		_successLabel_text.gameObject.SetActive(value: true);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_whoosh_01, base.gameObject);
		_loginScene.SetLoadingState(enabled: false);
	}

	protected void Update()
	{
		if (string.IsNullOrWhiteSpace(_email_inputField.InputField.text))
		{
			EnableButton(enabled: false);
		}
		else
		{
			EnableButton(enabled: true);
		}
	}
}
