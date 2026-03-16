using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;
using WAS;
using Wizards.Arena.Promises;

public class WASHTTPClient
{
	public class WASError
	{
		public int Code;

		public string Message;

		public WASError(int code, string message)
		{
			Code = code;
			Message = message;
		}
	}

	public const string AcceptLanguage = "Accept-Language";

	public static string BaseUri { get; private set; }

	public static string ClientID { get; private set; }

	public static string ClientSecret { get; private set; }

	public static EnvironmentType ClientEnvironment { get; private set; }

	public static void Init(string baseUri, string clientId, string clientSecret, EnvironmentType clientEnv)
	{
		BaseUri = baseUri;
		ClientID = clientId;
		ClientSecret = clientSecret;
		ClientEnvironment = clientEnv;
	}

	internal static WebPromise Login(string username, string password)
	{
		string url = BaseUri + "auth/oauth/token";
		KeyValuePair<string, string> basicAuthHeader = GetBasicAuthHeader(ClientID, ClientSecret);
		Dictionary<string, string> header = new Dictionary<string, string> { { basicAuthHeader.Key, basicAuthHeader.Value } };
		string content = GetContent(new List<KeyValuePair<string, string>>
		{
			new KeyValuePair<string, string>("grant_type", "password"),
			new KeyValuePair<string, string>("username", username),
			new KeyValuePair<string, string>("password", password)
		});
		return WebPromise.PostForm(url, header, content);
	}

	internal static WebPromise RegisterAsFullAccount(string input, string language)
	{
		return Register("accounts/register", input, language);
	}

	internal static WebPromise RegisterAsSocialAccount(string input, string language)
	{
		return Register("accounts/persona/register", input, language).WithLogging();
	}

	internal static WebPromise Register(string path, string input, string language)
	{
		string url = BaseUri + path;
		KeyValuePair<string, string> basicAuthHeader = GetBasicAuthHeader(ClientID, ClientSecret);
		Dictionary<string, string> header = new Dictionary<string, string>
		{
			{ basicAuthHeader.Key, basicAuthHeader.Value },
			{ "Accept-Language", language }
		};
		return WebPromise.PostJson(url, header, input);
	}

	internal static WebPromise LoginWithRefreshToken(string token)
	{
		string url = BaseUri + "auth/oauth/token";
		KeyValuePair<string, string> basicAuthHeader = GetBasicAuthHeader(ClientID, ClientSecret);
		Dictionary<string, string> header = new Dictionary<string, string> { { basicAuthHeader.Key, basicAuthHeader.Value } };
		string content = GetContent(new List<KeyValuePair<string, string>>
		{
			new KeyValuePair<string, string>("grant_type", "refresh_token"),
			new KeyValuePair<string, string>("refresh_token", token)
		});
		return WebPromise.PostForm(url, header, content);
	}

	internal static WebPromise LoginWithSocialToken(string type, string token)
	{
		string url = BaseUri + "auth/social/" + type + "/token";
		KeyValuePair<string, string> basicAuthHeader = GetBasicAuthHeader(ClientID, ClientSecret);
		Dictionary<string, string> header = new Dictionary<string, string> { { basicAuthHeader.Key, basicAuthHeader.Value } };
		string body = JsonConvert.SerializeObject(new
		{
			social_token = token
		});
		return WebPromise.PostJson(url, header, body).WithLogging();
	}

	internal static WebPromise GetLinkedAccounts(string accessToken)
	{
		return PlatformGet("accounts/socialidentities", string.Empty, accessToken);
	}

	internal static WebPromise GetAgeGate(string input)
	{
		string url = BaseUri + "accounts/requires-age-gate";
		KeyValuePair<string, string> basicAuthHeader = GetBasicAuthHeader(ClientID, ClientSecret);
		Dictionary<string, string> header = new Dictionary<string, string> { { basicAuthHeader.Key, basicAuthHeader.Value } };
		return WebPromise.PostJson(url, header, input);
	}

	internal static WebPromise UpdateParentalConsent(string input, string update_token)
	{
		string url = BaseUri + "accounts/parental-consent/update";
		Dictionary<string, string> header = new Dictionary<string, string> { 
		{
			"Authorization",
			"Bearer " + update_token
		} };
		return WebPromise.PostJson(url, header, input);
	}

	internal static WebPromise ForgotPassword(string input)
	{
		string url = BaseUri + "accounts/forgotpassword";
		KeyValuePair<string, string> basicAuthHeader = GetBasicAuthHeader(ClientID, ClientSecret);
		Dictionary<string, string> header = new Dictionary<string, string> { { basicAuthHeader.Key, basicAuthHeader.Value } };
		return WebPromise.PostJson(url, header, input);
	}

	internal static WebPromise ValidateUsername(string input)
	{
		string url = BaseUri + "accounts/moderate";
		KeyValuePair<string, string> basicAuthHeader = GetBasicAuthHeader(ClientID, ClientSecret);
		Dictionary<string, string> header = new Dictionary<string, string> { { basicAuthHeader.Key, basicAuthHeader.Value } };
		return WebPromise.PostJson(url, header, input);
	}

	internal static WebPromise PlatformPost(string path, string input, string language, string accessToken, Dictionary<string, string> header = null)
	{
		if (header == null)
		{
			header = new Dictionary<string, string>();
		}
		header.Add("Authorization", "Bearer " + accessToken);
		header.Add("Accept-Language", language);
		return WebPromise.PostJson(BaseUri + path, header, input);
	}

	internal static WebPromise GetPurchaseToken(string input, string language, string accessToken)
	{
		return PlatformPost("xsollaconnector/client/token", input, language, accessToken);
	}

	private static WebPromise PlatformGet(string path, string language, string accessToken)
	{
		Dictionary<string, string> header = new Dictionary<string, string>
		{
			{
				"Authorization",
				"Bearer " + accessToken
			},
			{ "Accept-Language", language }
		};
		return WebPromise.Get(BaseUri + path, header);
	}

	internal static WebPromise GetProfileToken(string language, string accessToken)
	{
		return PlatformGet("xsollaconnector/client/profile", language, accessToken);
	}

	internal static WebPromise GetProfile(string accessToken)
	{
		return PlatformGet("profile", string.Empty, accessToken);
	}

	internal static WebPromise GetAllEntitlementsByReceiptIdAndSource(string receiptId, string source, string language, string accessToken)
	{
		return PlatformGet("entitlements/source/" + source + "/receipt/" + receiptId, language, accessToken);
	}

	internal static WebPromise TryValidateReceipt(string store, string input, string language, string accessToken)
	{
		return PlatformPost("receiptverification/verify/" + store, input, language, accessToken);
	}

	internal static Promise<string> RedeemCode(string language, string code, string accessToken)
	{
		if (!ValidateUriSafeString(code))
		{
			return new SimplePromise<string>(new Error(404, "CODE NOT FOUND"));
		}
		return PlatformGet("redemption/code/" + code, language, accessToken);
	}

	internal static WebPromise GetStoreItems(string language, string currency, string accessToken)
	{
		string text = "xsollaconnector/client/skus";
		if (!string.IsNullOrEmpty(currency))
		{
			text = text + "?currency=" + currency;
		}
		return PlatformGet(text, language, accessToken);
	}

	public static WebPromise InitSteamPurchase(string accessToken, string language, string body)
	{
		return PlatformPost("purchase/steam/initiate", body, language, accessToken).WithLogging();
	}

	public static WebPromise LinkSocialAccount(string socialType, string socialToken, string accessToken)
	{
		return RequestLink(socialType, socialToken, accessToken);
	}

	public static WebPromise ResolveConflict(string socialType, string socialToken, string accessToken, ConflictingPersona personaToKeep)
	{
		return RequestLink(socialType, socialToken, accessToken, personaToKeep.personaType);
	}

	private static WebPromise RequestLink(string socialType, string socialToken, string accessToken, string forcePersonaType = "none")
	{
		string path = "auth/social/" + socialType + "/link";
		Dictionary<string, string> value = new Dictionary<string, string>
		{
			{ "social_token", socialToken },
			{ "force_on_persona", forcePersonaType }
		};
		return PlatformPost(path, JsonConvert.SerializeObject(value), string.Empty, accessToken).WithLogging();
	}

	public static WebPromise GetConflictingPersonas(string socialType, string socialToken, string accessToken)
	{
		string path = "auth/social/" + socialType + "/conflictInfo";
		Dictionary<string, string> value = new Dictionary<string, string> { { "social_token", socialToken } };
		return PlatformPost(path, JsonConvert.SerializeObject(value), string.Empty, accessToken).WithLogging();
	}

	public static WebPromise CancelLinking(string socialType, string accessToken)
	{
		return PlatformPost("auth/social/" + socialType + "/unlink", string.Empty, string.Empty, accessToken).WithLogging();
	}

	private static bool ValidateUriSafeString(string str)
	{
		if (str == null || str.Contains("..") || str.Contains("\\") || str.Contains("/") || str.Contains(" ") || str.Contains("\t") || str.Contains("\r") || str.Contains("\n"))
		{
			return false;
		}
		return true;
	}

	public static bool MyRemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
	{
		bool result = true;
		if (sslPolicyErrors != SslPolicyErrors.None)
		{
			for (int i = 0; i < chain.ChainStatus.Length; i++)
			{
				if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
				{
					chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
					chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
					chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
					chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
					if (!chain.Build((X509Certificate2)certificate))
					{
						result = false;
						break;
					}
				}
			}
		}
		return result;
	}

	private static string GetContent(IEnumerable<KeyValuePair<string, string>> nameValueCollection)
	{
		if (nameValueCollection == null)
		{
			throw new ArgumentNullException("nameValueCollection");
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, string> item in nameValueCollection)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append('&');
			}
			stringBuilder.Append(Encode(item.Key));
			stringBuilder.Append('=');
			stringBuilder.Append(Encode(item.Value));
		}
		return stringBuilder.ToString();
	}

	private static string Encode(string data)
	{
		if (string.IsNullOrEmpty(data))
		{
			return string.Empty;
		}
		return Uri.EscapeDataString(data).Replace("%20", "+");
	}

	private static KeyValuePair<string, string> GetBasicAuthHeader(string clientId, string clientSecret)
	{
		string text = Convert.ToBase64String(Encoding.UTF8.GetBytes(clientId + ":" + clientSecret));
		string value = "Basic " + text;
		return new KeyValuePair<string, string>("Authorization", value);
	}
}
