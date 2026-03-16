using System;
using System.IO;
using System.Threading.Tasks;
using Core.Code.Promises;
using UnityEngine;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Mtga.Platforms;
using Wotc.Mtga;

namespace Core.Shared.Code.Utilities;

public static class LoggingUtils
{
	private const string HAVE_PURGED_LOGS = "1.04.00.03 Purge";

	public static LoggingConfig LoggingConfig;

	private static LogToFile _logToFile;

	private static UnityCrossThreadLogger _taskLogger;

	public static LogToFile LogToFile => _logToFile;

	public static void Initialize()
	{
		if (!Debug.isDebugBuild && !Application.isEditor)
		{
			Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
		}
		_taskLogger = new UnityCrossThreadLogger("TaskLogger");
		Wizards.Arena.Promises.TaskExtensions.Logger = _taskLogger;
		PromiseExtensions.Logger = _taskLogger;
		SimpleLog.Impl = new UnitySimpleLogImpl();
		LoggingConfig = new LoggingConfig();
		UpdateVerbosity(Pantry.Get<IAccountClient>());
		Debug.Log("DETAILED LOGS: " + (LoggingConfig.VerboseLogs ? "ENABLED" : "DISABLED"));
		Debug.Log($"Startup Timestamp: {DateTime.Now}");
	}

	public static void Flush()
	{
		_logToFile?.ForceWriteIfNotWriting();
	}

	public static void Shutdown()
	{
		_logToFile?.Shutdown();
		_logToFile = null;
	}

	public static void UpdateVerbosity(IAccountClient accountClient)
	{
		bool flag = MDNPlayerPrefs.GetUseVerboseLogs();
		AccountInformation accountInformation = accountClient.AccountInformation;
		if (accountInformation != null && accountInformation.HasRole_WotCAccess())
		{
			flag = true;
			if (!MDNPlayerPrefs.GetHasEverForcedVerboseLogs())
			{
				MDNPlayerPrefs.SetUseVerboseLogs(newValue: true);
				MDNPlayerPrefs.SetHasEverForcedVerboseLogs(newValue: true);
			}
		}
		LoggingConfig.VerboseLogs = flag;
		if (accountClient.AccountInformation != null && PlatformUtils.IsHandheld() && !flag)
		{
			DisableLogToFile();
		}
		else
		{
			EnableLogToFile();
		}
	}

	public static void InjectBiLogger(IBILogger biLogger)
	{
		_taskLogger.InjectBILogger(biLogger);
	}

	public static Task PurgeOldLogs()
	{
		bool purgeAllLogs = !PlayerPrefsExt.HasKey("1.04.00.03 Purge");
		return Task.Run(delegate
		{
			ClearOldLogs(purgeAllLogs);
		});
	}

	private static void EnableLogToFile()
	{
		if (_logToFile == null)
		{
			_logToFile = new LogToFile();
		}
	}

	private static void DisableLogToFile()
	{
		if (_logToFile != null)
		{
			_logToFile.ForceWriteIfNotWriting();
			_logToFile.Shutdown();
			_logToFile = null;
		}
	}

	private static void ClearOldLogs(bool purgeAllLogs = false)
	{
		FileInfo fileInfo = new FileInfo(ResourceErrorLogger.ResourceErrorManifestPath);
		try
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/MTGA/");
			if (directoryInfo.Exists)
			{
				directoryInfo.Delete(recursive: true);
			}
		}
		catch (Exception)
		{
		}
		try
		{
			DirectoryInfo directoryInfo2 = new DirectoryInfo(Wotc.Mtga.Utilities.GetLogPath());
			if (directoryInfo2.Exists)
			{
				DateTime utcNow = DateTime.UtcNow;
				FileInfo[] files = directoryInfo2.GetFiles("*", SearchOption.AllDirectories);
				foreach (FileInfo fileInfo2 in files)
				{
					try
					{
						if (!(fileInfo2.FullName == fileInfo.FullName))
						{
							TimeSpan timeSpan = utcNow - fileInfo2.LastWriteTimeUtc;
							if (purgeAllLogs || timeSpan.TotalDays > 3.0)
							{
								fileInfo2.Delete();
							}
						}
					}
					catch (Exception)
					{
					}
				}
			}
		}
		catch (Exception)
		{
		}
		if (MainThreadDispatcher.Instance != null)
		{
			MainThreadDispatcher.Instance.Add(delegate
			{
				PlayerPrefsExt.SetInt("1.04.00.03 Purge", 1, save: true);
			});
		}
	}
}
