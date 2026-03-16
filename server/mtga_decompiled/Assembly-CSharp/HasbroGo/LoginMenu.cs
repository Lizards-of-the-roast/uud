using System;
using System.Threading;
using System.Threading.Tasks;
using HasbroGo.Accounts;
using HasbroGo.Accounts.Profile;
using HasbroGo.Errors;
using HasbroGo.Helpers;
using HasbroGo.Logging;
using HasbroGoUnity.Wands;
using HasbroGoUnity.Wands.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HasbroGo;

public class LoginMenu : MonoBehaviour
{
	private WizWandsPageView _pageViewObject;

	[SerializeField]
	private bool tryAutoLogin = true;

	[SerializeField]
	private TMP_InputField emailText;

	[SerializeField]
	private TMP_InputField passwordText;

	[SerializeField]
	private TextMeshProUGUI results;

	[SerializeField]
	private Button mainMenuButton;

	private readonly string logCategory = "LoginMenu";

	private HasbroGoSDK sdk => HasbroGoSDKManager.Instance.HasbroGoSdk;

	public async void OnEnable()
	{
		mainMenuButton.gameObject.SetActive(sdk.AccountsService.IsLoggedIn());
		if (sdk?.AccountsService == null)
		{
			throw new Exception("The HasbroGoUnity SDK intialization must add the AccountService to use the LoginMenu.");
		}
		_pageViewObject = new WizWandsPageView(base.gameObject.name);
		sdk.AccountsService.AccessRevoked += Accounts_AccessRevoked;
		sdk.AccountsService.Logout += OnUserLoggedOut;
		if (sdk.AccountsService.IsLoggedIn())
		{
			Result<ProfileResponse, Error> result = await sdk.AccountsService.GetProfile();
			if (result.IsOk)
			{
				results.text = result.Value.DisplayName;
			}
		}
		else if (tryAutoLogin)
		{
			await TryAutomaticLogin();
		}
	}

	public void OnDisable()
	{
		sdk.AccountsService.AccessRevoked -= Accounts_AccessRevoked;
		sdk.AccountsService.Logout -= OnUserLoggedOut;
		_pageViewObject.RecordPageView();
		_pageViewObject = null;
	}

	public async void LoginButtonClicked()
	{
		await Login();
	}

	public async void LogoutButtonClicked()
	{
		await Logout();
	}

	private async Task Login()
	{
		if (string.IsNullOrEmpty(emailText.text) || string.IsNullOrEmpty(passwordText.text))
		{
			results.text = "Invalid email address or password. Please check your spelling and try again.";
			return;
		}
		results.text = "Login started for account: " + emailText.text + ".";
		Result<LoginResult, Error> result = await AccountManager.Instance.LoginWithWAS(emailText.text, passwordText.text);
		if (result.IsOk)
		{
			results.text = result.Value.UserName;
			await SocialManager.Instance.RefreshUserPresence(isLoginRefresh: true);
			LoadMainMenuScene();
		}
		else
		{
			results.text = "Login Failure: " + result.Error.Message;
		}
	}

	private async Task Logout()
	{
		if ((await sdk.SocialService.UpdatePresence()).IsOk)
		{
			LogHelper.Logger.Log(logCategory, LogLevel.Log, "User Presence set to offline");
		}
		else
		{
			LogHelper.Logger.Log(logCategory, LogLevel.Error, "Failed to set user Presence to offline.");
		}
		AccountManager.Instance.Logout();
	}

	private Task OnUserLoggedOut(object obj, EventArgs args, CancellationToken token)
	{
		results.text = "No user logged in.";
		mainMenuButton.gameObject.SetActive(value: false);
		return Task.CompletedTask;
	}

	public void GoogleLogin()
	{
		results.text = "Not implemented yet.";
	}

	public void AppleLogin()
	{
		results.text = "Not implemented yet.";
	}

	public void SendEventTest()
	{
		WizWandsEvents.RecordPlaySessionStart(base.gameObject);
		WizWandsEvents.RecordPlayStart(base.gameObject, "", "");
		WizWandsEvents.RecordAppleLogin(base.gameObject, "Nintendo", "Quach", "");
		WizWandsEvents.RecordLogout(base.gameObject, "");
		WizWandsEvents.RecordClientSessionEnd();
	}

	private Task Accounts_AccessRevoked(object sender, EventArgs e, CancellationToken token)
	{
		results.text = "Account access revoked.Please log-in again.";
		return Task.CompletedTask;
	}

	private async Task TryAutomaticLogin()
	{
		Result<LoginResult, Error> result = await AccountManager.Instance.TryAutomaticLogin();
		if (result.IsOk)
		{
			results.text = result.Value.UserName;
			await SocialManager.Instance.RefreshUserPresence(isLoginRefresh: true);
			LoadMainMenuScene();
		}
	}

	private void LoadMainMenuScene()
	{
		mainMenuButton.onClick.Invoke();
	}
}
