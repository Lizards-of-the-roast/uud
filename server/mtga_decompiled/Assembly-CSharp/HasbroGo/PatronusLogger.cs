using HasbroGo.Logging;
using UnityEngine;

namespace HasbroGo;

internal class PatronusLogger : HasbroGo.Logging.ILogger
{
	public void Log(LogLevel logLevel, string message)
	{
		LogInternal(string.Empty, logLevel, message);
	}

	public void Log(string category, LogLevel logLevel, string message)
	{
		LogInternal(category, logLevel, message);
	}

	public void LogFormat(LogLevel logLevel, string message, params object[] args)
	{
		LogInternal(string.Empty, logLevel, string.Format(message, args));
	}

	public void LogFormat(string category, LogLevel logLevel, string message, params object[] args)
	{
		LogInternal(category, logLevel, string.Format(message, args));
	}

	private void LogInternal(string category, LogLevel logLevel, string message)
	{
		string text = (string.IsNullOrEmpty(category) ? (message ?? "") : ("[" + category + "]: " + message));
		text = "PatronusWiz - " + text;
		switch (logLevel)
		{
		case LogLevel.Log:
			Debug.Log(text);
			break;
		case LogLevel.Warning:
			Debug.LogWarning(text);
			break;
		case LogLevel.Error:
			Debug.LogError(text);
			break;
		}
	}
}
