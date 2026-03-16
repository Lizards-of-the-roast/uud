using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioHistory
{
	private readonly Queue<AudioHistoryEntry> _queue = new Queue<AudioHistoryEntry>();

	private int maxSize = 50;

	private string search = string.Empty;

	private static Vector2 scrollpos;

	public void Enqueue(AudioHistoryEntry entry)
	{
		_queue.Enqueue(entry);
		if (_queue.Count > maxSize)
		{
			_queue.Dequeue();
		}
	}

	public AudioHistoryEntry Get(int index)
	{
		if (index < _queue.Count)
		{
			return _queue.ElementAt(index);
		}
		return _queue.ElementAt(0);
	}

	public void DoOnGui()
	{
		GUILayout.Label("------- Audio Debug -------");
		GUILayout.BeginVertical();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Search: ");
		GUILayout.FlexibleSpace();
		search = GUILayout.TextField(search, GUILayout.MinWidth(1000f));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.Label(string.Format("{0}\t{1}\t{2}", "Event", "Object", "Time"));
		scrollpos = GUILayout.BeginScrollView(scrollpos);
		foreach (AudioHistoryEntry item in _queue)
		{
			if ((string.IsNullOrEmpty(item._event) || FastString.StartsWith(item._event, search)) && item._event.Contains(search))
			{
				GUILayout.BeginHorizontal();
				Color contentColor = GUI.contentColor;
				if (item.failed)
				{
					GUI.contentColor = Color.red;
				}
				GUILayout.Label(item._event);
				GUILayout.FlexibleSpace();
				GUILayout.Label(item.objectname);
				GUILayout.FlexibleSpace();
				GUILayout.Label(item.time.ToString());
				GUI.contentColor = contentColor;
				GUILayout.EndHorizontal();
			}
		}
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
	}
}
