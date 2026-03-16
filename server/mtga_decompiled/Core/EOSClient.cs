using System;
using System.Linq;
using System.Text;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Logging;
using Epic.OnlineServices.Platform;
using UnityEngine;
using UnityEngine.CrashReportHandler;

public class EOSClient : MonoBehaviour
{
	private const string ProductId = "dd3c38c69b0c489694376dbd027a18a1";

	private const string SandboxId = "5fdcc0dc67294af3be740f362bd976fd";

	private const string ClientId = "4766fd45385e4c90bcbeb38d4f1cb87b";

	private const string ClientSecret = "faidg4m1XhKx2bbXDWDovi6NKq0D93sgABiy5goQge6b";

	private const string DeploymentId = "637bf354361c4a22b44166eb698d6c10";

	private const string ProductName = "MTGA";

	private const string ProductVersion = "1";

	private static EOSClient _instance;

	private static bool _initializedStatic;

	private bool _initializedInstance;

	private PlatformInterface _epicPlatform;

	public static EOSClientState State { get; private set; }

	public static EpicAccountId AccountId { get; private set; }

	public static Epic.OnlineServices.Auth.Token AuthToken { get; private set; }

	public static string AccountIdToken { get; private set; }

	public static void Initialize(Credentials loginCredentials)
	{
		if (_initializedStatic)
		{
			LogError("already initialized, only do this once per execution");
			return;
		}
		_initializedStatic = true;
		GameObject obj = new GameObject("EOSClient");
		UnityEngine.Object.DontDestroyOnLoad(obj);
		_instance = obj.AddComponent<EOSClient>();
		_instance.OnInitialize(loginCredentials);
	}

	public static bool TryGetLauncherCredentials(out Credentials loginCredentials)
	{
		loginCredentials = null;
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		if (!commandLineArgs.Contains("-EpicPortal"))
		{
			Log("no Epic Games launcher args found, skipping EOS startup");
			CrashReportHandler.SetUserMetadata("epicClient", "false");
			return false;
		}
		Log("detecting launch from Epic Games launcher");
		CrashReportHandler.SetUserMetadata("epicClient", "true");
		Debug.Log("Epic Args: " + commandLineArgs);
		string epicCommandLineArgs = GetEpicCommandLineArgs("-epicuserid");
		if (string.IsNullOrEmpty(epicCommandLineArgs))
		{
			LogError("unfound launch arg: -epicuserid");
			return false;
		}
		string epicCommandLineArgs2 = GetEpicCommandLineArgs("-AUTH_PASSWORD");
		if (string.IsNullOrEmpty(epicCommandLineArgs2))
		{
			LogError("unfound launch arg: -AUTH_PASSWORD");
			return false;
		}
		Log("found launch args epicUserId(" + epicCommandLineArgs + ") authPassword(" + epicCommandLineArgs2 + ")");
		loginCredentials = new Credentials
		{
			Type = LoginCredentialType.ExchangeCode,
			Id = epicCommandLineArgs,
			Token = epicCommandLineArgs2
		};
		return true;
	}

	public static string GetEpicCommandLineArgs(string input)
	{
		string text = Environment.GetCommandLineArgs().FirstOrDefault((string x) => x.Contains(input));
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		string[] array = text.Split("=".ToCharArray(), 2);
		if (array.Length != 2)
		{
			LogError("error parsing " + input + " from " + text);
			return null;
		}
		return array[1];
	}

	public static Credentials CreateDeveloperCredentials(string host, string userName)
	{
		return new Credentials
		{
			Type = LoginCredentialType.Developer,
			Id = host,
			Token = userName
		};
	}

	private void OnInitialize(Credentials loginCredentials)
	{
		Log("initializing new EOSClient");
		_initializedInstance = true;
		Result result = PlatformInterface.Initialize(new InitializeOptions
		{
			ProductName = "MTGA",
			ProductVersion = "1"
		});
		Log($"PlatformInterface.Initialize returned with result {result}");
		LoggingInterface.SetLogLevel(LogCategory.AllCategories, LogLevel.Verbose);
		LoggingInterface.SetCallback(SDKLog);
		ClientCredentials clientCredentials = new ClientCredentials
		{
			ClientId = "4766fd45385e4c90bcbeb38d4f1cb87b",
			ClientSecret = "faidg4m1XhKx2bbXDWDovi6NKq0D93sgABiy5goQge6b"
		};
		Options options = new Options
		{
			ProductId = "dd3c38c69b0c489694376dbd027a18a1",
			SandboxId = "5fdcc0dc67294af3be740f362bd976fd",
			DeploymentId = "637bf354361c4a22b44166eb698d6c10",
			ClientCredentials = clientCredentials
		};
		_epicPlatform = PlatformInterface.Create(options);
		LoginOptions options2 = new LoginOptions
		{
			Credentials = loginCredentials
		};
		State = EOSClientState.LoggingIn;
		Log($"attempting to login with type({loginCredentials.Type}) id({loginCredentials.Id}) token({loginCredentials.Token})");
		_epicPlatform.GetAuthInterface().Login(options2, null, delegate(LoginCallbackInfo info)
		{
			if (info.ResultCode == Result.Success)
			{
				AccountId = info.LocalUserId;
				StringBuilder stringBuilder = new StringBuilder(64, 64);
				int inOutBufferLength = 64;
				if (info.LocalUserId.ToString(stringBuilder, ref inOutBufferLength) == Result.Success)
				{
					AccountIdToken = stringBuilder.ToString();
				}
				else
				{
					LogError("error retrieving AccountIdToken from AccountId");
				}
				if (_epicPlatform.GetAuthInterface().CopyUserAuthToken(new CopyUserAuthTokenOptions(), AccountId, out var outUserAuthToken) == Result.Success)
				{
					AuthToken = outUserAuthToken;
				}
				else
				{
					LogError("error retrieving AuthToken from auth interface");
				}
				State = EOSClientState.LoggedIn;
			}
			else
			{
				State = EOSClientState.LoggedOut;
			}
			Log($"AuthInterface.Login returned token '{AccountIdToken}' with result {info.ResultCode}");
		});
	}

	private void OnDestroy()
	{
		if (_initializedInstance)
		{
			_epicPlatform?.Release();
			PlatformInterface.Shutdown();
		}
	}

	private void Update()
	{
		_epicPlatform?.Tick();
	}

	private static void Log(string message)
	{
		Debug.Log("[EOSClient] " + message);
	}

	private static void LogError(string message)
	{
		Debug.LogError("[EOSClient] " + message);
	}

	private static void SDKLog(LogMessage message)
	{
		Debug.Log("[EOS_SDK] " + message.Category + " -- " + message.Message);
	}
}
