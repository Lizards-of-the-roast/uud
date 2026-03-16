using System.Threading.Tasks;
using HasbroGo.Accounts;
using HasbroGo.Accounts.Models.Requests;
using HasbroGo.Errors;
using HasbroGo.Helpers;
using HasbroGo.Logging;
using UnityEngine;

namespace HasbroGo;

public class AccountManager : MonoBehaviour
{
	private bool loginInProgress;

	private readonly string logCategory = "AccountManager";

	public static AccountManager Instance { get; private set; }

	private HasbroGoSDK sdk => HasbroGoSDKManager.Instance.HasbroGoSdk;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Object.Destroy(this);
			return;
		}
		Instance = this;
		Object.DontDestroyOnLoad(this);
	}

	public async Task<Result<LoginResult, Error>> LoginWithWAS(string email, string password)
	{
		if (loginInProgress)
		{
			return new GenericError("Login currently in progress.");
		}
		if (sdk.AccountsService.IsLoggedIn())
		{
			LogHelper.Logger.Log(logCategory, LogLevel.Log, "Logging out current account: " + sdk.AuthManager.GetAccountId());
			Logout();
		}
		loginInProgress = true;
		LogHelper.Logger.Log(logCategory, LogLevel.Log, "Logging in account: " + email);
		LoginWASRequest loginWASRequest = new LoginWASRequest
		{
			Email = email,
			Password = password
		};
		Result<LoginResult, Error> result = await sdk.AccountsService.LoginWithWAS(loginWASRequest);
		loginInProgress = false;
		if (result.IsOk)
		{
			return result;
		}
		return new GenericError(result.Error.Message);
	}

	public async Task<Result<LoginResult, Error>> TryAutomaticLogin()
	{
		return await sdk.AccountsService.TryLoginWithRefreshToken();
	}

	public void Logout()
	{
		sdk.AccountsService.LogoutUser();
	}
}
