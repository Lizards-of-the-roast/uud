using System;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Wizards.Arena.Client.Logging;

namespace Wizards.Mtga.Logging;

public class UnityLogger : Wizards.Arena.Client.Logging.Logger
{
	private string prefix;

	public UnityLogger(string loggerName, LoggerLevel loggerLevel)
		: base(loggerName, loggerLevel)
	{
		prefix = "[" + loggerName + "] ";
	}

	[Conditional("DEBUG")]
	public void LogDebug(string message)
	{
		LogImpl(LoggerLevel.Debug, message, null);
	}

	public void LogDebugForRelease(string message)
	{
		LogImplForRelease(LoggerLevel.Debug, message, null);
	}

	[Conditional("DEBUG")]
	public void LogDebugFormat(string message, params object[] args)
	{
		LogImpl(LoggerLevel.Debug, string.Format(message, args), null);
	}

	[Conditional("DEBUG")]
	public void LogWarning(string message)
	{
		LogImpl(LoggerLevel.Warning, message, null);
	}

	public void LogWarningForRelease(string message)
	{
		LogImplForRelease(LoggerLevel.Warning, message, null);
	}

	[Conditional("DEBUG")]
	public void LogWarningFormat(string message, params object[] args)
	{
		LogImpl(LoggerLevel.Warning, string.Format(message, args), null);
	}

	public void LogError(string message)
	{
		LogImpl(LoggerLevel.Error, message, null);
	}

	public void LogErrorFormat(string message, params object[] args)
	{
		UnityEngine.Debug.LogErrorFormat(message, args);
	}

	public void LogException(Exception ex)
	{
		LogImpl(LoggerLevel.Error, ex, "");
	}

	protected override void LogImpl(LoggerLevel loggerLevel, string message, JObject additionalContext)
	{
		if (base.Level >= loggerLevel)
		{
			if (loggerLevel <= LoggerLevel.Error)
			{
				UnityEngine.Debug.LogError(prefix + message);
			}
			else if (loggerLevel == LoggerLevel.Warning)
			{
				UnityEngine.Debug.LogWarning(prefix + message);
			}
			else
			{
				UnityEngine.Debug.Log(prefix + message);
			}
		}
	}

	protected void LogImplForRelease(LoggerLevel loggerLevel, string message, JObject additionalContext)
	{
		if (base.Level >= loggerLevel || loggerLevel <= LoggerLevel.Warning)
		{
			if (loggerLevel <= LoggerLevel.Error)
			{
				UnityEngine.Debug.LogError(prefix + message);
			}
			else if (loggerLevel == LoggerLevel.Warning)
			{
				UnityEngine.Debug.LogWarning(prefix + message);
			}
			else
			{
				UnityEngine.Debug.Log(prefix + message);
			}
		}
	}

	protected override void LogImpl(LoggerLevel loggerLevel, Exception exception, string message)
	{
		if (base.Level >= loggerLevel)
		{
			if (exception != null)
			{
				UnityEngine.Debug.LogException(exception);
			}
			else
			{
				LogImpl(loggerLevel, message, null);
			}
		}
	}

	protected override void LogImpl(LoggerLevel loggerLevel, Exception exception, string message, JObject additionalContext)
	{
		if (base.Level >= loggerLevel)
		{
			if (exception != null)
			{
				UnityEngine.Debug.LogException(exception);
			}
			else
			{
				LogImpl(loggerLevel, message, null);
			}
		}
	}
}
