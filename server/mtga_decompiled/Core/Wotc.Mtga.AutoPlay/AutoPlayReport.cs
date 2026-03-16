using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Core.Shared.Code.DebugTools;
using UnityEngine;
using UnityEngine.Profiling;
using Wizards.Mtga;

namespace Wotc.Mtga.AutoPlay;

public class AutoPlayReport : IDisposable
{
	private class MinMax
	{
		public long Min = long.MaxValue;

		public long Max = long.MinValue;

		public void Set(long val)
		{
			if (val < Min)
			{
				Min = val;
			}
			if (val > Max)
			{
				Max = val;
			}
		}
	}

	private readonly List<(float frameTimeLowerBound, int count)> _frameBuckets;

	private PerformanceCSVLogger _logger;

	private readonly DateTime _startTime;

	private int _exceptions;

	private int _errors;

	private int _lowMemoryWarnings;

	private int _totalFrames;

	private string _scriptName;

	private readonly MinMax _monoHeapSizes = new MinMax();

	private readonly MinMax _monoUsedSizes = new MinMax();

	private readonly MinMax _allocatedSizes = new MinMax();

	private readonly MinMax _reservedSizes = new MinMax();

	public AutoPlayAction.Failure FailDetails;

	public int AssetErrors { get; private set; }

	public bool Success { get; private set; }

	public AutoPlayReport(string scriptName)
	{
		_scriptName = scriptName;
		string folderPath = Path.Combine(Utilities.GetLogPath(), "AutoplayLogs", "Canary");
		if (AssetBundleManager.Instance != null)
		{
			_logger = new PerformanceCSVLogger(AssetBundleManager.Instance, Pantry.Get<MatchManager>(), Pantry.Get<IThermalStatusProvider>(), Pantry.Get<ICPUUsageProvider>(), folderPath, scriptName);
		}
		_startTime = DateTime.UtcNow;
		_frameBuckets = new List<(float, int)>
		{
			(1f / 29f, 0),
			(0.04f, 0),
			(0.05f, 0),
			(0.1f, 0),
			(0.2f, 0),
			(1f, 0)
		};
		ResourceErrorLogger.OnAssetBundleError += OnAssetBundleError;
		Application.logMessageReceived += ApplicationOnLogMessageReceived;
		Application.lowMemory += ApplicationOnlowMemory;
	}

	public void Dispose()
	{
		ResourceErrorLogger.OnAssetBundleError -= OnAssetBundleError;
		Application.logMessageReceived -= ApplicationOnLogMessageReceived;
		Application.lowMemory -= ApplicationOnlowMemory;
	}

	public static string Color(string text, string color)
	{
		return "<color=" + color + ">" + text + "</color>";
	}

	public string GenerateStringReport()
	{
		double totalSeconds = (DateTime.UtcNow - _startTime).TotalSeconds;
		int num = (int)((double)_totalFrames / totalSeconds);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Exceptions: " + ColorIfGreater(_exceptions, 0, "red"));
		stringBuilder.AppendLine("Asset Exceptions: " + ColorIfGreater(AssetErrors, 0, "red"));
		stringBuilder.AppendLine("Log Errors: " + ColorIfGreater(_errors, 0, "yellow"));
		stringBuilder.AppendLine("");
		stringBuilder.AppendLine("Memory");
		stringBuilder.AppendLine("Low memory warnings: " + ColorIfGreater(_lowMemoryWarnings, 0, "yellow"));
		stringBuilder.AppendLine("Mono Heap " + MinMaxMBString(_monoHeapSizes));
		stringBuilder.AppendLine("Mono Used " + MinMaxMBString(_monoUsedSizes));
		stringBuilder.AppendLine("Allocated " + MinMaxMBString(_allocatedSizes));
		stringBuilder.AppendLine("Reserved " + MinMaxMBString(_reservedSizes));
		stringBuilder.AppendLine("");
		stringBuilder.AppendLine("Frame Rate");
		stringBuilder.AppendLine($"Average FPS: {num}");
		stringBuilder.AppendLine($"Total frames: {_totalFrames}");
		foreach (var (num2, num3) in _frameBuckets)
		{
			stringBuilder.AppendLine($"<{1f / num2:0.#} fps: {(float)num3 / (float)_totalFrames * 100f:0.00}% ({num3})");
		}
		return stringBuilder.ToString();
		static string ColorIfGreater(int value, int bound, string color)
		{
			return ((value > bound) ? Color(value.ToString(), color) : value.ToString()) ?? "";
		}
		static string MinMaxMBString(MinMax mm)
		{
			return $"Usage: {ToMegabytes(mm.Min)}MB -> {ToMegabytes(mm.Max)}MB (+{ToMegabytes(mm.Max - mm.Min)}MB)";
		}
		static long ToMegabytes(long bytes)
		{
			return bytes / 1000000;
		}
	}

	public void Update(float deltaTime)
	{
		_logger?.Update(deltaTime);
		_totalFrames++;
		TrackFrame(deltaTime);
		_monoHeapSizes.Set(Profiler.GetMonoHeapSizeLong());
		_monoUsedSizes.Set(Profiler.GetMonoUsedSizeLong());
		_allocatedSizes.Set(Profiler.GetTotalAllocatedMemoryLong());
		_reservedSizes.Set(Profiler.GetTotalReservedMemoryLong());
	}

	private void TrackFrame(float frameTime)
	{
		for (int i = 0; i < _frameBuckets.Count; i++)
		{
			float num;
			int num2;
			(num, num2) = _frameBuckets[i];
			if (frameTime > num)
			{
				_frameBuckets[i] = (num, ++num2);
			}
		}
	}

	private void ApplicationOnLogMessageReceived(string condition, string stacktrace, LogType type)
	{
		if (type == LogType.Exception)
		{
			_exceptions++;
		}
		if (type == LogType.Error)
		{
			_errors++;
		}
	}

	private void OnAssetBundleError(string message, Dictionary<string, string> details)
	{
		AssetErrors++;
	}

	private void ApplicationOnlowMemory()
	{
		_lowMemoryWarnings++;
	}

	public void SetSuccess()
	{
		Success = true;
	}

	public void SetFailure(AutoPlayAction.Failure failDetails)
	{
		Success = false;
		FailDetails = failDetails;
	}
}
