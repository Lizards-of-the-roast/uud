using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public class DebugUI : MonoBehaviour
{
	private readonly List<DebugModule> _debugModules = new List<DebugModule>();

	private readonly List<GUIContent> _moduleTabContents = new List<GUIContent>();

	private readonly List<DebugModule> _activeModules = new List<DebugModule>();

	private GUISkin _debugUISkin;

	private Vector2 _moduleScroll = Vector2.zero;

	private bool _enabled;

	private const int WINDOW_WIDTH = 484;

	private IMGUIDrawer _IMGUIDrawer;

	private void Awake()
	{
		_IMGUIDrawer = base.gameObject.AddComponent<IMGUIDrawer>();
		_IMGUIDrawer.Init(1, "DuelScene Debug UI", DrawUI, 484, Screen.height, Matrix4x4.identity);
		_IMGUIDrawer.enabled = false;
	}

	private void Start()
	{
		_debugUISkin = Resources.Load<GUISkin>("DebugUISkin");
	}

	public void ToggleVisibility()
	{
		_enabled = !_enabled;
	}

	public void AddDebugModules(IEnumerable<DebugModule> debugModules)
	{
		foreach (DebugModule debugModule in debugModules)
		{
			_debugModules.Add(debugModule);
			_moduleTabContents.Add(new GUIContent(debugModule.Name, debugModule.Description));
		}
	}

	private bool IsDebugKeyPressed()
	{
		if (!UnityEngine.Input.GetKeyDown(KeyCode.F1))
		{
			return UnityEngine.Input.GetKeyDown(KeyCode.F2);
		}
		return true;
	}

	private void Update()
	{
		if (IsDebugKeyPressed())
		{
			_enabled = !_enabled;
			ToggleUIDisplay(_enabled);
			if (_enabled && UnityEngine.Input.GetKeyDown(KeyCode.F2) && !_activeModules.Exists((DebugModule x) => x.Name == "Player Controls") && _debugModules.Find((DebugModule x) => x.Name == "Player Controls") is AggregateTabModule aggregateTabModule)
			{
				aggregateTabModule.SetSelectionIndex(1);
				_activeModules.Add(aggregateTabModule);
			}
		}
	}

	private void ToggleUIDisplay(bool value)
	{
		_IMGUIDrawer.enabled = value;
	}

	private void OnDestroy()
	{
		while (_debugModules.Count > 0)
		{
			if (_debugModules[0] is IDisposable disposable)
			{
				disposable.Dispose();
			}
			_debugModules.RemoveAt(0);
		}
		_moduleTabContents.Clear();
		_activeModules.Clear();
	}

	public void DrawUI(int windowID)
	{
		GUI.skin = _debugUISkin;
		GUI.DragWindow(new Rect(0f, 0f, 10000f, 20f));
		DebugModule debugModule = DrawModuleSelections();
		GUILayout.Space(3f);
		MatchRenderer.RenderLine();
		GUILayout.Space(3f);
		RenderActiveModules();
		if (debugModule != null)
		{
			if (_activeModules.Contains(debugModule))
			{
				_activeModules.Remove(debugModule);
			}
			else
			{
				_activeModules.Add(debugModule);
				_activeModules.Sort((DebugModule x, DebugModule y) => _debugModules.IndexOf(x).CompareTo(_debugModules.IndexOf(y)));
			}
		}
		GUI.skin = null;
	}

	private void RenderActiveModules()
	{
		_moduleScroll = GUILayout.BeginScrollView(_moduleScroll);
		for (int i = 0; i < _activeModules.Count; i++)
		{
			DebugModule debugModule = _activeModules[i];
			if (i >= 1)
			{
				MatchRenderer.RenderLine();
				GUILayout.Space(1f);
			}
			debugModule.Render();
		}
		GUILayout.EndScrollView();
	}

	private DebugModule DrawModuleSelections()
	{
		DebugModule result = null;
		int num = 120;
		int num2 = Mathf.FloorToInt(GUIWidth() / (float)num);
		GUILayout.BeginVertical();
		GUILayout.BeginHorizontal();
		for (int i = 0; i < _debugModules.Count; i++)
		{
			DebugModule debugModule = _debugModules[i];
			GUIContent content = _moduleTabContents[i];
			GUI.backgroundColor = (_activeModules.Contains(debugModule) ? Color.green : Color.white);
			if (GUILayout.Button(content, GUILayout.Width(num)))
			{
				result = debugModule;
			}
			GUI.backgroundColor = Color.white;
			if ((i + 1) % num2 == 0)
			{
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
			}
		}
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		return result;
	}

	private float GUIWidth()
	{
		return 484f;
	}
}
