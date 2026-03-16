using System;
using System.Collections.Generic;
using System.IO;
using EventPage;
using Newtonsoft.Json;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;

public class EventPageGUI : IDebugGUIPage
{
	private List<string> eventDataPaths;

	private int lastEventCount;

	private int lastCourseCount;

	private EventPageContentController contentController;

	private string nPath;

	private DebugInfoIMGUIOnGui _GUI;

	private bool eventPathsAvailable => eventDataPaths != null;

	public DebugInfoIMGUIOnGui.DebugTab TabType => DebugInfoIMGUIOnGui.DebugTab.Events;

	public string TabName => "Events";

	public bool HiddenInTab => false;

	public void Init(DebugInfoIMGUIOnGui gui)
	{
		_GUI = gui;
	}

	public void Destroy()
	{
	}

	public void OnQuit()
	{
	}

	public bool OnUpdate()
	{
		return true;
	}

	private void CheckEventPaths()
	{
		eventDataPaths = MDNPlayerPrefs.DebugLocalEventPaths;
		Pantry.Get<IEventsServiceWrapper>();
	}

	public void OnGUI()
	{
		if (!eventPathsAvailable)
		{
			CheckEventPaths();
			return;
		}
		DrawMockEventUI();
		if (contentController != null)
		{
			_ = contentController.CurrentEventContext?.PlayerEvent.EventInfo.InternalEventName;
		}
		else if (Event.current.type == EventType.Repaint)
		{
			contentController = UnityEngine.Object.FindObjectOfType<EventPageContentController>();
		}
		if (eventDataPaths.Count != lastEventCount)
		{
			MDNPlayerPrefs.DebugLocalEventPaths = eventDataPaths;
			lastEventCount = eventDataPaths.Count;
		}
	}

	private void DrawMockEventUI()
	{
		DrawPathList("Event Data", eventDataPaths);
		if (_GUI.ShowDebugButton("Refresh", 250f))
		{
			SceneLoader.GetSceneLoader().GoToLanding(new HomePageContext(), forceReload: true);
		}
	}

	private void DrawPathList(string label, List<string> paths)
	{
		_GUI.ShowLabel(label, GUILayout.MaxWidth(500f));
		for (int i = 0; i < paths.Count; i++)
		{
			GUILayout.BeginHorizontal();
			_GUI.ShowBox(paths[i], GUILayout.MaxWidth(500f));
			if (_GUI.ShowDebugButton("-", 20f))
			{
				paths.RemoveAt(i);
				i--;
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.BeginHorizontal(GUILayout.MaxWidth(500f));
		nPath = GUILayout.TextField(nPath);
		if (GUILayout.Button("+", GUILayout.MaxWidth(30f)))
		{
			paths.Add(nPath);
		}
		GUILayout.EndHorizontal();
	}

	private List<T> DeserializeFromPaths<T>(List<string> paths)
	{
		List<T> list = new List<T>();
		JsonSerializer jsonSerializer = new JsonSerializer();
		foreach (string path in paths)
		{
			try
			{
				using StreamReader reader = File.OpenText(path);
				T item = (T)jsonSerializer.Deserialize(reader, typeof(T));
				list.Add(item);
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
		}
		return list;
	}
}
