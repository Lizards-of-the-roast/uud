using System;
using System.Collections;
using UnityEngine;

namespace Wotc.Mtga.Extensions;

public static class GUILayoutExtensions
{
	public static void DrawList(IList currentList, string listName, Action<int> drawElement, Action<int> removeElement, Action<int> addElement, bool allowReordering = true)
	{
		GUILayout.BeginVertical();
		GUILayout.Label(listName + ":");
		GUILayout.BeginVertical(new GUIStyle(GUI.skin.box));
		for (int i = 0; i < currentList.Count; i++)
		{
			GUILayout.BeginHorizontal(new GUIStyle(GUI.skin.box));
			drawElement(i);
			if (allowReordering)
			{
				if (GUILayout.Button((i > 0) ? "▲" : "   ", GUILayout.ExpandWidth(expand: false)) && i > 0)
				{
					object value = currentList[i];
					currentList.RemoveAt(i);
					currentList.Insert(i - 1, value);
					break;
				}
				if (GUILayout.Button((i < currentList.Count - 1) ? "▼" : "   ", GUILayout.ExpandWidth(expand: false)) && i < currentList.Count - 1)
				{
					object value2 = currentList[i];
					currentList.RemoveAt(i);
					currentList.Insert(i + 1, value2);
					break;
				}
			}
			GUI.backgroundColor = Color.red;
			if (removeElement != null && GUILayout.Button("X", GUILayout.ExpandWidth(expand: false)))
			{
				removeElement(i);
				break;
			}
			GUI.backgroundColor = Color.green;
			if (addElement != null && GUILayout.Button("+", GUILayout.ExpandWidth(expand: false)))
			{
				addElement(i);
				break;
			}
			GUI.backgroundColor = Color.white;
			GUILayout.EndHorizontal();
		}
		if (addElement != null)
		{
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUI.backgroundColor = Color.green;
			if (GUILayout.Button("+", GUILayout.ExpandWidth(expand: false)))
			{
				addElement(currentList.Count);
			}
			GUI.backgroundColor = Color.white;
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
		GUILayout.EndVertical();
	}
}
