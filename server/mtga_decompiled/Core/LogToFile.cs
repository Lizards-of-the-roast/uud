using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Wotc.Mtga;

public class LogToFile
{
	private class LogData
	{
		public string Log;

		public string StackTrace;

		public int Frame;
	}

	private bool _writingToFile;

	private float _timeSinceLastWrite;

	private bool _forceWrite;

	private static float _timeBetweenWrites = 30f;

	private List<LogData> _logsBeingWritten = new List<LogData>(50);

	private List<LogData> _logs = new List<LogData>(50);

	private string _logFilePath;

	public LogToFile()
	{
		LogToFile logToFile = this;
		string text = Path.Combine(Utilities.GetLogPath(), "Logs");
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		_logFilePath = Path.Combine(text, string.Format("UTC_Log - {0}.log", DateTime.UtcNow.ToString("MM-dd-yyyy HH.mm.ss")));
		string systemSpecsString = GetSystemSpecsString();
		Task.Run(delegate
		{
			logToFile.WriteInitialData(systemSpecsString);
		});
		Application.logMessageReceived += OnLogMessageReceived;
	}

	public void Shutdown()
	{
		Application.logMessageReceived -= OnLogMessageReceived;
	}

	public void Update()
	{
		if (!_writingToFile)
		{
			_timeSinceLastWrite += Time.deltaTime;
			if (_logs.Count > 0 && (_forceWrite || _timeSinceLastWrite > _timeBetweenWrites))
			{
				_forceWrite = false;
				_timeSinceLastWrite = 0f;
				_logsBeingWritten.Clear();
				_logsBeingWritten.AddRange(_logs);
				_logs.Clear();
				Task.Run((Action)WriteToFile);
			}
		}
	}

	private void OnLogMessageReceived(string logString, string stackTrace, LogType type)
	{
		_logs.Add(new LogData
		{
			Log = logString,
			StackTrace = stackTrace,
			Frame = Time.frameCount
		});
		if (type == LogType.Exception)
		{
			_forceWrite = true;
		}
	}

	private string GetSystemSpecsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("graphicsDeviceName ");
		stringBuilder.AppendLine(SystemInfo.graphicsDeviceName);
		stringBuilder.Append("graphicsDeviceType ");
		stringBuilder.AppendLine(SystemInfo.graphicsDeviceType.ToString());
		stringBuilder.Append("graphicsDeviceVendor ");
		stringBuilder.AppendLine(SystemInfo.graphicsDeviceVendor);
		stringBuilder.Append("graphicsDeviceVersion ");
		stringBuilder.AppendLine(SystemInfo.graphicsDeviceVersion);
		stringBuilder.Append("graphicsMemorySize ");
		stringBuilder.AppendLine(SystemInfo.graphicsMemorySize.ToString());
		stringBuilder.Append("graphicsMultiThreaded ");
		stringBuilder.AppendLine(SystemInfo.graphicsMultiThreaded.ToString());
		stringBuilder.Append("graphicsShaderLevel ");
		stringBuilder.AppendLine(SystemInfo.graphicsShaderLevel.ToString());
		stringBuilder.Append("deviceModel ");
		stringBuilder.AppendLine(SystemInfo.deviceModel);
		stringBuilder.Append("deviceType ");
		stringBuilder.AppendLine(SystemInfo.deviceType.ToString());
		stringBuilder.Append("operatingSystem ");
		stringBuilder.AppendLine(SystemInfo.operatingSystem);
		stringBuilder.Append("operatingSystemFamily ");
		stringBuilder.AppendLine(SystemInfo.operatingSystemFamily.ToString());
		stringBuilder.Append("processorCount ");
		stringBuilder.AppendLine(SystemInfo.processorCount.ToString());
		stringBuilder.Append("processorFrequency ");
		stringBuilder.AppendLine(SystemInfo.processorFrequency.ToString());
		stringBuilder.Append("processorType ");
		stringBuilder.AppendLine(SystemInfo.processorType);
		stringBuilder.Append("systemMemorySize ");
		stringBuilder.AppendLine(SystemInfo.systemMemorySize.ToString());
		stringBuilder.Append("maxTextureSize ");
		stringBuilder.AppendLine(SystemInfo.maxTextureSize.ToString());
		return stringBuilder.ToString();
	}

	private void WriteInitialData(string systemSpecsString)
	{
		_writingToFile = true;
		try
		{
			using StreamWriter streamWriter = FileSystemUtils.CreateText(_logFilePath);
			streamWriter.Write(systemSpecsString);
		}
		catch
		{
		}
		_writingToFile = false;
	}

	private void WriteToFile()
	{
		_writingToFile = true;
		try
		{
			using StreamWriter streamWriter = File.AppendText(_logFilePath);
			for (int i = 0; i < _logsBeingWritten.Count; i++)
			{
				LogData logData = _logsBeingWritten[i];
				streamWriter.Write('[');
				streamWriter.Write(logData.Frame);
				streamWriter.Write("] ");
				streamWriter.WriteLine(logData.Log);
				streamWriter.Write(logData.StackTrace);
			}
		}
		catch
		{
		}
		_writingToFile = false;
	}

	public IEnumerator CopyCurrentLogToPath(string targetPath)
	{
		while (_writingToFile)
		{
			yield return null;
		}
		ForceWriteIfNotWriting();
		try
		{
			FileSystemUtils.CopyFile(_logFilePath, FileSystemUtils.GenerateUniqueFilePath(targetPath));
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public void ForceWriteIfNotWriting()
	{
		if (!_writingToFile && _logs.Count > 0)
		{
			_logsBeingWritten.Clear();
			_logsBeingWritten.AddRange(_logs);
			_logs.Clear();
			WriteToFile();
		}
	}
}
