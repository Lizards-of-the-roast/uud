using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Core.Shared.Code.Connection;
using Core.Shared.Code.DebugTools;
using Core.Shared.Code.Providers;
using Core.Shared.Code.ServiceFactories;
using Cysharp.Threading.Tasks;
using GreClient.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wizards.Arena.Client.Logging;
using Wizards.Arena.TcpConnection;
using Wizards.Mtga;
using Wizards.Mtga.Assets;
using Wotc.Mtga;
using Wotc.Mtga.AutoPlay;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Replays;
using Wotc.Mtgo.Gre.External.Messaging;

public class BotBattleScene : MonoBehaviour
{
	private class ErrorLog
	{
		public string Text;

		public string StackTrace;
	}

	public const string SCENE_NAME = "BotBattleScene";

	private PAPA _papa;

	private MatchManager _matchManager;

	private IAccountClient _accountClient;

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	private CardMaterialBuilder _cardMaterialBuilder;

	private readonly Queue<BotBattleTest> _testQueue = new Queue<BotBattleTest>();

	private BotBattleTest _currentTest;

	private readonly List<ErrorLog> _exceptions = new List<ErrorLog>();

	private readonly List<ErrorLog> _errors = new List<ErrorLog>();

	private readonly List<ErrorLog> _asserts = new List<ErrorLog>();

	private static bool _loadingScene;

	private void Awake()
	{
		if (UnityEngine.Object.FindObjectsOfType<BotBattleScene>().Length > 1)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private IEnumerator Start()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		yield return GetPapa();
		_accountClient = Pantry.Get<IAccountClient>();
		_matchManager = _papa.MatchManager;
		_matchManager.MatchCompleted += OnMatchCompleted;
		_cardDatabase = _papa.CardDatabase;
		_cardViewBuilder = _papa.CardViewBuilder;
		_cardMaterialBuilder = _papa.CardMaterialBuilder;
		SceneManager.sceneUnloaded += OnSceneUnloaded;
		Application.logMessageReceived += OnLogHandled;
		if (_testQueue.Count == 0)
		{
			LoadLauncher();
		}
		else
		{
			RunTest();
		}
		static string GetBotBattleLogPath()
		{
			return Path.Combine(Utilities.GetLogPath(), "BotBattleLogs");
		}
		string GetLogFilePath(string testName)
		{
			return Path.Combine(GetSessionTimeFilePath(), "BotBattleLog_" + testName + ".json");
		}
		IEnumerator GetPapa()
		{
			_papa = UnityEngine.Object.FindObjectOfType<PAPA>();
			if (!(_papa != null))
			{
				_papa = PAPA.Create();
				string previousFDServer = MDNPlayerPrefs.PreviousFDServer;
				Pantry.Get<EnvironmentManager>().SetEnvironmentByName(previousFDServer);
				AutoLoginState state = AutoLoginState.None;
				FrontDoorConnectionManager frontDoorConnectionManager = Pantry.Get<FrontDoorConnectionManager>();
				yield return frontDoorConnectionManager.TryFastLogIn(delegate(AutoLoginState x)
				{
					state = x;
				});
				if (state != AutoLoginState.Connected)
				{
					Debug.LogError("Failed to log in. Better go do that (Or make this page support it).");
				}
				else
				{
					yield return new LoadLocDatabaseUniTask().Load().ToCoroutine();
					yield return _papa.LoadCardDatabase();
					_papa.CreateFormatManager();
					AudioManager.InitializeAudio(_papa.AssetLookupSystem);
					_papa.InitializeNpe();
					_papa.InitializeMatchMaking();
				}
			}
		}
		static string GetSessionDateFilePath()
		{
			return Path.Combine(GetBotBattleLogPath(), $"BotBattle_{DateTime.Today:MM-dd-yyyy}");
		}
		string GetSessionTimeFilePath()
		{
			return Path.Combine(GetSessionDateFilePath(), _currentTest.StartTime.ToLocalTime().ToString("HH-mm-ss"));
		}
		void OnLogHandled(string logString, string stackTrace, LogType type)
		{
			ErrorLog item = new ErrorLog
			{
				Text = logString,
				StackTrace = stackTrace
			};
			switch (type)
			{
			case LogType.Exception:
				_exceptions.Add(item);
				if (_matchManager != null)
				{
					string text = GetSessionTimeFilePath();
					if (!Directory.Exists(text))
					{
						Directory.CreateDirectory(text);
					}
					ReplayUtilities.SaveReplay("BotBattleReplay_MatchId_" + (_matchManager.MatchID ?? "NULL_MATCH"), text, _matchManager.MessageHistory.Messages);
				}
				break;
			case LogType.Error:
				_errors.Add(item);
				break;
			case LogType.Assert:
				_asserts.Add(item);
				break;
			case LogType.Warning:
			case LogType.Log:
				break;
			}
		}
		void OnSceneUnloaded(Scene scene)
		{
			if (scene.name == "MatchScene")
			{
				Record();
				RunTest();
			}
		}
		void Record()
		{
			try
			{
				string path = GetBotBattleLogPath();
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
				string path2 = GetSessionDateFilePath();
				if (!Directory.Exists(path2))
				{
					Directory.CreateDirectory(path2);
				}
				string path3 = GetSessionTimeFilePath();
				if (!Directory.Exists(path3))
				{
					Directory.CreateDirectory(path3);
				}
				JObject value = new JObject
				{
					["Session"] = _currentTest.ToJObject(),
					["Exceptions"] = JArray.FromObject(_exceptions),
					["Errors"] = JArray.FromObject(_errors),
					["Asserts"] = JArray.FromObject(_asserts)
				};
				string text = _currentTest.DsConfig.FileName;
				if (string.IsNullOrEmpty(text))
				{
					text = "UNNAMEDTEST";
				}
				File.WriteAllText(GetLogFilePath(text), JsonConvert.SerializeObject(value));
			}
			catch (IOException exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	private void OnDestroy()
	{
		if (_matchManager != null)
		{
			_matchManager.MatchCompleted -= OnMatchCompleted;
		}
		_accountClient = null;
		_matchManager = null;
		_cardDatabase = null;
		_cardViewBuilder = null;
		_cardMaterialBuilder = null;
		_currentTest = null;
		_testQueue.Clear();
	}

	private static BotBattleTest GenerateTest(BotBattleDSConfig botBattleConfig, CardDatabase cardDatabase)
	{
		switch (botBattleConfig.SessionType)
		{
		case BotBattleSessionType.DeckTest:
			return new DeckTest(botBattleConfig, cardDatabase);
		case BotBattleSessionType.SetTest:
		case BotBattleSessionType.CardTest:
			return new SetTest(botBattleConfig, cardDatabase);
		default:
			return null;
		}
	}

	private void EnqueueTests(params BotBattleDSConfig[] configs)
	{
		if (configs == null || configs.Length == 0)
		{
			return;
		}
		for (int i = 0; i < configs.Length; i++)
		{
			BotBattleTest botBattleTest = GenerateTest(configs[i], _cardDatabase);
			if (botBattleTest != null)
			{
				_testQueue.Enqueue(botBattleTest);
			}
		}
	}

	private void RunTest()
	{
		BotBattleTest nextTest;
		if (CanRunCurrentTest())
		{
			LoadMatch();
		}
		else if (TryDequeueTest(out nextTest))
		{
			_currentTest = nextTest;
			RunTest();
		}
		bool CanRunCurrentTest()
		{
			if (_currentTest != null)
			{
				return !_currentTest.IsComplete();
			}
			return false;
		}
		void LoadMatch()
		{
			AccountInformation accountInformation = _accountClient?.AccountInformation;
			ConnectionConfig connectionConfig;
			if (accountInformation != null)
			{
				connectionConfig = ConnectConfig(accountInformation);
				Wizards.Arena.Client.Logging.ILogger logger = new ConsoleLogger();
				TcpConnection tcpConnection = new TcpConnection(logger, 17, ServicePointManager.ServerCertificateValidationCallback);
				IGREConnection connection = new GREConnection(Translate.ToConnectMessage(_cardDatabase.VersionProvider.DataVersion, 1u), new LoggingConfig
				{
					VerboseLogs = true
				}, new MatchTcpConnection(tcpConnection), logger, ClientType.User, RecordHistoryUtils.ShouldRecordHistory);
				_matchManager.Initialize(connection, _cardDatabase);
				MatchSceneManager.Load(_papa, null, _cardDatabase, _cardViewBuilder, _cardMaterialBuilder, connectToMatch);
			}
			void connectToMatch()
			{
				Debug.Log($"Connecting to GRE at {Pantry.CurrentEnvironment.mdHost}:{Pantry.CurrentEnvironment.mdPort}");
				_matchManager.OnConnectedToService += createMatch;
				_matchManager.OnRoomCreated += joinMatch;
				_matchManager.ConnectToMatchService(connectionConfig);
			}
			void joinMatch(string uri, string matchId)
			{
				_matchManager.OnRoomCreated -= joinMatch;
				_matchManager.JoinMatch(uri, matchId);
				Wizards.Arena.Client.Logging.ILogger logger2 = new ConsoleLogger();
				TcpConnection tcpConnection2 = new TcpConnection(logger2, 17, ServicePointManager.ServerCertificateValidationCallback);
				HeadlessClient headlessClient = new HeadlessClient(new GREConnection(Translate.ToConnectMessage(_cardDatabase.VersionProvider.DataVersion, 2u), new LoggingConfig(), new MatchTcpConnection(tcpConnection2), logger2, ClientType.Familiar, RecordHistoryUtils.ShouldRecordHistory), _currentTest.OpponentStrategy, _cardDatabase);
				headlessClient.ConnectAndJoinMatch(connectionConfig.CreateFamiliar(), uri, matchId);
				_matchManager.RegisterFamiliarClient(headlessClient);
			}
		}
		bool TryDequeueTest(out BotBattleTest reference)
		{
			while (_testQueue.Count > 0)
			{
				BotBattleTest botBattleTest = _testQueue.Dequeue();
				if (!botBattleTest.IsComplete())
				{
					reference = botBattleTest;
					return true;
				}
			}
			reference = null;
			return false;
		}
		void createMatch()
		{
			_matchManager.OnConnectedToService -= createMatch;
			_matchManager.CreateMatch(_currentTest.CreateMatchConfig());
			_matchManager.SetLocalPlayerStrategy(_currentTest.LocalPlayerStrategy);
		}
	}

	private ConnectionConfig ConnectConfig(AccountInformation accountInformation)
	{
		return new ConnectionConfig(Pantry.CurrentEnvironment.mdHost, Pantry.CurrentEnvironment.mdPort, accountInformation.PersonaID, accountInformation.Credentials.Jwt, Global.VersionInfo.ContentVersion.ToString(), MDNPlayerPrefs.InactivityTimeoutMs);
	}

	private void LoadLauncher()
	{
		BotBattleLauncher.Load(OnLoaded);
		static string GetConfigPath()
		{
			return Path.Combine(AutoPlayManager.GetConfigRoot, "BotBattleConfig.json");
		}
		static void OnExportConfigClicked(BotBattleDSConfig config)
		{
			if (config != null)
			{
				string getConfigRoot = AutoPlayManager.GetConfigRoot;
				if (!Directory.Exists(getConfigRoot))
				{
					Directory.CreateDirectory(getConfigRoot);
				}
				string contents = JsonConvert.SerializeObject(config, Formatting.None);
				File.WriteAllText(GetConfigPath(), contents);
			}
		}
		void OnLauncherUnloaded()
		{
			RunTest();
		}
		void OnLoaded(BotBattleLauncher botBattleLauncher)
		{
			botBattleLauncher.ExportConfigClicked += OnExportConfigClicked;
			botBattleLauncher.StartButtonClicked += OnStartButtonClicked;
		}
		void OnStartButtonClicked(BotBattleDSConfig config)
		{
			if (config != null)
			{
				EnqueueTests(config);
				BotBattleLauncher.Unload(OnLauncherUnloaded);
			}
		}
	}

	private void OnMatchCompleted()
	{
		_currentTest.OnMatchCompleted();
	}

	public static void Load(params BotBattleDSConfig[] configs)
	{
		if (_loadingScene)
		{
			Debug.LogError("BOTBATTLE LAUNCHER ALREADY LOADING");
			return;
		}
		_loadingScene = true;
		SceneManager.sceneLoaded += OnSceneLoaded;
		Scenes.LoadScene("BotBattleScene");
		void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
		{
			BotBattleScene sceneComponent = scene.GetSceneComponent<BotBattleScene>();
			if (sceneComponent != null)
			{
				sceneComponent.EnqueueTests(configs);
				_loadingScene = false;
				SceneManager.sceneLoaded -= OnSceneLoaded;
			}
		}
	}
}
