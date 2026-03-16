using Wizards.Arena.Client.Logging;
using Wizards.Mtga.Logging;

namespace GreClient.Network;

public class UnityConnectionLogger : IConnectionLogger
{
	private const string LOGGED_MESSAGE = "Reconnecting to match. Attempt #{0}";

	private readonly UnityLogger _logger;

	public UnityConnectionLogger(ILogger logger)
	{
		if (logger is UnityLogger logger2)
		{
			_logger = logger2;
		}
	}

	public void LogConnectAttempt(uint attemptCount)
	{
		if (_logger != null)
		{
			_logger.LogDebugForRelease($"Reconnecting to match. Attempt #{attemptCount}");
		}
	}
}
