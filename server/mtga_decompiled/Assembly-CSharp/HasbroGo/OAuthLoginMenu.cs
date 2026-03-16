using System.Net;
using System.Threading.Tasks;
using System.Web;
using HasbroGo.Accounts;
using HasbroGo.Accounts.OAuth;
using HasbroGo.Errors;
using HasbroGo.Helpers;
using HasbroGo.Logging;
using HasbroGoUnity.AccountsBrowser;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HasbroGo;

public class OAuthLoginMenu : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI textBox;

	[SerializeField]
	private string mainMenuSceneName;

	private bool _loginInProgress;

	private readonly IOpenBrowserAction _openBrowserAction = new UnityOpenBrowserAction();

	private IOAuthProvider _authProvider;

	private readonly string _logCategory = "WasBrowser";

	private HasbroGoSDK sdk => HasbroGoSDKManager.Instance.HasbroGoSdk;

	private void Start()
	{
		if (!ShouldCreateWebServer())
		{
			Application.deepLinkActivated += onDeepLinkActivated;
		}
	}

	private void OnDestroy()
	{
		if (!ShouldCreateWebServer())
		{
			Application.deepLinkActivated -= onDeepLinkActivated;
		}
	}

	public void PerformRegistrationOAuth()
	{
		OAuthProviderType providerType = (HasbroGoSDKManager.Instance.UseProductionEnvironment ? OAuthProviderType.WizardsRegistration : OAuthProviderType.WizardsRegistration_DEVELOPMENT);
		OpenWASBrowser(providerType);
	}

	public void PerformAuthorizationOAuth()
	{
		OAuthProviderType providerType = ((!HasbroGoSDKManager.Instance.UseProductionEnvironment) ? OAuthProviderType.WizardsLogin_DEVELOPMENT : OAuthProviderType.WizardsLogin);
		OpenWASBrowser(providerType);
	}

	private void OpenWASBrowser(OAuthProviderType providerType)
	{
		if (_loginInProgress)
		{
			return;
		}
		OAuthProviderFactory oAuthProviderFactory = new OAuthProviderFactory(sdk.Configuration, sdk.AccountsService);
		string redirectUri = GetRedirectUri();
		Result<IOAuthProvider, Error> result = oAuthProviderFactory.Create(providerType, redirectUri);
		if (!result.IsOk)
		{
			LogHelper.Logger.Log(_logCategory, LogLevel.Error, result.Error.Message);
			return;
		}
		_authProvider = result.Value;
		_loginInProgress = true;
		if (ShouldCreateWebServer())
		{
			new OAuthRequestServer(_authProvider, _openBrowserAction).PerformOAuthLogin().ContinueWith(HandleLoginResult, TaskScheduler.FromCurrentSynchronizationContext());
			return;
		}
		string url = _authProvider.RequestUri.ToString();
		_openBrowserAction.LaunchBrowser(url);
	}

	private void onDeepLinkActivated(string url)
	{
		if (_authProvider != null && url.Contains(_authProvider.RedirectUri))
		{
			HttpUtility.ParseQueryString(url);
			_authProvider.PerformLogin(url).ContinueWith(HandleLoginResult, TaskScheduler.FromCurrentSynchronizationContext());
		}
	}

	private void HandleLoginResult(Task<Result<LoginResult, Error>> loginResult)
	{
		_loginInProgress = false;
		if (loginResult.IsFaulted)
		{
			textBox.text = $"Unhandled exception during login: {loginResult.Exception} ";
			return;
		}
		if (loginResult.IsCanceled)
		{
			textBox.text = "Login operation canceled or timed out.";
			return;
		}
		if (!loginResult.Result.IsOk)
		{
			textBox.text = "login failed " + loginResult.Result.Error.Message;
			return;
		}
		textBox.text = loginResult.Result.Value.UserName ?? "Login successful";
		LoadMainMenuScene();
	}

	private string GetRedirectUri()
	{
		if (IsRunningOnMobile())
		{
			return "unitydl://oauth/wizards";
		}
		string arg = (HasbroGoSDKManager.Instance.UseProductionEnvironment ? "patronus" : "patronus_dev");
		return $"http://{IPAddress.Loopback}:{47412}/{arg}/";
	}

	private bool ShouldCreateWebServer()
	{
		if (IsRunningOnMobile())
		{
			return false;
		}
		return true;
	}

	private bool IsRunningOnMobile()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return Application.platform == RuntimePlatform.IPhonePlayer;
		}
		return true;
	}

	private void LoadMainMenuScene()
	{
		SceneManager.LoadScene(mainMenuSceneName);
	}
}
