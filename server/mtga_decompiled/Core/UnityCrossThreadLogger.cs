using System;
using System.Collections.Concurrent;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Wizards.Arena.Client.Logging;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;

public class UnityCrossThreadLogger : Wizards.Arena.Client.Logging.Logger
{
	private readonly ConcurrentQueue<Action> _toMainThreadQueue = new ConcurrentQueue<Action>();

	private readonly Thread _unityThread;

	private IBILogger _biLogger;

	private MatchManager _matchManager;

	public UnityCrossThreadLogger(string name = "UnityCrossThreadLogger")
		: base(name, LoggerLevel.Debug)
	{
		_unityThread = Thread.CurrentThread;
		PAPA.UpdateEvent += Update;
	}

	public void InjectBILogger(IBILogger biLogger)
	{
		_biLogger = biLogger;
	}

	public void InjectMatchManager(MatchManager matchManager)
	{
		_matchManager = matchManager;
	}

	public void ClearMatchManager()
	{
		_matchManager = null;
	}

	private void Update()
	{
		int num = 0;
		Action result;
		while (_toMainThreadQueue.TryDequeue(out result))
		{
			result?.Invoke();
			if (++num > 20)
			{
				break;
			}
		}
		if (_toMainThreadQueue.Count > 200)
		{
			Action result2;
			while (_toMainThreadQueue.TryDequeue(out result2))
			{
			}
		}
	}

	public void Shutdown()
	{
		PAPA.UpdateEvent -= Update;
	}

	protected override void LogImpl(LoggerLevel loggerLevel, Exception exception, string message)
	{
		LogImpl(loggerLevel, exception, message, null);
	}

	protected override void LogImpl(LoggerLevel loggerLevel, Exception exception, string message, JObject additionalContext)
	{
		if (additionalContext != null)
		{
			EnqueueLogAction(delegate
			{
				UnityEngine.Debug.LogError($"Additional Context will be ignored, please create a new BI Message to handle this context: {additionalContext}");
			});
		}
		if (!string.IsNullOrEmpty(base.Name))
		{
			message = "[" + base.Name + "]" + message;
		}
		if (loggerLevel <= LoggerLevel.Error)
		{
			EnqueueBILog(loggerLevel, message);
		}
		else if (loggerLevel <= LoggerLevel.Warning)
		{
			EnqueueLogAction(delegate
			{
				SimpleLog.LogWarningForRelease(message);
			});
		}
		else
		{
			EnqueueLogAction(delegate
			{
				SimpleLog.LogForRelease(message);
			});
		}
	}

	protected override void LogImpl(LoggerLevel loggerLevel, string message, JObject additionalContext)
	{
		if (!string.IsNullOrEmpty(base.Name))
		{
			message = "[" + base.Name + "]" + message;
		}
		string logMessage = message;
		if (additionalContext != null)
		{
			logMessage = message + " " + additionalContext.ToString(Formatting.None);
		}
		switch (loggerLevel)
		{
		case LoggerLevel.Emergency:
		case LoggerLevel.Alert:
		case LoggerLevel.Critical:
		case LoggerLevel.Error:
			EnqueueBILog(loggerLevel, message);
			EnqueueLogAction(delegate
			{
				SimpleLog.LogError(logMessage);
			});
			break;
		case LoggerLevel.Warning:
			EnqueueLogAction(delegate
			{
				SimpleLog.LogWarningForRelease(logMessage);
			});
			break;
		case LoggerLevel.Notice:
		case LoggerLevel.Info:
		case LoggerLevel.Debug:
			EnqueueLogAction(delegate
			{
				SimpleLog.LogForRelease(logMessage);
			});
			break;
		}
	}

	private void EnqueueBILog(LoggerLevel loggerLevel, string message)
	{
		string humanContext = message.Split('\r', '\n')[0];
		string matchID = ((_matchManager?.GreConnection != null && !string.IsNullOrEmpty(_matchManager?.MatchID)) ? _matchManager.MatchID : string.Empty);
		switch (loggerLevel)
		{
		case LoggerLevel.Emergency:
			EnqueueLogAction(delegate
			{
				SimpleLog.LogError(message);
				_biLogger?.Send(ClientBusinessEventType.ClientLogEmergency, new ClientLogEmergency
				{
					Message = message,
					HumanContext = humanContext,
					EventTime = DateTime.UtcNow,
					MatchID = matchID
				});
			});
			break;
		case LoggerLevel.Alert:
			EnqueueLogAction(delegate
			{
				SimpleLog.LogError(message);
				_biLogger?.Send(ClientBusinessEventType.ClientLogAlert, new ClientLogAlert
				{
					Message = message,
					HumanContext = humanContext,
					EventTime = DateTime.UtcNow,
					MatchID = matchID
				});
			});
			break;
		case LoggerLevel.Critical:
			EnqueueLogAction(delegate
			{
				SimpleLog.LogError(message);
				_biLogger?.Send(ClientBusinessEventType.ClientLogCritical, new ClientLogCritical
				{
					Message = message,
					HumanContext = humanContext,
					EventTime = DateTime.UtcNow,
					MatchID = matchID
				});
			});
			break;
		case LoggerLevel.Error:
			EnqueueLogAction(delegate
			{
				SimpleLog.LogError(message);
				_biLogger?.Send(ClientBusinessEventType.ClientLogError, new ClientLogError
				{
					Message = message,
					HumanContext = humanContext,
					EventTime = DateTime.UtcNow,
					MatchID = matchID
				});
			});
			break;
		}
	}

	private void EnqueueLogAction(Action a)
	{
		if (Thread.CurrentThread.Equals(_unityThread))
		{
			a();
		}
		else
		{
			_toMainThreadQueue.Enqueue(a);
		}
	}
}
