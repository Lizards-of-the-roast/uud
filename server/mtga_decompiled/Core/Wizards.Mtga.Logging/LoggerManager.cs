using System.Collections.Generic;
using Wizards.Arena.Client.Logging;

namespace Wizards.Mtga.Logging;

public static class LoggerManager
{
	public class LoggerInfo
	{
		public string Name;

		public LoggerLevel Level;

		public int Count;

		public override string ToString()
		{
			return $"[LoggerInfo Name={Name},Level={Level},Count={Count}]";
		}
	}

	private static Dictionary<string, List<Logger>> loggers = new Dictionary<string, List<Logger>>();

	public static void Register(Logger logger)
	{
		List<Logger> value = null;
		if (!loggers.TryGetValue(logger.Name, out value))
		{
			value = new List<Logger>();
			loggers[logger.Name] = value;
		}
		value.Add(logger);
	}

	public static void Unregister(Logger logger)
	{
		if (loggers.TryGetValue(logger.Name, out var value))
		{
			value.Remove(logger);
		}
	}

	public static List<LoggerInfo> GetLoggerInfo()
	{
		List<LoggerInfo> list = new List<LoggerInfo>();
		foreach (KeyValuePair<string, List<Logger>> logger in loggers)
		{
			if (logger.Value.Count > 0)
			{
				LoggerInfo item = new LoggerInfo
				{
					Name = logger.Key,
					Level = logger.Value[0].Level,
					Count = logger.Value.Count
				};
				list.Add(item);
			}
		}
		return list;
	}

	public static void OverrideLogLevel(string name, LoggerLevel level)
	{
		List<Logger> value = null;
		if (!loggers.TryGetValue(name, out value))
		{
			return;
		}
		foreach (Logger item in value)
		{
			item.Level = level;
		}
	}

	public static void Reset()
	{
		loggers.Clear();
	}
}
