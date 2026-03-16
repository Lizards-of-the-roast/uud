using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
using Wizards.Mtga;
using Wotc.Mtga.Quality;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Core.Shared.Code.DebugTools;

public class PerformanceCSVLogger : IDisposable
{
	private class CSVData
	{
		public float FrameTime;

		public float TotalCPUUsage;

		public float HeapSize;

		public float HeapUsed;

		public float AllocatedSize;

		public float ReservedSize;

		public BatteryStatus BatteryStatus;

		public float BatteryLevel;

		public int ThermalStatus;

		public int LogMessages;

		public int LogWarnings;

		public int LogErrors;

		public List<string> BundlesLoaded = new List<string>(10);

		public List<string> BundlesUnloaded = new List<string>(10);

		public List<string> GREMessages = new List<string>(10);

		private StringBuilder sb = new StringBuilder();

		public static uint Version => 2u;

		public int BundlesLoadedCount => BundlesLoaded.Count;

		public int BundlesUnloadedCount => BundlesUnloaded.Count;

		public static string GenerateCSVHeader()
		{
			return "FrameTime, TotalCPUUsage, HeapSize, HeapUsed, AllocatedSize, ReservedSize, BatteryStatus, BatteryLevel, ThermalStatus, BundlesLoadedCount, BundlesUnloadedCount, LogMessages, LogWarnings, LogErrors, BundlesLoaded, BundlesUnloaded, GREMessages\n";
		}

		public string GenerateCSVLine()
		{
			return $"{FrameTime}, " + $"{TotalCPUUsage}, " + $"{HeapSize}, " + $"{HeapUsed}, " + $"{AllocatedSize}, " + $"{ReservedSize}, " + Enum.GetName(typeof(BatteryStatus), BatteryStatus) + ", " + $"{BatteryLevel}, " + $"{ThermalStatus}, " + $"{BundlesLoadedCount}, " + $"{BundlesUnloadedCount}, " + $"{LogMessages}, " + $"{LogWarnings}, " + $"{LogErrors}, " + EscapeStringArray(BundlesLoaded) + ", " + EscapeStringArray(BundlesUnloaded) + ",  " + EscapeStringArray(GREMessages) + "\n";
		}

		public static string GenerateHardwareAndBuildData()
		{
			StringBuilder stringBuilder = new StringBuilder();
			List<(string, string)> list = new List<(string, string)>();
			list.Add(("Data Version", Version.ToString()));
			list.Add(("deviceModel", SystemInfo.deviceModel));
			list.Add(("deviceType", SystemInfo.deviceType.ToString()));
			list.Add(("operatingSystem", SystemInfo.operatingSystem));
			list.Add(("systemMemorySize", SystemInfo.systemMemorySize.ToString()));
			list.Add(("graphicsDeviceName", SystemInfo.graphicsDeviceName));
			list.Add(("graphicsMemorySize", SystemInfo.graphicsMemorySize.ToString()));
			list.Add(("Version number", Global.VersionInfo.GetFullVersionString()));
			list.Add(("Quality Level", QualitySettingsUtil.Instance.CurrentTierId));
			list.Add(("Unity Quality Level", QualitySettings.names[QualitySettings.GetQualityLevel()]));
			foreach (var item in list)
			{
				stringBuilder.Append(item.Item1);
				stringBuilder.Append(',');
			}
			stringBuilder.Append('\n');
			foreach (var item2 in list)
			{
				stringBuilder.Append('"');
				stringBuilder.Append(item2.Item2.Replace("\"", "\"\""));
				stringBuilder.Append('"');
				stringBuilder.Append(',');
			}
			stringBuilder.Append('\n');
			stringBuilder.Append('\n');
			return stringBuilder.ToString();
		}

		private string EscapeStringArray(List<string> inputs)
		{
			sb.Clear();
			sb.Append('[');
			int num = 0;
			foreach (string input in inputs)
			{
				sb.Append(input);
				sb.Append(',');
				num++;
			}
			if (sb.Length > 1)
			{
				sb.Remove(sb.Length - 1, 1);
			}
			sb.Append(']');
			sb.Replace("\"", "\"\"");
			sb.Replace("\n", "");
			sb.Insert(0, "\"");
			sb.Append("\"");
			return sb.ToString();
		}
	}

	private readonly AssetBundleManager _assetBundleManager;

	private readonly MatchManager _matchManager;

	private readonly CSVData _csvRow;

	private readonly StreamWriter _fileStream;

	private static int _csvLoggersActive;

	private readonly IThermalStatusProvider _thermalStatusProvider;

	private readonly ICPUUsageProvider _cpuUsageProvider;

	public static bool ShouldRunDSS
	{
		get
		{
			if (!MDNPlayerPrefs.RunningDSS)
			{
				return OverridesConfiguration.Local.GetFeatureToggleValue("RunningDSS");
			}
			return true;
		}
	}

	public static bool IsReporting => _csvLoggersActive > 0;

	public PerformanceCSVLogger(AssetBundleManager assetBundleManager, MatchManager matchManager, IThermalStatusProvider thermalStatusProvider, ICPUUsageProvider cpuUsageProvider, string folderPath, string nameOverride)
	{
		_csvLoggersActive++;
		_assetBundleManager = assetBundleManager;
		_assetBundleManager.AssetBundleLoaded += OnAssetBundleLoaded;
		_assetBundleManager.AssetBundleUnloaded += OnAssetBundleUnloaded;
		_matchManager = matchManager;
		_matchManager.MessageSent += GREMessageSent;
		_matchManager.MessageReceived += GREMessageReceived;
		_thermalStatusProvider = thermalStatusProvider;
		_cpuUsageProvider = cpuUsageProvider;
		Application.logMessageReceived += OnLogMessageReceived;
		_csvRow = new CSVData();
		if (!string.IsNullOrEmpty(nameOverride))
		{
			nameOverride += "_";
		}
		string text = Path.Combine(folderPath, nameOverride + "report_" + DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss") + ".csv");
		SimpleLog.LogForRelease("DSS reporting to file: " + text);
		FileSystemUtils.CreateDirectory(folderPath);
		_fileStream = FileSystemUtils.CreateText(text);
		_fileStream.Write(CSVData.GenerateHardwareAndBuildData());
		_fileStream.Write(CSVData.GenerateCSVHeader());
	}

	private void OnLogMessageReceived(string condition, string stacktrace, LogType type)
	{
		switch (type)
		{
		case LogType.Log:
			_csvRow.LogMessages++;
			break;
		case LogType.Warning:
			_csvRow.LogWarnings++;
			break;
		case LogType.Error:
		case LogType.Assert:
		case LogType.Exception:
			_csvRow.LogErrors++;
			break;
		}
	}

	private void GREMessageReceived(GREToClientMessage message)
	{
		_csvRow.GREMessages.Add(message.ToString());
	}

	private void GREMessageSent(ClientToGREMessage message)
	{
		_csvRow.GREMessages.Add(message.ToString());
	}

	private void OnAssetBundleLoaded(string name)
	{
		_csvRow.BundlesLoaded.Add(name);
	}

	private void OnAssetBundleUnloaded(string name)
	{
		_csvRow.BundlesUnloaded.Add(name);
	}

	public void Update(float deltaTime)
	{
		_csvRow.FrameTime = deltaTime;
		_csvRow.HeapSize = Profiler.GetMonoHeapSizeLong();
		_csvRow.HeapUsed = Profiler.GetMonoUsedSizeLong();
		_csvRow.AllocatedSize = Profiler.GetTotalAllocatedMemoryLong();
		_csvRow.ReservedSize = Profiler.GetTotalReservedMemoryLong();
		_csvRow.BatteryStatus = SystemInfo.batteryStatus;
		_csvRow.BatteryLevel = SystemInfo.batteryLevel;
		_csvRow.ThermalStatus = _thermalStatusProvider.GetThermalStatus();
		_csvRow.TotalCPUUsage = _cpuUsageProvider.GetTotalCPUUsage();
		_fileStream.Write(_csvRow.GenerateCSVLine());
		_csvRow.GREMessages.Clear();
		_csvRow.BundlesLoaded.Clear();
		_csvRow.BundlesUnloaded.Clear();
		_csvRow.LogMessages = 0;
		_csvRow.LogWarnings = 0;
		_csvRow.LogErrors = 0;
	}

	public void Flush()
	{
		_fileStream.Flush();
	}

	public void Dispose()
	{
		_csvLoggersActive--;
		_fileStream.Close();
		if (_assetBundleManager != null)
		{
			_assetBundleManager.AssetBundleLoaded -= OnAssetBundleLoaded;
			_assetBundleManager.AssetBundleUnloaded -= OnAssetBundleUnloaded;
		}
		if (_matchManager != null)
		{
			_matchManager.MessageSent -= GREMessageSent;
			_matchManager.MessageReceived -= GREMessageReceived;
		}
		Application.logMessageReceived -= OnLogMessageReceived;
	}
}
