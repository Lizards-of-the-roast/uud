using HasbroGo.Logging;
using Wizards.Mtga.Logging;

namespace MTGA.Social;

public class SocialLogger : ILogger
{
	private UnityLogger _logger;

	public SocialLogger(UnityLogger logger)
	{
		_logger = logger;
	}

	public void Log(LogLevel logLevel, string message)
	{
		switch (logLevel)
		{
		case LogLevel.Error:
			_logger.LogError(message);
			break;
		case LogLevel.Log:
		case LogLevel.Warning:
			break;
		}
	}

	public void Log(string category, LogLevel logLevel, string message)
	{
		Log(logLevel, category + ": " + message);
	}

	public void LogFormat(LogLevel logLevel, string message, params object[] args)
	{
		Log(logLevel, message);
	}

	public void LogFormat(string category, LogLevel logLevel, string message, params object[] args)
	{
		Log(logLevel, category + ": " + message);
	}
}
