using System;
using System.Collections.Generic;
using UnityEngine;
using Wizards.Mtga.Platforms;

public class DebugLogGUI : MonoBehaviour
{
	private class LogData
	{
		public string Log;

		public string LogShort;

		public string StackTrace;

		public LogType logType;

		public bool ShowFull;
	}

	private static int _maxLogDataLength = 30;

	private List<LogData> _logData = new List<LogData>(_maxLogDataLength);

	private Vector2 _scrollPosition;

	private bool _canOpenOnException;

	private bool _ignoreFutureExceptions;

	private GUIStyle _guiStyle = new GUIStyle();

	private Dictionary<LogType, Color> _logColors = new Dictionary<LogType, Color>();

	private Dictionary<LogType, bool> _shouldDrawType = new Dictionary<LogType, bool>();

	private IMGUIDrawer _onGUIDrawer;

	private Func<bool> _hasDebugRole;

	private void Awake()
	{
		foreach (LogType value2 in EnumHelper.GetValues(typeof(LogType)))
		{
			_shouldDrawType.Add(value2, value: true);
			Color value = Color.white;
			switch (value2)
			{
			case LogType.Error:
			case LogType.Assert:
			case LogType.Exception:
				value = Color.red;
				break;
			case LogType.Warning:
				value = Color.yellow;
				break;
			}
			_logColors.Add(value2, value);
		}
		_guiStyle.fontSize = 20;
		_onGUIDrawer = base.gameObject.AddComponent<IMGUIDrawer>();
		_onGUIDrawer.Init(1, "Debug Logs", DrawGUI, 900, GetGUIWidth(), PlatformContext.GetIMGUIScale());
		_onGUIDrawer.enabled = false;
		Application.logMessageReceived += OnLogMessageReceived;
	}

	public void Init(Func<bool> hasDebugRole)
	{
		_hasDebugRole = hasDebugRole;
	}

	private void Update()
	{
		_canOpenOnException = HasDebugRole();
		if (_canOpenOnException && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.BackQuote))
		{
			_onGUIDrawer.enabled = !_onGUIDrawer.enabled;
		}
	}

	private bool HasDebugRole()
	{
		if (_hasDebugRole != null)
		{
			return _hasDebugRole();
		}
		return false;
	}

	private void OnDestroy()
	{
		_hasDebugRole = null;
		Application.logMessageReceived -= OnLogMessageReceived;
	}

	private int GetGUIWidth()
	{
		return (int)((double)Screen.height * 0.7);
	}

	private void DrawGUI(int windowId)
	{
		GUILayout.BeginVertical();
		GUILayout.BeginHorizontal();
		foreach (LogType value in EnumHelper.GetValues(typeof(LogType)))
		{
			_shouldDrawType[value] = GUILayout.Toggle(_shouldDrawType[value], value.ToString());
		}
		GUILayout.EndHorizontal();
		_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
		for (int i = 0; i < _logData.Count; i++)
		{
			LogData logData = _logData[i];
			if (_shouldDrawType[logData.logType])
			{
				GUILayout.BeginHorizontal();
				if (GUILayout.Button(logData.ShowFull ? "-" : "+", GUILayout.Width(50f)))
				{
					logData.ShowFull = !logData.ShowFull;
				}
				_guiStyle.normal.textColor = _logColors[logData.logType];
				if (!logData.ShowFull)
				{
					GUILayout.Label(logData.LogShort, _guiStyle);
				}
				else
				{
					GUILayout.Label(logData.Log, _guiStyle);
				}
				GUILayout.EndHorizontal();
				if (logData.ShowFull)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label(logData.StackTrace, _guiStyle);
					GUILayout.EndHorizontal();
				}
				GUILayout.Space(8f);
			}
		}
		GUILayout.EndScrollView();
		GUILayout.BeginHorizontal();
		_guiStyle.normal.textColor = Color.white;
		if (GUILayout.Button("Close", GUILayout.Width(90f), GUILayout.Height(60f)))
		{
			_onGUIDrawer.enabled = false;
		}
		_ignoreFutureExceptions = GUILayout.Toggle(_ignoreFutureExceptions, "Ignore Exceptions");
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
	}

	private void OnLogMessageReceived(string logString, string stackTrace, LogType type)
	{
		LogData logData = null;
		if (_logData.Count == _maxLogDataLength)
		{
			logData = _logData[_logData.Count - 1];
			logData.ShowFull = false;
			_logData.RemoveAt(_logData.Count - 1);
		}
		else
		{
			logData = new LogData();
		}
		logData.Log = logString;
		logData.LogShort = logString.Substring(0, Mathf.Min(logString.Length, 100)).Replace("\n", " ");
		logData.StackTrace = stackTrace;
		logData.logType = type;
		_logData.Insert(0, logData);
		if (!_ignoreFutureExceptions && _canOpenOnException && type == LogType.Exception)
		{
			_onGUIDrawer.enabled = true;
		}
	}
}
