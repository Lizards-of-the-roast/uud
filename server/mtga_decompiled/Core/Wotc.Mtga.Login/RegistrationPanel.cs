using System;
using System.Collections;
using System.Collections.Generic;
using Core.Code.Input;
using Core.Code.Promises;
using MTGA.KeyboardManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WAS;
using Wizards.Arena.Promises;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wizards.Mtga.BI;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Login;

public class RegistrationPanel : Panel
{
	[SerializeField]
	private UIWidget_InputField_Registration displayName_inputField;

	[SerializeField]
	private Animator displayName_animator;

	[SerializeField]
	private GameObject displayName_validationSpinner;

	[SerializeField]
	private UIWidget_InputField_Registration email_inputField;

	[SerializeField]
	private UIWidget_InputField_Registration email2_inputField;

	[SerializeField]
	private UIWidget_InputField_Registration password_inputField;

	[SerializeField]
	private UIWidget_InputField_Registration password2_inputField;

	[SerializeField]
	private Toggle receiveOffers_Toggle;

	[SerializeField]
	private Toggle dataShare_Toggle;

	[SerializeField]
	private Toggle termsAndConditions_Toggle;

	[SerializeField]
	private Toggle codeOfConduct_Toggle;

	[SerializeField]
	private Toggle privacyPolicy_Toggle;

	[SerializeField]
	private GameObject submitButton;

	[SerializeField]
	private TextMeshProUGUI generalError;

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private float scrollSpeed = 0.3f;

	private bool _validDisplayName;

	private bool _submitting;

	private static readonly int _upHash = Animator.StringToHash("Up");

	protected override GameObject SelectOnLoad => displayName_inputField.InputField.gameObject;

	public static bool PolicyAcceptedThisSession { get; private set; }

	public override void Initialize(LoginScene loginScene, IActionSystem actions, KeyboardManager keyboardManager, IBILogger biLogger)
	{
		email_inputField.InputField.onSelect.AddListener(base.onInputField);
		email2_inputField.InputField.onSelect.AddListener(base.onInputField);
		displayName_inputField.InputField.onSelect.AddListener(base.onInputField);
		password_inputField.InputField.onSelect.AddListener(base.onInputField);
		password2_inputField.InputField.onSelect.AddListener(base.onInputField);
		email_inputField.InputField.onSelect.AddListener(_email_select);
		email2_inputField.InputField.onSelect.AddListener(_email2_select);
		displayName_inputField.InputField.onSelect.AddListener(_displayName_select);
		password_inputField.InputField.onSelect.AddListener(_password_select);
		password2_inputField.InputField.onSelect.AddListener(_password2_select);
		email_inputField.InputField.onEndEdit.AddListener(_email_endEdit);
		email2_inputField.InputField.onEndEdit.AddListener(_email2_endEdit);
		displayName_inputField.InputField.onEndEdit.AddListener(_displayName_endEdit);
		password_inputField.InputField.onEndEdit.AddListener(_password1_endEdit);
		password2_inputField.InputField.onEndEdit.AddListener(_password2_endEdit);
		password_inputField.InputField.onValueChanged.AddListener(_passwords_onValueChanged);
		password2_inputField.InputField.onValueChanged.AddListener(_passwords_onValueChanged);
		displayName_inputField.Initialize(actions);
		email_inputField.Initialize(actions);
		email2_inputField.Initialize(actions);
		password_inputField.Initialize(actions);
		password2_inputField.Initialize(actions);
		base.Initialize(loginScene, actions, keyboardManager, biLogger);
		generalError.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
		email_inputField.InputField.onSelect.RemoveListener(base.onInputField);
		email2_inputField.InputField.onSelect.RemoveListener(base.onInputField);
		displayName_inputField.InputField.onSelect.RemoveListener(base.onInputField);
		password_inputField.InputField.onSelect.RemoveListener(base.onInputField);
		password2_inputField.InputField.onSelect.RemoveListener(base.onInputField);
		email_inputField.InputField.onSelect.RemoveListener(_email_select);
		email2_inputField.InputField.onSelect.RemoveListener(_email2_select);
		displayName_inputField.InputField.onSelect.RemoveListener(_displayName_select);
		password_inputField.InputField.onSelect.RemoveListener(_password_select);
		password2_inputField.InputField.onSelect.RemoveListener(_password2_select);
		email_inputField.InputField.onEndEdit.RemoveListener(_email_endEdit);
		email2_inputField.InputField.onEndEdit.RemoveListener(_email2_endEdit);
		displayName_inputField.InputField.onEndEdit.RemoveListener(_displayName_endEdit);
		password_inputField.InputField.onEndEdit.RemoveListener(_password1_endEdit);
		password2_inputField.InputField.onEndEdit.RemoveListener(_password2_endEdit);
		password_inputField.InputField.onValueChanged.RemoveListener(_passwords_onValueChanged);
		password2_inputField.InputField.onValueChanged.RemoveListener(_passwords_onValueChanged);
	}

	private void _password2_select(string arg0)
	{
		password2_inputField.ClearFeedbackText();
	}

	private void _passwords_onValueChanged(string arg0)
	{
		password2_inputField.ClearFeedbackText();
		if (password_inputField.InputField.text != password2_inputField.InputField.text && string.IsNullOrWhiteSpace(password_inputField.FeedbackText.text) && !string.IsNullOrWhiteSpace(password2_inputField.InputField.text))
		{
			password2_inputField.SetFeedbackText(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Password_Confirmation_Required"), resetInput: false);
			EnableButton(enabled: false);
		}
	}

	private void _password2_endEdit(string arg0)
	{
		LoginFlowAnalytics.SendEvent_Registration("registration_password2_entered");
		password2_inputField.ClearFeedbackText();
		if (password_inputField.InputField.text != password2_inputField.InputField.text)
		{
			if (string.IsNullOrWhiteSpace(password_inputField.FeedbackText.text))
			{
				password2_inputField.SetFeedbackText(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Password_Confirmation_Required"), resetInput: false);
				EnableButton(enabled: false);
			}
		}
		else
		{
			LoginFlowAnalytics.SendEvent_Registration("registration_password_success");
		}
	}

	private void _password_select(string arg0)
	{
		password_inputField.ClearFeedbackText();
	}

	private bool VerifyPasswordLength(string passwordString)
	{
		if (passwordString.Length < 8 || passwordString.Length > 64)
		{
			return false;
		}
		return true;
	}

	private bool VerifyPasswordDoesNotContainDisplayName(string passwordString)
	{
		if (string.IsNullOrEmpty(displayName_inputField.InputField.text))
		{
			return true;
		}
		return !passwordString.ToLower().Contains(displayName_inputField.InputField.text.ToLower());
	}

	private bool VerifyPasswordDoesNotContainEmail(string passwordString)
	{
		if (string.IsNullOrEmpty(email_inputField.InputField.text))
		{
			return true;
		}
		string[] array = email_inputField.InputField.text.Split('@');
		bool flag = true;
		string[] array2 = array;
		foreach (string text in array2)
		{
			flag = flag && !passwordString.ToLower().Contains(text.ToLower());
		}
		return flag;
	}

	private bool VerifyPasswordDoesNotContainCharacterRepeatedThreeTimes(string passwordString)
	{
		if (passwordString.Length < 3)
		{
			return true;
		}
		for (int i = 2; i < passwordString.Length; i++)
		{
			if (passwordString[i] == passwordString[i - 1] && passwordString[i] == passwordString[i - 2])
			{
				return false;
			}
		}
		return true;
	}

	private void _password1_endEdit(string arg0)
	{
		LoginFlowAnalytics.SendEvent_Registration("registration_password1_entered");
		if (!VerifyPasswordLength(password_inputField.InputField.text))
		{
			password_inputField.SetFeedbackText(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Password_Length_Feedback"), resetInput: false);
			EnableButton(enabled: false);
			return;
		}
		if (!VerifyPasswordDoesNotContainEmail(password_inputField.InputField.text))
		{
			password_inputField.SetFeedbackText(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Password_Contains_Email_Feedback"), resetInput: false);
			EnableButton(enabled: false);
			return;
		}
		if (!VerifyPasswordDoesNotContainDisplayName(password_inputField.InputField.text))
		{
			password_inputField.SetFeedbackText(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Password_Contains_Display_Name_Feedback"), resetInput: false);
			EnableButton(enabled: false);
			return;
		}
		if (!VerifyPasswordDoesNotContainCharacterRepeatedThreeTimes(password_inputField.InputField.text))
		{
			password_inputField.SetFeedbackText(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Password_Character_Repeated_Three_Times_Feedback"), resetInput: false);
			EnableButton(enabled: false);
			return;
		}
		password_inputField.ClearFeedbackText();
		if (string.IsNullOrWhiteSpace(password2_inputField.InputField.text))
		{
			return;
		}
		password2_inputField.ClearFeedbackText();
		if (password_inputField.InputField.text != password2_inputField.InputField.text)
		{
			if (string.IsNullOrWhiteSpace(password_inputField.FeedbackText.text))
			{
				password2_inputField.SetFeedbackText(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Password_Confirmation_Required"), resetInput: false);
				EnableButton(enabled: false);
			}
		}
		else
		{
			LoginFlowAnalytics.SendEvent_Registration("registration_password_success");
		}
	}

	private void _email2_select(string arg0)
	{
		email2_inputField.ClearFeedbackText();
	}

	private void _email2_endEdit(string arg0)
	{
		email2_inputField.ClearFeedbackText();
		LoginFlowAnalytics.SendEvent_Registration("registration_email2_entered");
		if (email_inputField.InputField.text != email2_inputField.InputField.text && string.IsNullOrWhiteSpace(email_inputField.FeedbackText.text))
		{
			email2_inputField.SetFeedbackText(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Email_Confirmation_Required"), resetInput: false);
			EnableButton(enabled: false);
		}
	}

	private void _email_select(string arg0)
	{
		email_inputField.ClearFeedbackText();
	}

	private void _email_endEdit(string arg0)
	{
		LoginFlowAnalytics.SendEvent_Registration("registration_email1_entered");
		if (string.IsNullOrWhiteSpace(email_inputField.InputField.text))
		{
			email_inputField.SetFeedbackText(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Email_Required"), resetInput: false);
			EnableButton(enabled: false);
			return;
		}
		if (!VerifyPasswordDoesNotContainEmail(password_inputField.InputField.text))
		{
			password_inputField.SetFeedbackText(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Password_Contains_Email_Feedback"), resetInput: false);
			EnableButton(enabled: false);
			return;
		}
		email_inputField.ClearFeedbackText();
		if (!string.IsNullOrWhiteSpace(email2_inputField.InputField.text))
		{
			email2_inputField.ClearFeedbackText();
			if (email_inputField.InputField.text != email2_inputField.InputField.text && string.IsNullOrWhiteSpace(email_inputField.FeedbackText.text))
			{
				email2_inputField.SetFeedbackText(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Email_Confirmation_Required"), resetInput: false);
				EnableButton(enabled: false);
			}
		}
	}

	private void genericOnHover()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
	}

	private void _displayName_select(string arg0)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
		displayName_validationSpinner.SetActive(value: true);
		_validDisplayName = false;
		displayName_inputField.ClearFeedbackText();
	}

	private void _displayName_endEdit(string arg0)
	{
		LoginFlowAnalytics.SendEvent_Registration("registration_displayName_attempted");
		if (displayName_inputField.InputField.text.Length < 3 || displayName_inputField.InputField.text.Length > 23)
		{
			displayName_inputField.SetFeedbackText(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/DisplayName_Length_Feedback"), resetInput: false);
			displayName_validationSpinner.SetActive(value: false);
			EnableButton(enabled: false);
			return;
		}
		if (!VerifyPasswordDoesNotContainDisplayName(password_inputField.InputField.text))
		{
			password_inputField.SetFeedbackText(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Password_Contains_Display_Name_Feedback"), resetInput: false);
			EnableButton(enabled: false);
		}
		displayName_inputField.ClearFeedbackText();
		EnableButton(enabled: false);
		StartCoroutine(Coroutine_ValidateUsername(displayName_inputField.InputField.text));
	}

	private IEnumerator Coroutine_ValidateUsername(string username)
	{
		displayName_inputField.InputField.enabled = false;
		displayName_animator.SetTrigger(_upHash);
		displayName_animator.enabled = false;
		_validDisplayName = false;
		yield return _loginScene._accountClient.ValidateUsername(username).ThenOnMainThreadIfSuccess((Action<string>)delegate
		{
			_validDisplayName = true;
			displayName_validationSpinner.SetActive(value: false);
			displayName_inputField.InputField.enabled = true;
			displayName_animator.enabled = true;
			LoginFlowAnalytics.SendEvent_Registration("registration_displayName_success");
		}).ThenOnMainThreadIfError(delegate(Error e)
		{
			displayName_validationSpinner.SetActive(value: false);
			displayName_animator.enabled = true;
			displayName_inputField.InputField.enabled = true;
			AccountError accountError = WASUtils.ToAccountError(e);
			if (accountError.ErrorType == AccountError.ErrorTypes.DisplayName)
			{
				displayName_inputField.SetFeedbackText(accountError.LocalizedErrorMessage, resetInput: false);
			}
		})
			.AsCoroutine();
	}

	public override void Show()
	{
		LoginFlowAnalytics.SendEvent_Registration("registration_begin");
		displayName_inputField.InputField.text = string.Empty;
		email_inputField.InputField.text = string.Empty;
		email2_inputField.InputField.text = string.Empty;
		password_inputField.InputField.text = string.Empty;
		password2_inputField.InputField.text = string.Empty;
		displayName_inputField.ClearFeedbackText();
		email_inputField.ClearFeedbackText();
		email2_inputField.ClearFeedbackText();
		password_inputField.ClearFeedbackText();
		password2_inputField.ClearFeedbackText();
		displayName_validationSpinner.SetActive(value: false);
		displayName_inputField.InputField.enabled = true;
		displayName_animator.enabled = true;
		if (CountryCodes.DataShareCountries.ContainsKey(_loginScene.SelectedCountry))
		{
			dataShare_Toggle.transform.parent.gameObject.SetActive(value: true);
			dataShare_Toggle.isOn = false;
		}
		else
		{
			dataShare_Toggle.transform.parent.gameObject.SetActive(value: false);
			dataShare_Toggle.isOn = true;
		}
		base.Show();
	}

	public void OnButton_SubmitRegistration()
	{
		email_inputField.ClearFeedbackText();
		email2_inputField.ClearFeedbackText();
		password_inputField.ClearFeedbackText();
		password2_inputField.ClearFeedbackText();
		displayName_inputField.ClearFeedbackText();
		EnableButton(enabled: false);
		LoginFlowAnalytics.SendEvent_Registration("registration_attempted");
		DoRegistration();
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, base.gameObject);
	}

	private Promise<CreateUserResponse> DoRegistration()
	{
		_submitting = true;
		string email = email_inputField.InputField.text;
		string text = password_inputField.InputField.text;
		string text2 = displayName_inputField.InputField.text;
		bool isOn = receiveOffers_Toggle.isOn;
		bool isOn2 = dataShare_Toggle.isOn;
		Debug.Log("[Accounts - Registration] Attempting to register user.");
		return _loginScene._accountClient.RegisterAsFullAccount(email, text, text2, isOn, isOn2, _loginScene.Birthday, _loginScene.SelectedCountry).ThenOnMainThreadIfSuccess((Action<CreateUserResponse>)delegate
		{
			OnRegisterSuccess(email);
		}).ThenOnMainThreadIfError(delegate(Error e)
		{
			OnRegisterError(WASUtils.ToAccountError(e));
		})
			.Then(delegate
			{
				_submitting = false;
			});
	}

	private void OnRegisterError(AccountError error)
	{
		Debug.Log("[Accounts - Registration] Error registering.");
		UIWidget_InputField_Registration targetInputField = null;
		bool selectInputField = true;
		switch (error.ErrorType)
		{
		case AccountError.ErrorTypes.Email:
			targetInputField = email_inputField;
			email2_inputField.InputField.text = "";
			break;
		case AccountError.ErrorTypes.Password:
			targetInputField = password_inputField;
			password2_inputField.InputField.text = "";
			break;
		case AccountError.ErrorTypes.DisplayName:
			targetInputField = displayName_inputField;
			displayName_inputField.InputField.text = "";
			break;
		case AccountError.ErrorTypes.Token:
			submitButton.SetActive(value: false);
			_loginScene._accountClient.AllowAccountCreation = false;
			generalError.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/SocialIdentityInUse");
			generalError.gameObject.SetActive(value: true);
			StartCoroutine(ScrollUp());
			break;
		case AccountError.ErrorTypes.Age:
			AgeGateUtils.GateUserFromLoginDueToAge();
			submitButton.SetActive(value: false);
			LoginUtils.ShowAgeGateRegistrationFailurePopup();
			StartCoroutine(ScrollUp());
			break;
		default:
			selectInputField = false;
			targetInputField = displayName_inputField;
			break;
		}
		if (error.ErrorType != AccountError.ErrorTypes.Password)
		{
			LoginFlowAnalytics.SendEvent_Registration("registration_password_success");
		}
		if (error.ErrorType != AccountError.ErrorTypes.Token)
		{
			_loginScene.HandleAccountError(error, targetInputField, selectInputField);
		}
	}

	private void OnRegisterSuccess(string email)
	{
		AccountInformation accountInformation = _loginScene._accountClient.AccountInformation;
		Debug.Log("[Accounts - Registration] Finished registering user.");
		TrackBi(accountInformation);
		AssignExperiments(accountInformation);
		_loginScene._accountClient.RememberMe = true;
		MDNPlayerPrefs.Accounts_LastLogin_Email = email;
		_loginScene.ConnectToFrontDoor(accountInformation);
	}

	private void TrackBi(AccountInformation accountInfoResult)
	{
		BIEventTracker.TrackEvent(EBiEvent.PlayerRegistration, accountInfoResult.PersonaID);
		BIEventTracker.TrackEvent(EBiEvent.PlayerLogin, accountInfoResult.PersonaID);
		NPEState.BI_NPEProgressUpdate(new NPEState.NPEProgressContext(NPEState.NPEProgressMarker.Finished_Registration), _biLogger, null);
		LoginFlowAnalytics.SendEvent_Registration_Completed();
		PlayerExperienceSurvey payload = new PlayerExperienceSurvey
		{
			EventTime = DateTime.UtcNow,
			Experience = MDNPlayerPrefs.PLAYERPREFS_Experience
		};
		_biLogger.SendViaFrontdoor(ClientBusinessEventType.PlayerExperienceSurvey, payload);
	}

	private void AssignExperiments(AccountInformation accountInfoResult)
	{
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
		System.Random random = new System.Random();
		string key = "Experiment002pvpLockedUntilLevel3";
		double num = 0.2;
		if (dictionary[key] = random.NextDouble() <= num)
		{
			MDNPlayerPrefs.AddUserToExperimentalGroup_Experiment002(accountInfoResult.PersonaID);
		}
		string key2 = "Experiment003noStitcherBetweenG1AndG2";
		double num2 = 0.2;
		if (dictionary[key2] = random.NextDouble() <= num2)
		{
			MDNPlayerPrefs.AddUserToExperimentalGroup_Experiment003(accountInfoResult.PersonaID);
		}
		BI_RegistrationExperimentAssignment(dictionary);
	}

	private void BI_RegistrationExperimentAssignment(Dictionary<string, bool> assignments)
	{
		RegistrationExperimentAssignment registrationExperimentAssignment = new RegistrationExperimentAssignment
		{
			EventTime = DateTime.UtcNow,
			Assignments = assignments
		};
		_biLogger.Send(registrationExperimentAssignment.EventType, registrationExperimentAssignment);
	}

	private IEnumerator ScrollUp()
	{
		if (!(scrollRect == null))
		{
			float position = scrollRect.verticalNormalizedPosition;
			while (position < 1f)
			{
				position += scrollSpeed;
				scrollRect.verticalNormalizedPosition = position;
				yield return null;
			}
		}
	}

	private void _checkFields()
	{
		bool flag = false;
		if (!VerifyPasswordLength(password_inputField.InputField.text))
		{
			flag = true;
		}
		if (!VerifyPasswordDoesNotContainEmail(password_inputField.InputField.text))
		{
			flag = true;
		}
		if (!VerifyPasswordDoesNotContainDisplayName(password_inputField.InputField.text))
		{
			flag = true;
		}
		if (!VerifyPasswordDoesNotContainCharacterRepeatedThreeTimes(password_inputField.InputField.text))
		{
			flag = true;
		}
		if (displayName_inputField.InputField.text.Length < 3 || displayName_inputField.InputField.text.Length > 23 || !_validDisplayName)
		{
			EnableButton(enabled: false);
		}
		else if (string.IsNullOrWhiteSpace(email_inputField.InputField.text))
		{
			EnableButton(enabled: false);
		}
		else if (email_inputField.InputField.text != email2_inputField.InputField.text)
		{
			EnableButton(enabled: false);
		}
		else if (password_inputField.InputField.text.Length < 8)
		{
			EnableButton(enabled: false);
		}
		else if (flag || password_inputField.InputField.text != password2_inputField.InputField.text)
		{
			EnableButton(enabled: false);
		}
		else if (!termsAndConditions_Toggle.isOn || !codeOfConduct_Toggle.isOn || !privacyPolicy_Toggle.isOn)
		{
			PolicyAcceptedThisSession = false;
			EnableButton(enabled: false);
		}
		else if (_submitting)
		{
			EnableButton(enabled: false);
		}
		else
		{
			PolicyAcceptedThisSession = true;
			EnableButton(enabled: true);
		}
	}

	protected void Update()
	{
		_checkFields();
	}

	public void FillUser(string display, string baseName)
	{
		displayName_inputField.InputField.text = display;
		string text = baseName + "@test.wizards.com";
		string text2 = "Password1!";
		email_inputField.InputField.text = text;
		email2_inputField.InputField.text = text;
		password_inputField.InputField.text = text2;
		password2_inputField.InputField.text = text2;
	}

	public static void ResetPolicyAccepted()
	{
		PolicyAcceptedThisSession = false;
	}
}
