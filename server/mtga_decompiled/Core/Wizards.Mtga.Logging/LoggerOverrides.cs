using System;
using System.Collections.Generic;
using UnityEngine;
using Wizards.Arena.Client.Logging;

namespace Wizards.Mtga.Logging;

[CreateAssetMenu(fileName = "LoggerOverride", menuName = "ScriptableObject/Logger Override", order = 0)]
public class LoggerOverrides : ScriptableObject
{
	[Serializable]
	public class LoggerOverride
	{
		public string name;

		public LoggerLevel overrideLevel;
	}

	public List<LoggerOverride> overrides = new List<LoggerOverride>();

	public void Apply(Wizards.Arena.Client.Logging.Logger logger)
	{
		foreach (LoggerOverride @override in overrides)
		{
			if (@override.name == logger.Name)
			{
				logger.Level = @override.overrideLevel;
				break;
			}
		}
	}
}
