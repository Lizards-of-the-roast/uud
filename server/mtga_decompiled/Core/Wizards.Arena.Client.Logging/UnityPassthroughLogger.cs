using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Wizards.Arena.Client.Logging;

public class UnityPassthroughLogger : Logger
{
	public UnityPassthroughLogger(LoggerLevel loggerLevel = LoggerLevel.Warning)
		: base("unity passthrough", loggerLevel)
	{
	}

	protected override void LogImpl(LoggerLevel loggerLevel, string message, JObject additionalContext)
	{
		switch (loggerLevel)
		{
		case LoggerLevel.Emergency:
		case LoggerLevel.Alert:
		case LoggerLevel.Critical:
		case LoggerLevel.Error:
			UnityEngine.Debug.LogError($"{message}\n{additionalContext}");
			break;
		case LoggerLevel.Warning:
		case LoggerLevel.Notice:
			UnityEngine.Debug.LogWarning($"{message}\n{additionalContext}");
			break;
		case LoggerLevel.Info:
		case LoggerLevel.Debug:
			UnityEngine.Debug.Log($"{message}\n{additionalContext}");
			break;
		}
	}

	protected override void LogImpl(LoggerLevel loggerLevel, Exception exception, string message)
	{
		switch (loggerLevel)
		{
		case LoggerLevel.Emergency:
		case LoggerLevel.Alert:
		case LoggerLevel.Critical:
		case LoggerLevel.Error:
			UnityEngine.Debug.LogError($"[{exception.GetType()}] {exception.Message} -> {message}");
			break;
		case LoggerLevel.Warning:
		case LoggerLevel.Notice:
			UnityEngine.Debug.LogWarning($"[{exception.GetType()}] {exception.Message} -> {message}");
			break;
		case LoggerLevel.Info:
		case LoggerLevel.Debug:
			UnityEngine.Debug.Log($"[{exception.GetType()}] {exception.Message} -> {message}");
			break;
		}
	}

	protected override void LogImpl(LoggerLevel loggerLevel, Exception exception, string message, JObject additionalContext)
	{
		switch (loggerLevel)
		{
		case LoggerLevel.Emergency:
		case LoggerLevel.Alert:
		case LoggerLevel.Critical:
		case LoggerLevel.Error:
			UnityEngine.Debug.LogError($"[{exception.GetType()}] {exception.Message} -> {message}\n{additionalContext}");
			break;
		case LoggerLevel.Warning:
		case LoggerLevel.Notice:
			UnityEngine.Debug.LogWarning($"[{exception.GetType()}] {exception.Message} -> {message}\n{additionalContext}");
			break;
		case LoggerLevel.Info:
		case LoggerLevel.Debug:
			UnityEngine.Debug.Log($"[{exception.GetType()}] {exception.Message} -> {message}\n{additionalContext}");
			break;
		}
	}
}
