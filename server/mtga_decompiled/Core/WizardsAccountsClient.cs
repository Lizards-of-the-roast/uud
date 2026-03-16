using System;
using System.Collections.Generic;
using Core.Code.Promises;
using Newtonsoft.Json;
using UnityEngine;
using WAS;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Mtga.Platforms;
using Wizards.Mtga.Platforms.EpicOnlineService;
using Wizards.Platform.Sdk.Model;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Login;
using _3rdParty.Steam;

public class WizardsAccountsClient : IAccountClient
{
	[Serializable]
	public struct AttModel
	{
		public string user_id;

		public string type;

		public AttIdentifiers identifiers;
	}

	[Serializable]
	public struct AttIdentifiers
	{
		public string resolution;

		public string os;

		public string platform;
	}

	private const string WAS_RememberMe = "WAS-RememberMe";

	private static bool _socialLoginThisSession;

	private bool _isHandheld;

	public AccountInformation AccountInformation { get; private set; }

	public LoginState CurrentLoginState { get; private set; }

	public bool AllowAccountCreation
	{
		get
		{
			if (!_socialLoginThisSession)
			{
				return !AgeGateUtils.IsGatedDueToAge();
			}
			return false;
		}
		set
		{
			_socialLoginThisSession = !value;
		}
	}

	public bool RememberMe
	{
		get
		{
			string key = "WAS-RememberMe" + GetPrefsKeySuffix();
			if (PlayerPrefsExt.HasKey(key))
			{
				return bool.Parse(PlayerPrefsExt.GetString(key));
			}
			return false;
		}
		set
		{
			PlayerPrefsExt.SetString("WAS-RememberMe" + GetPrefsKeySuffix(), value.ToString());
			PlayerPrefsExt.Save();
		}
	}

	private string SavedRefreshToken
	{
		get
		{
			return MDNPlayerPrefs.GetRefreshToken(GetPrefsKeySuffix());
		}
		set
		{
			MDNPlayerPrefs.SetRefreshToken(value, GetPrefsKeySuffix());
		}
	}

	public string UpdateToken { get; set; }

	public bool IsPreProd
	{
		get
		{
			if (AccountInformation != null)
			{
				return AccountInformation.HasRole_FeatureToggle();
			}
			IClientVersionInfo versionInfo = Global.VersionInfo;
			if (!versionInfo.IsDevBuild())
			{
				return versionInfo.ContentVersion.Build >= 900;
			}
			return true;
		}
	}

	public event Action<LoginState> LoginStateChanged;

	private void SetLoginStateAndFireEvent(LoginState newState)
	{
		if (CurrentLoginState == newState)
		{
			return;
		}
		CurrentLoginState = newState;
		if (this.LoginStateChanged != null)
		{
			MainThreadDispatcher.Dispatch(delegate
			{
				this.LoginStateChanged(newState);
			});
		}
	}

	private static string GetPrefsKeySuffix()
	{
		if (WASHTTPClient.ClientEnvironment == EnvironmentType.PreProd)
		{
			return "-PreProd";
		}
		if (WASHTTPClient.ClientEnvironment == EnvironmentType.Load)
		{
			return "-Load";
		}
		return string.Empty;
	}

	public static IAccountClient Create()
	{
		return new WizardsAccountsClient();
	}

	public void SetCredentials(EnvironmentDescription env)
	{
		ILoginContext loginContext = PlatformContext.GetLoginContext();
		string clientID;
		string clientSecret;
		if (loginContext.IsLoggedIn && loginContext is EpicLoginContext)
		{
			clientID = env.epicWASClientId;
			clientSecret = env.epicWASClientSecret;
		}
		else if (Steam.Status == Steam.SteamStatus.Available)
		{
			clientID = env.steamClientId;
			clientSecret = env.steamClientSecret;
		}
		else
		{
			clientID = env.accountSystemId;
			clientSecret = env.accountSystemSecret;
		}
		loginContext.ClientID = clientID;
		loginContext.ClientSecret = clientSecret;
		WASHTTPClient.Init(env.accountSystemBaseUri, loginContext.ClientID, loginContext.ClientSecret, env.accountSystemEnvironment);
		AccountInformation = null;
	}

	public Promise<SteamReceipt> InitSteamPurchase(string steamSessionTicket, string language, string currency, string sku, bool sandbox)
	{
		string body = JsonUtility.ToJson(new InitiateSteamPurchaseRequest
		{
			ticket = steamSessionTicket,
			language = language,
			currency = currency,
			sku = sku,
			sandbox = sandbox
		});
		return WASHTTPClient.InitSteamPurchase(AccountInformation?.Credentials.Jwt, Languages.CurrentLanguage, body).Convert(JsonUtility.FromJson<SteamReceipt>);
	}

	public void Reset()
	{
		AccountInformation = null;
		SetLoginStateAndFireEvent(LoginState.NotLoggedIn);
	}

	public Promise<AccountInformation> LogIn_Credentials(string email, string password)
	{
		ILoginContext loginContext = PlatformContext.GetLoginContext();
		LoginFlowAnalytics.SendEvent_AttemptLogin(LoginAction.Manual, loginContext);
		AccountInformation = null;
		SetLoginStateAndFireEvent(LoginState.AttemptingToLogin);
		return WASHTTPClient.Login(email, password).Convert(JsonConvert.DeserializeObject<LoginResponse>).IfSuccess<LoginResponse>(delegate(Promise<LoginResponse> p)
		{
			LoginResponse login = p.Result;
			AccountInformation = new AccountInformation();
			AccountInformation.Email = email;
			AccountInformation.Password = password;
			return RetrieveProfile(login.access_token).IfSuccess(delegate(Promise<Profile> profile)
			{
				UpdateAccountInformation(login, LoginState.FullyRegisteredLogin, profile.Result);
			}).Convert((Profile _) => login);
		})
			.IfError((Action<Promise<LoginResponse>>)OnWASAccountError)
			.Convert((LoginResponse _) => AccountInformation);
	}

	public Promise<LoginResponse> RefreshAccessToken()
	{
		return WASHTTPClient.LoginWithRefreshToken(SavedRefreshToken).Convert(JsonUtility.FromJson<LoginResponse>).IfSuccess(delegate(Promise<LoginResponse> p)
		{
			LoginResponse login = p.Result;
			return RetrieveProfile(login.access_token).IfSuccess(delegate(Promise<Profile> promise)
			{
				UpdateAccountInformation(login, CurrentLoginState, promise.Result);
			}).Convert((Profile _) => p.Result);
		})
			.IfError((Action<Promise<LoginResponse>>)OnWASAccountError);
	}

	public Promise<AccountInformation> SocialLogin_Fast(bool manualLogin)
	{
		AccountInformation = null;
		ILoginContext loginCtx = PlatformContext.GetLoginContext();
		LoginFlowAnalytics.SendEvent_AttemptLogin(manualLogin ? LoginAction.Manual : LoginAction.Automatic, loginCtx);
		PromiseExtensions.Logger.Info("[Accounts - Startup] Attempting social login (" + loginCtx.SocialType + ").");
		return WASHTTPClient.LoginWithSocialToken(loginCtx.SocialType, loginCtx.SocialToken).Convert(JsonUtility.FromJson<LoginResponse>).IfSuccess(delegate(Promise<LoginResponse> p)
		{
			LoginResponse login = p.Result;
			AccountInformation = new AccountInformation();
			_socialLoginThisSession = true;
			PromiseExtensions.Logger.Info("[Accounts - Client] Successfully logged in to " + loginCtx.SocialType + "-linked acount: " + login.display_name);
			return RetrieveProfile(login.access_token).IfSuccess(delegate(Promise<Profile> promise)
			{
				UpdateAccountInformation(login, LoginState.FullyRegisteredLogin, promise.Result);
			}).Convert((Profile _) => login);
		})
			.Convert((LoginResponse _) => AccountInformation);
	}

	public Promise<AccountInformation> LogIn_Fast()
	{
		SetLoginStateAndFireEvent(LoginState.AttemptingToLogin);
		string savedRefreshToken = SavedRefreshToken;
		if (PlatformContext.GetLoginContext().IsLoggedIn && !_socialLoginThisSession)
		{
			return SocialLogin_Fast(manualLogin: false).IfError((Promise<AccountInformation> _) => RefreshToken_LogIn(savedRefreshToken), allowRecovery: true);
		}
		return RefreshToken_LogIn(savedRefreshToken);
	}

	public Promise<AccountInformation> RefreshToken_LogIn(string savedRefreshToken)
	{
		ILoginContext loginContext = PlatformContext.GetLoginContext();
		LoginFlowAnalytics.SendEvent_AttemptLogin(LoginAction.Automatic, loginContext);
		PromiseExtensions.Logger.Info("[Accounts - Startup] Player has rememberMe ID (refresh token) saved. Attempting fast login.");
		AccountInformation = null;
		return WASHTTPClient.LoginWithRefreshToken(savedRefreshToken).Convert(JsonUtility.FromJson<LoginResponse>).IfSuccess(delegate(Promise<LoginResponse> p)
		{
			AccountInformation = new AccountInformation();
			LoginResponse login = p.Result;
			return RetrieveProfile(p.Result.access_token).IfSuccess(delegate(Promise<Profile> profile)
			{
				UpdateAccountInformation(login, LoginState.FullyRegisteredLogin, profile.Result);
			}).Convert((Profile _) => login);
		})
			.IfError((Action<Promise<LoginResponse>>)OnWASAccountError)
			.Convert((LoginResponse _) => AccountInformation);
	}

	public Promise<CreateUserResponse> RegisterAsSocialAccount(string email, string displayName, bool receiveOffersOptIn, bool dataShareOptIn, string birthday, string country)
	{
		AccountInformation = null;
		ILoginContext loginContext = PlatformContext.GetLoginContext();
		_isHandheld = PlatformUtils.IsHandheld();
		return WASHTTPClient.RegisterAsSocialAccount(JsonUtility.ToJson(new RegisterUserRequest
		{
			displayName = displayName,
			email = email,
			password = string.Empty,
			country = country,
			dateOfBirth = birthday,
			acceptedTC = true,
			emailOptIn = receiveOffersOptIn,
			dataShareOptIn = dataShareOptIn,
			dryRun = false,
			socialType = loginContext.SocialType,
			socialToken = loginContext.SocialToken
		}), Languages.CurrentLanguage).Convert(JsonUtility.FromJson<CreateUserResponse>).IfSuccess((Promise<CreateUserResponse> p) => OnRegistrationSuccess(string.Empty, p.Result));
	}

	public Promise<CreateUserResponse> RegisterAsFullAccount(string email, string password, string displayName, bool receiveOffersOptIn, bool dataShareOptIn, string birthday, string country)
	{
		AccountInformation = null;
		ILoginContext loginContext = PlatformContext.GetLoginContext();
		_isHandheld = PlatformUtils.IsHandheld();
		return WASHTTPClient.RegisterAsFullAccount(JsonUtility.ToJson(new RegisterUserRequest
		{
			displayName = displayName,
			email = email,
			password = password,
			country = country,
			dateOfBirth = birthday,
			acceptedTC = true,
			emailOptIn = receiveOffersOptIn,
			dataShareOptIn = dataShareOptIn,
			dryRun = false,
			socialType = loginContext.SocialType,
			socialToken = loginContext.SocialToken
		}), Languages.CurrentLanguage).Convert(JsonUtility.FromJson<CreateUserResponse>).IfSuccess((Promise<CreateUserResponse> p) => OnRegistrationSuccess(password, p.Result))
			.IfError((Action<Promise<CreateUserResponse>>)OnWASAccountError);
	}

	private Promise<CreateUserResponse> OnRegistrationSuccess(string password, CreateUserResponse user)
	{
		AccountInformation = new AccountInformation();
		AccountInformation.Email = user.email;
		AccountInformation.Password = password;
		return RetrieveProfile(user.tokens.access_token).IfSuccess(delegate(Promise<Profile> p)
		{
			UpdateAccountInformation(user.tokens, LoginState.FullyRegisteredLogin, p.Result);
		}).Then((Promise<Profile> _) => LogInToGamesight(user)).Convert((string _) => user);
	}

	private Promise<string> LogInToGamesight(CreateUserResponse user)
	{
		if (_isHandheld)
		{
			return new SimplePromise<string>(string.Empty);
		}
		return GetGamesightLoginRequest(user.persona.personaID).Then(delegate(Promise<string> p)
		{
			WebPromise webPromise = (WebPromise)p;
			if (webPromise.ResponseCode != 201)
			{
				PromiseExtensions.Logger.Info($"Registration failure - code:{webPromise.ResponseCode} body: {webPromise.Result}");
			}
		});
	}

	public Promise<CreateUserResponse> CanRegister(string email, string password, string displayName, bool receiveOffersOptIn, bool dataShareOptIn, string birthday, string country)
	{
		AccountInformation = null;
		return WASHTTPClient.RegisterAsFullAccount(JsonUtility.ToJson(new RegisterUserRequest
		{
			displayName = displayName,
			email = email,
			password = password,
			country = country,
			dateOfBirth = birthday,
			acceptedTC = true,
			emailOptIn = receiveOffersOptIn,
			dataShareOptIn = dataShareOptIn,
			dryRun = true,
			socialType = PlatformContext.GetLoginContext().SocialType,
			socialToken = PlatformContext.GetLoginContext().SocialToken
		}), Languages.CurrentLanguage).Convert(JsonUtility.FromJson<CreateUserResponse>);
	}

	public void LogOut()
	{
		RememberMe = false;
		AccountInformation = null;
		SetLoginStateAndFireEvent(LoginState.NotLoggedIn);
	}

	public Promise<AgeCheckForAgeGatingResponse> GetAgeGate(string country, string dateOfBirth)
	{
		return WASHTTPClient.GetAgeGate(JsonUtility.ToJson(new AgeCheckForAgeGatingRequest
		{
			Country = country,
			DateOfBirth = dateOfBirth
		})).Convert(JsonUtility.FromJson<AgeCheckForAgeGatingResponse>).IfError((Action<Promise<AgeCheckForAgeGatingResponse>>)OnWASAccountError);
	}

	public Promise<string> UpdateParentalConsent(string country, string dateOfBirth)
	{
		return WASHTTPClient.UpdateParentalConsent(JsonUtility.ToJson(new AgeCheckForAgeGatingRequest
		{
			Country = country,
			DateOfBirth = dateOfBirth
		}), UpdateToken).IfSuccess(delegate
		{
			SetLoginStateAndFireEvent(LoginState.NotLoggedIn);
		}).IfError((Action<Promise<string>>)OnWASAccountError);
	}

	public Promise<string> SendPasswordRecoveryEmail(string email)
	{
		return WASHTTPClient.ForgotPassword(JsonUtility.ToJson(new RecoverPasswordRequest
		{
			LoginID = email
		}));
	}

	public Promise<string> ValidateUsername(string username)
	{
		return WASHTTPClient.ValidateUsername(JsonUtility.ToJson(new ValidateUsernameRequest
		{
			value = username,
			name = "Display Name"
		}));
	}

	public Promise<PurchaseTokenResponse> GetPurchaseToken(string currency, string sku, TransactionType transactionType)
	{
		return WASHTTPClient.GetPurchaseToken(JsonUtility.ToJson(new PurchaseTokenRequest
		{
			targetGameID = AccountInformation?.GameID,
			currency = currency,
			skus = new List<string> { sku },
			isTest = (transactionType == TransactionType.Sandbox)
		}), Languages.CurrentLanguage, AccountInformation?.Credentials.Jwt).Convert(JsonUtility.FromJson<PurchaseTokenResponse>);
	}

	public Promise<ProfileToken> GetProfileToken()
	{
		return WASHTTPClient.GetProfileToken(Languages.CurrentLanguage, AccountInformation?.Credentials.Jwt).Convert(JsonUtility.FromJson<ProfileToken>);
	}

	public Promise<ValidateReceiptResponse> TryValidateReceipt(string productId, AppStore platform, string packageName, string receipt)
	{
		var (text, text2) = GetPayload(productId, platform, packageName, receipt);
		if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text2))
		{
			return new SimplePromise<ValidateReceiptResponse>(new Error(-1, "Null storePath or input"));
		}
		return WASHTTPClient.TryValidateReceipt(text, text2, Languages.CurrentLanguage, AccountInformation?.Credentials.Jwt).Convert(JsonUtility.FromJson<ValidateReceiptResponse>);
	}

	private (string, string) GetPayload(string productId, AppStore platform, string packageName, string receipt)
	{
		string item = string.Empty;
		string item2 = string.Empty;
		switch (platform)
		{
		case AppStore.AppleAppStore:
			item = "appstore";
			item2 = JsonConvert.SerializeObject(new ValidateAppleAppStoreReceiptRequest
			{
				personaID = AccountInformation?.PersonaID,
				receipt_data = receipt
			});
			break;
		case AppStore.GooglePlay:
			item = "playstore";
			item2 = JsonConvert.SerializeObject(new ValidateGooglePlayReceiptRequest
			{
				personaID = AccountInformation?.PersonaID,
				packageName = packageName,
				productID = productId,
				token = receipt
			});
			break;
		case AppStore.SteamStore:
		{
			item = "steam/v2";
			SteamReceipt steamReceipt = JsonConvert.DeserializeObject<SteamReceipt>(receipt);
			item2 = JsonConvert.SerializeObject(new ValidateSteamReceiptRequest
			{
				personaID = AccountInformation?.PersonaID,
				orderID = steamReceipt.orderid,
				transactionID = steamReceipt.transid
			});
			break;
		}
		}
		return (item, item2);
	}

	public Promise<ItemsResponse> GetStoreItems(string currency)
	{
		return WASHTTPClient.GetStoreItems(Languages.CurrentLanguage, currency, AccountInformation?.Credentials.Jwt).Convert(JsonUtility.FromJson<ItemsResponse>);
	}

	public Promise<string> RedeemCode(string code)
	{
		return RetryPromise<string>.Create(() => WASHTTPClient.RedeemCode(Languages.CurrentLanguage, code, AccountInformation?.Credentials.Jwt), (Promise<string> p) => p.Error.Code == 500, (int _) => TimeSpan.FromMilliseconds(15.0), new RetryTermination.MaxRetries(2));
	}

	public Promise<AccountInformation> Debug_LogIn_Familiar(string email, string password)
	{
		Debug.LogFormat("[Accounts - Client] Logging in to familiar: {0}", email);
		return WASHTTPClient.Login(email, password).Convert((string p) => OnFamiliarLoginSuccess(JsonConvert.DeserializeObject<LoginResponse>(p)));
	}

	private static AccountInformation OnFamiliarLoginSuccess(LoginResponse response)
	{
		AccountInformation accountInformation = new AccountInformation();
		accountInformation.DisplayName = response.display_name;
		accountInformation.AccountID = response.account_id;
		accountInformation.PersonaID = response.persona_id;
		accountInformation.Credentials = new Credentials(response.access_token, response.refresh_token, response.expires_in, response.token_type);
		Debug.LogFormat("[Accounts - Client] Success logging in to familiar: ScreenName:{0} ID:{1} Ticket:{2}", response.display_name, response.persona_id, response.access_token);
		return accountInformation;
	}

	private void OnFamiliarLoginError(Error error, Action<AccountError> errorCallback)
	{
		_ = (int)error;
		errorCallback?.Invoke(new AccountError
		{
			ErrorCode = error.Code,
			ErrorMessage = error.Message,
			ErrorType = AccountError.ErrorTypes.Other
		});
	}

	public Promise<LoginResponse> LinkSocialAccount()
	{
		ILoginContext loginCtx = PlatformContext.GetLoginContext();
		return WASHTTPClient.LinkSocialAccount(loginCtx.SocialType, loginCtx.SocialToken, AccountInformation.AccessToken).Convert(JsonConvert.DeserializeObject<LoginResponse>).IfSuccess<LoginResponse>(delegate(Promise<LoginResponse> p)
		{
			LoginFlowAnalytics.SendEvent_SocialAccountLinked(loginCtx);
			UpdateAccountInformation(p.Result, LoginState.FullyRegisteredLogin);
		});
	}

	public Promise<ConflictingPersonas> GetLinkConflictInfo()
	{
		ILoginContext loginContext = PlatformContext.GetLoginContext();
		return WASHTTPClient.GetConflictingPersonas(loginContext.SocialType, loginContext.SocialToken, AccountInformation.AccessToken).Convert(JsonConvert.DeserializeObject<ConflictingPersonas>);
	}

	public Promise<LoginResponse> ResolveLinkConflict(ConflictingPersona personaToKeep, ConflictingPersona personaToDiscard)
	{
		ILoginContext loginCtx = PlatformContext.GetLoginContext();
		return WASHTTPClient.ResolveConflict(loginCtx.SocialType, loginCtx.SocialToken, AccountInformation.AccessToken, personaToKeep).Convert(JsonConvert.DeserializeObject<LoginResponse>).IfSuccess<LoginResponse>(delegate(Promise<LoginResponse> p)
		{
			LoginFlowAnalytics.SendEvent_AccountConflictResolved(loginCtx, personaToKeep, personaToDiscard);
			UpdateAccountInformation(p.Result, LoginState.FullyRegisteredLogin);
		});
	}

	public Promise<LoginResponse> CancelAccountLinking()
	{
		ILoginContext loginCtx = PlatformContext.GetLoginContext();
		return WASHTTPClient.CancelLinking(loginCtx.SocialType, AccountInformation.AccessToken).Convert(JsonConvert.DeserializeObject<LoginResponse>).IfSuccess<LoginResponse>(delegate(Promise<LoginResponse> p)
		{
			LoginFlowAnalytics.SendEvent_SocialAccountLinkedCancelled(loginCtx);
			UpdateAccountInformation(p.Result, LoginState.FullyRegisteredLogin);
		});
	}

	public Promise<SocialIdentities> GetLinkedAccounts()
	{
		return WASHTTPClient.GetLinkedAccounts(AccountInformation.AccessToken).Convert(JsonConvert.DeserializeObject<SocialIdentities>).Then<SocialIdentities, SocialIdentities>(delegate(Promise<SocialIdentities> p)
		{
			SocialIdentities socialIdentities = p.Result ?? new SocialIdentities
			{
				socialIdentities = new List<SocialIdentity>()
			};
			AccountInformation.LinkedAccounts = socialIdentities.socialIdentities;
			return (!p.Error.IsError || p.Error.Code == 403) ? new SimplePromise<SocialIdentities>(socialIdentities) : new SimplePromise<SocialIdentities>(p.Error);
		});
	}

	private void UpdateAccountInformation(CreateUserResponse response, LoginState loginState)
	{
		if (AccountInformation != null)
		{
			AccountInformation.Email = response.email;
			AccountInformation.ExternalID = response.externalID;
			UpdateAccountInformation(response.tokens, loginState);
		}
	}

	private void UpdateAccountInformation(LoginResponse response, LoginState loginState, Profile profile)
	{
		if (AccountInformation != null)
		{
			AccountInformation.Email = profile.Email;
			AccountInformation.ExternalID = profile.ExternalID;
			AccountInformation.CountryCode = profile.CountryCode;
			UpdateAccountInformation(response, loginState);
		}
	}

	private void UpdateAccountInformation(LoginResponse response, LoginState loginState)
	{
		if (AccountInformation != null)
		{
			AccountInformation.DisplayName = response.display_name;
			AccountInformation.AccountID = response.account_id;
			AccountInformation.GameID = response.game_id;
			AccountInformation.PersonaID = response.persona_id;
			SetAccountCredentialsFromLoginResponse(response);
			TrackLoginDetails(response, loginState);
		}
	}

	private void SetAccountCredentialsFromLoginResponse(LoginResponse response)
	{
		string access_token = response.access_token;
		AccountInformation.Roles = JwtHandler.GetRolesFromAccessToken(access_token);
		AccountInformation.AccessToken = access_token;
		AccountInformation.Credentials = new Credentials(access_token, response.refresh_token, response.expires_in, response.token_type);
	}

	private void TrackLoginDetails(LoginResponse response, LoginState loginState)
	{
		MainThreadDispatcher.Dispatch(delegate
		{
			SavedRefreshToken = response.refresh_token;
			MDNPlayerPrefs.Accounts_DateLastLoggedIn = DateTime.Now;
			SetLoginStateAndFireEvent(loginState);
		});
	}

	private Promise<Profile> RetrieveProfile(string accessToken)
	{
		return WASHTTPClient.GetProfile(accessToken).Convert(JsonConvert.DeserializeObject<Profile>).IfError<Profile>(delegate(Promise<Profile> p)
		{
			PromiseExtensions.Logger.Error("RetrieveProfile: " + p.Error);
		});
	}

	internal WebPromise GetGamesightLoginRequest(string personaId)
	{
		Dictionary<string, string> header = new Dictionary<string, string>
		{
			{ "Authorization", "fad5fe7f1101f61ba2d3977050ae8807" },
			{ "Accept", "application/json" }
		};
		AttModel attModel = new AttModel
		{
			user_id = personaId,
			identifiers = new AttIdentifiers
			{
				os = Environment.OSVersion.ToString(),
				resolution = Screen.width + "x" + Screen.height,
				platform = PlatformUtils.GetClientPlatform()
			},
			type = "game_launch"
		};
		return WebPromise.PostJson("https://mtgarena.api.bi-installs.wizards.com", header, JsonUtility.ToJson(attModel));
	}

	private void OnWASAccountError<T>(Promise<T> promise)
	{
		AccountError accountError = WASUtils.ToAccountError(promise.Error);
		if (AccountInformation != null)
		{
			if (accountError.ErrorMessage == "INVALID REFRESH TOKEN")
			{
				AccountInformation.CredentialsState = TokenState.Invalid;
			}
			if (accountError.ErrorMessage == "REFRESH TOKEN EXPIRED")
			{
				AccountInformation.CredentialsState = TokenState.Expired;
			}
		}
		if (accountError.ErrorType == AccountError.ErrorTypes.UpdateRequired)
		{
			UpdateToken = accountError.UpdateToken;
		}
		SetLoginStateAndFireEvent(accountError.ErrorType switch
		{
			AccountError.ErrorTypes.ResetPassword => LoginState.ResetPassword, 
			AccountError.ErrorTypes.UpdateRequired => LoginState.UpdateAgeGateInfo, 
			_ => LoginState.NotLoggedIn, 
		});
	}
}
