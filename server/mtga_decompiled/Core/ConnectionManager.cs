using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using Assets.Core.Code.Doorbell;
using Core.BI;
using Core.Code.Input;
using Core.Code.SceneUtils;
using Core.Shared.Code.Connection;
using Cysharp.Threading.Tasks;
using GreClient.Network;
using MTGA.KeyboardManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wizards.Arena.Client.Logging;
using Wizards.Arena.Promises;
using Wizards.Arena.TcpConnection;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Logging;
using Wotc.Mtga.Login;
using Wotc.Mtga.Network.ServiceWrappers;

public class ConnectionManager : MonoBehaviour, IDisposable
{
	private const string MATCH_RECONNECT_LOG_ALREADY_RECONNECTING = "Aborting reconnect, because we are already reconnecting.";

	private const string MATCH_RECONNECT_LOG_CONNECTION_ALREADY_EXISTS = "Aborting reconnect, because we are already connected.";

	private const string MATCH_RECONNECT_LOG_START_RECONNECT = "Starting FD & GRE reconnect coroutine";

	private const string MATCH_RECONNECT_PRE_RECONNECT_CLOSURE_REASON = "Cleanup before reconnecting";

	private const uint MAX_GRE_RECONNECT_ATTEMPTS = 15u;

	private static WaitForSeconds MATCH_RECONNECT_DELAY = new WaitForSeconds(4f);

	public Action OnFdReconnected;

	private bool _reconnecting;

	private bool _reconnectingToGre;

	private int _maxReconnectAttempts = 1;

	private int _delayBetweenAttempts = 4;

	private int _maxTimePerAttempt = 6;

	private IConnectionStatusResponder _responder;

	private IConnectionIndicator _connectionIndicator;

	private FrontDoorConnectionManager _fdConnectionManager;

	private IActiveMatchesServiceWrapper _activeMatchesWrapper;

	private IFrontDoorConnectionServiceWrapper _connectionServiceWrapper;

	private IAccountClient _accountClient;

	private MatchManager _matchManager;

	private Matchmaking _matchmaking;

	private EventManager _eventManager;

	private AssetLookupSystem _assetLookupSystem;

	private KeyboardManager _keyboardManager;

	private IActionSystem _actions;

	private IBILogger _biLogger;

	private UnityLogger _logger;

	private SettingsMenuHost _settingsMenuHost;

	private bool _disposed;

	public DoorbellRingResponseV2 DoorbellResponse { get; set; }

	public bool Connected => _connectionServiceWrapper?.Connected ?? false;

	public static ConnectionManager Create()
	{
		GameObject obj = new GameObject("ConnectionHelper");
		UnityEngine.Object.DontDestroyOnLoad(obj);
		return obj.AddComponent<ConnectionManager>();
	}

	public void Init(FrontDoorConnectionManager fdConnectionManager, IFrontDoorConnectionServiceWrapper connectionServiceWrapper, IActiveMatchesServiceWrapper activeMatchesServiceWrapper, IAccountClient accountClient, MatchManager matchManager, Matchmaking matchmaking, EventManager eventManager, AssetLookupSystem assetLookupSystem, KeyboardManager keyboardManager, IActionSystem actions, IBILogger biLogger, SettingsMenuHost settingsMenuHost, IConnectionStatusResponder responder = null, IConnectionIndicator connectionIndicator = null)
	{
		if (responder == null)
		{
			responder = new NullConnectionStatusResponder();
		}
		if (connectionIndicator == null)
		{
			connectionIndicator = new NullConnectionIndicator();
		}
		_fdConnectionManager = fdConnectionManager;
		_connectionServiceWrapper = connectionServiceWrapper;
		_accountClient = accountClient;
		_matchManager = matchManager;
		_settingsMenuHost = settingsMenuHost;
		_matchmaking = matchmaking;
		_eventManager = eventManager;
		_assetLookupSystem = assetLookupSystem;
		_keyboardManager = keyboardManager;
		_actions = actions;
		_biLogger = biLogger;
		_responder = responder;
		_connectionIndicator = connectionIndicator;
		_activeMatchesWrapper = activeMatchesServiceWrapper;
		_logger = new UnityLogger("ConnectionManager", LoggerLevel.Debug);
		LoggerManager.Register(_logger);
		_fdConnectionManager.OnConnectionLost += Reconnect;
	}

	public IEnumerator RingDoorbell()
	{
		string installId = BILoggingUtils.InstallId;
		Promise<DoorbellRingResponseV2> ring = Doorbell.RingDoorbell(_fdConnectionManager.CurrentEnvironment, Global.VersionInfo, installId);
		yield return ring.AsCoroutine();
		if (!ring.Successful)
		{
			_logger.LogDebugForRelease($"Doorbell error! {ring.Error}");
			BIEventType.ProdUriError.SendWithDefaults(("Error", ring.Error.Message));
			yield break;
		}
		if (string.IsNullOrEmpty(_fdConnectionManager.CurrentEnvironment.fdHost))
		{
			string[] array = ring.Result.FdURI.Replace("tcp://", "").Split(":");
			_fdConnectionManager.CurrentEnvironment.fdHost = array.FirstOrDefault();
			if (array.Length > 1)
			{
				int.TryParse(array[1], out _fdConnectionManager.CurrentEnvironment.fdPort);
				if (_fdConnectionManager.CurrentEnvironment.fdPort == 0)
				{
					_fdConnectionManager.CurrentEnvironment.fdPort = 443;
				}
			}
		}
		DoorbellResponse = ring.Result;
		BIEventType.ProdUriDetermined.SendWithDefaults(("Uri", _fdConnectionManager.CurrentEnvironment.GetFullUri()));
	}

	public IEnumerator FullLogIn()
	{
		LoadSceneUniTask loadSceneUniTask = new LoadSceneUniTask("Login");
		yield return loadSceneUniTask.Load().ToCoroutine();
		LoginScene sceneComponent = SceneManager.GetSceneByName("Login").GetSceneComponent<LoginScene>();
		sceneComponent.Inject(_connectionServiceWrapper, _accountClient, _fdConnectionManager.CurrentEnvironment, this, _assetLookupSystem, _keyboardManager, _actions, _biLogger, _matchManager, _settingsMenuHost);
		bool loginComplete = false;
		sceneComponent.LoggedIn += delegate
		{
			loginComplete = true;
		};
		yield return new WaitUntil(() => loginComplete);
	}

	public void Reconnect(TcpConnectionCloseType closeType = TcpConnectionCloseType.NormalClosure)
	{
		if (closeType == TcpConnectionCloseType.SocketFailure)
		{
			_reconnecting = false;
		}
		if (_reconnecting || _connectionServiceWrapper.Connected)
		{
			return;
		}
		switch (closeType)
		{
		case TcpConnectionCloseType.ClosedByServer:
		case TcpConnectionCloseType.SocketFailure:
			_responder.OnConnectionClosedByServer();
			return;
		case TcpConnectionCloseType.ClientSideIdle:
			_responder.OnConnectionClosedByIdleTimeout();
			return;
		}
		if (!UpdatePoliciesPanel.NeedsToAcceptPolicy)
		{
			_reconnecting = true;
			StartCoroutine(Coroutine_Reconnect());
		}
	}

	private IEnumerator Coroutine_Reconnect()
	{
		int curAttempt = 0;
		AutoLoginState state = AutoLoginState.None;
		while (true)
		{
			_connectionIndicator.ShowReconnectIndicator(shouldEnable: true);
			_logger.LogDebugForRelease($"Starting reconnect attempt #{curAttempt + 1}");
			state = AutoLoginState.None;
			_connectionServiceWrapper.Close(TcpConnectionCloseType.NormalClosure, "Cleanup before reconnecting");
			yield return new WaitUntil(() => !_connectionServiceWrapper.Connected);
			DateTime timeout = DateTime.UtcNow + TimeSpan.FromSeconds(_maxTimePerAttempt);
			Coroutine fastLoginCoroutine = StartCoroutine(_fdConnectionManager.ReconnectYield(delegate(AutoLoginState x)
			{
				state = x;
			}));
			yield return new WaitUntil(() => state != AutoLoginState.None || DateTime.UtcNow > timeout);
			if (DateTime.UtcNow > timeout)
			{
				_logger.LogDebugForRelease("Reconnect timed out");
				StopCoroutine(fastLoginCoroutine);
			}
			curAttempt++;
			_logger.LogDebugForRelease("Reconnect result : " + state);
			if (state == AutoLoginState.Connected || state == AutoLoginState.InQueue || curAttempt >= _maxReconnectAttempts)
			{
				break;
			}
			yield return new WaitForSeconds(_delayBetweenAttempts);
			_connectionIndicator.ShowReconnectIndicator(shouldEnable: false);
		}
		_connectionIndicator.ShowReconnectIndicator(shouldEnable: false);
		_reconnecting = false;
		if (state == AutoLoginState.None || state == AutoLoginState.Error)
		{
			_logger.LogDebugForRelease("Reconnect failed");
			_responder.OnReconnectFailed();
			yield break;
		}
		_logger.LogDebugForRelease($"Reconnect succeeded after {curAttempt} attempts");
		OnFdReconnected?.Invoke();
		if (_matchmaking?.IsMatchReady() ?? false)
		{
			_logger.LogDebugForRelease("Still in a match....reconnecting to GRE.");
			yield return Coroutine_ReconnectToGre();
		}
	}

	public void RefreshMatchConnection()
	{
		if (_reconnectingToGre)
		{
			_logger.LogDebugForRelease("Aborting reconnect, because we are already reconnecting.");
			return;
		}
		if (_matchManager.ConnectionState == ConnectionState.Connected)
		{
			_logger.LogDebugForRelease("Aborting reconnect, because we are already connected.");
			return;
		}
		_logger.LogDebugForRelease("Starting FD & GRE reconnect coroutine");
		_connectionServiceWrapper.Close(TcpConnectionCloseType.NormalClosure, "Cleanup before reconnecting");
		Reconnect();
	}

	private IEnumerator Coroutine_ReconnectToGre()
	{
		_reconnectingToGre = true;
		_connectionIndicator.ShowReconnectIndicator(shouldEnable: true);
		Promise<List<NewMatchCreatedConfig>> retryPromise = RetryPromise<List<NewMatchCreatedConfig>>.Create(delegate
		{
			_logger.LogDebugForRelease("ConnectionHelper.Coroutine_ReconnectToGre: Fetching active matches");
			return _activeMatchesWrapper.GetActiveMatches();
		}, (Promise<List<NewMatchCreatedConfig>> p) => !p.Successful, (int tries) => TimeSpan.FromSeconds(_maxTimePerAttempt), new RetryTermination.MaxRetries(_maxReconnectAttempts));
		yield return retryPromise.AsCoroutine();
		NewMatchCreatedConfig newMatchCreatedConfig = retryPromise.Result?.FirstOrDefault();
		if (newMatchCreatedConfig != null)
		{
			_logger.LogDebugForRelease("ConnectionHelper.Coroutine_ReconnectToGre: Active match found");
			MatchConnectionConfig connectionConfig = MatchConnectionConfig(newMatchCreatedConfig.matchEndpointHost, newMatchCreatedConfig.matchEndpointPort, newMatchCreatedConfig.controllerFabricUri, newMatchCreatedConfig.matchId);
			IMatchConnection matchConnection = InitializeMatchConnection(_matchManager, connectionConfig, _logger);
			while (!matchConnection.State.ConnectionIsComplete())
			{
				matchConnection.Connect();
				yield return MATCH_RECONNECT_DELAY;
			}
			if (matchConnection.State == ConnectionState.Connected)
			{
				_logger.LogDebugForRelease("ConnectionHelper.Coroutine_ReconnectToGre: Reconnected to GRE");
				HandleGreReconnectSuccess();
			}
			else
			{
				HandleGreReconnectFailure();
			}
		}
		else
		{
			_responder.NoActiveMatchFound();
			_connectionIndicator.ShowReconnectIndicator(shouldEnable: false);
		}
		_reconnectingToGre = false;
	}

	public static IMatchConnection InitializeMatchConnection(MatchManager matchManager, MatchConnectionConfig connectionConfig, Wizards.Arena.Client.Logging.ILogger logger)
	{
		if (matchManager == null || !connectionConfig.IsValid())
		{
			return new FailedConnection();
		}
		return InitializeMatchConnection(new MatchConnection(matchManager, connectionConfig), 15u, logger);
	}

	public static IMatchConnection InitializeMatchConnection(IMatchConnection matchConnection, uint attemptCounts, Wizards.Arena.Client.Logging.ILogger logger)
	{
		if (matchConnection == null)
		{
			return new FailedConnection();
		}
		IMatchConnection connection = new AttemptConstrainedConnection(attemptCounts, matchConnection);
		return new LoggedMatchConnection(new UnityConnectionLogger(logger), connection);
	}

	private MatchConnectionConfig MatchConnectionConfig(string host, int port, string uri, string matchId)
	{
		return new MatchConnectionConfig(ConnectionConfig(host, port), uri, matchId);
	}

	private ConnectionConfig ConnectionConfig(string host, int port)
	{
		return new ConnectionConfig(host, port, _accountClient.AccountInformation.PersonaID, _accountClient.AccountInformation.Credentials.Jwt, Global.VersionInfo.ContentVersion.ToString(), MDNPlayerPrefs.InactivityTimeoutMs);
	}

	private void HandleGreReconnectSuccess()
	{
		_connectionIndicator.ShowReconnectIndicator(shouldEnable: false);
	}

	private void HandleGreReconnectFailure()
	{
		_logger.LogDebugForRelease("Failed to reconnect to GRE");
		_connectionIndicator.ShowReconnectIndicator(shouldEnable: false);
		_responder.OnMatchReconnectFailed();
	}

	private void OnDestroy()
	{
		if (_fdConnectionManager != null)
		{
			_fdConnectionManager.OnConnectionLost -= Reconnect;
		}
		if (_logger != null)
		{
			LoggerManager.Unregister(_logger);
		}
		OnFdReconnected = null;
		_disposed = true;
	}

	public void Dispose()
	{
		if (!_disposed && base.gameObject != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
