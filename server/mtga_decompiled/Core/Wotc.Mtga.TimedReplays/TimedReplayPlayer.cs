using System;
using UnityEngine;
using Wizards.Mtga.IO;
using Wotc.Mtga.AutoPlay;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.TimedReplays;

public class TimedReplayPlayer : MonoBehaviour
{
	private static readonly string _replayPath = Application.persistentDataPath + "/AutoReplay.rply";

	private WorkflowTranslator_TimedReplay _replayTranslation;

	private ReplayReader _replay;

	private readonly ReplayMessageFilter _filter = new ReplayMessageFilter(ReplayMessageFilter.CommonGREMessagesToFilter, ReplayMessageFilter.CommonClientMessagesToFilter);

	private MatchManager _matchManager;

	private MatchSceneManager _matchSceneManager;

	private float _elapsedTimeMS;

	private const long TIMEOUT_MS = 60000L;

	private bool _errored;

	public static string NextReplay { get; set; }

	public event System.Action OnDone;

	public event Action<string> OnError;

	public static bool IsReplayAvailable()
	{
		if (NextReplay == null)
		{
			return WindowsSafePath.FileExists(_replayPath);
		}
		return true;
	}

	public static void Create(MatchManager matchManager, MatchSceneManager matchSceneManager, NPEController npeController, WorkflowTranslator_TimedReplay translation)
	{
		new GameObject("TimedReplayPlayer").AddComponent<TimedReplayPlayer>().Initialize(matchManager, matchSceneManager, npeController, translation, NextReplay ?? _replayPath);
	}

	public void Initialize(MatchManager matchManager, MatchSceneManager matchSceneManager, NPEController npeController, WorkflowTranslator_TimedReplay replayTranslation, string replayPath)
	{
		_elapsedTimeMS = 0f;
		_matchManager = matchManager;
		_matchSceneManager = matchSceneManager;
		_replayTranslation = replayTranslation;
		NextReplay = null;
		_matchSceneManager.MatchEndSceneCreated += MatchSceneManagerOnMatchEndSceneCreated;
		_matchManager.MessageReceived += MatchManagerOnMessageReceived;
		ReplayReader.Result result = ReplayReader.TryCreateReplayFromPath(replayPath, out _replay, _filter);
		if (result.ResultType == ReplayReader.Result.ResultTypes.DeserializeException)
		{
			OnReplayError(result.FailMessage);
		}
		if ((bool)npeController)
		{
			npeController.AutoSkipTooltips = true;
		}
	}

	private void OnReplayError(string message)
	{
		Error($"Parsing Error at line {_replay?.LineCount ?? 0}:\n {message}");
	}

	private void Error(string message)
	{
		_matchSceneManager.MatchEndSceneCreated -= MatchSceneManagerOnMatchEndSceneCreated;
		_matchManager.MessageReceived -= MatchManagerOnMessageReceived;
		SystemMessageManager.Instance.ShowOk("Replay Error: Please Check Replay File", message, delegate
		{
			_matchSceneManager.ExitMatchScene();
		});
		_errored = true;
		this.OnError?.Invoke(message);
		SimpleLog.LogError(message);
	}

	private void MatchManagerOnMessageReceived(GREToClientMessage receivedMessage)
	{
		if (_filter.ShouldIgnore(receivedMessage))
		{
			return;
		}
		if (_replay.IsPassedEnd())
		{
			Error($"Unexpected additional match manager message received: {receivedMessage}");
			return;
		}
		ReplayEntry currentMessage = _replay.GetCurrentMessage();
		if (currentMessage.GREToClient == null)
		{
			Error($"Unexpected GRE message received. Received message: {receivedMessage}");
			return;
		}
		if (!currentMessage.GREToClient.Equals(receivedMessage))
		{
			string text = $"Unexpected GRE contents received: {receivedMessage}.\nExpected: {currentMessage.GREToClient}";
			if (currentMessage.GREToClient.Type != receivedMessage.Type)
			{
				Error(text);
				return;
			}
			SimpleLog.LogWarningForRelease(text);
		}
		MoveToNextMessage();
		_elapsedTimeMS = currentMessage.Timestamp;
	}

	private void Update()
	{
		if (_errored)
		{
			return;
		}
		_elapsedTimeMS += Time.deltaTime * 1000f;
		if (_replay.IsPassedEnd())
		{
			return;
		}
		ReplayEntry currentMessage = _replay.GetCurrentMessage();
		if (currentMessage.ClientToGRE == null)
		{
			if ((float)(currentMessage.Timestamp + 60000) < _elapsedTimeMS)
			{
				Error($"Timeout Exceeded. Expected a message from GRE at {currentMessage.Timestamp} and it is {_elapsedTimeMS}. {currentMessage.GREToClient}");
			}
		}
		else if (!(_elapsedTimeMS <= (float)currentMessage.Timestamp))
		{
			_elapsedTimeMS = currentMessage.Timestamp;
			MoveToNextMessage();
			_replayTranslation.SendResponse(currentMessage.ClientToGRE);
		}
	}

	private void MatchSceneManagerOnMatchEndSceneCreated(MatchEndScene matchEndScene)
	{
		_matchSceneManager.MatchEndSceneCreated -= MatchSceneManagerOnMatchEndSceneCreated;
		if (!_replay.IsPassedEnd())
		{
			SimpleLog.LogError("There are remaining GRE messages but the match end scene has appeared.");
		}
		matchEndScene.EndOfMatchControlsEnabled += LeaveScene;
		void LeaveScene()
		{
			matchEndScene.EndOfMatchControlsEnabled -= LeaveScene;
			matchEndScene.LeaveMatch();
			this.OnDone?.Invoke();
		}
	}

	private void OnDestroy()
	{
		if (_matchManager != null)
		{
			_matchManager.MessageReceived -= MatchManagerOnMessageReceived;
		}
	}

	private void MoveToNextMessage()
	{
		ReplayReader.Result result = _replay.TryMoveToNextMessage();
		if (result.ResultType == ReplayReader.Result.ResultTypes.DeserializeException)
		{
			OnReplayError(result.FailMessage);
		}
	}
}
