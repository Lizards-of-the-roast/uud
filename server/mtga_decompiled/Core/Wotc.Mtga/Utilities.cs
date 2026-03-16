using System;
using System.IO;
using Wizards.Mtga.Platforms;

namespace Wotc.Mtga;

public static class Utilities
{
	public static string GetLogPath()
	{
		string text = PlatformContext.GetStorageContext().LocalPersistedStoragePath + "/Logs/";
		try
		{
			Directory.CreateDirectory(text);
		}
		catch (Exception)
		{
		}
		return text;
	}

	public static string GetCommandLineArg(string name)
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i].Equals(name, StringComparison.OrdinalIgnoreCase) && commandLineArgs.Length > i + 1)
			{
				return commandLineArgs[i + 1];
			}
		}
		return string.Empty;
	}

	public static bool HasCommandLineArg(string name)
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i].Equals(name, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}
}
