using System;
using UnityEngine;

public static class ScriptableObjectClipboard
{
	private static Type _copyType;

	private static string _copyBuffer;

	[ContextMenu("Copy Object")]
	public static void CopyObject(ScriptableObject scriptableObject)
	{
		_copyType = scriptableObject.GetType();
		_copyBuffer = JsonUtility.ToJson(scriptableObject);
	}

	[ContextMenu("Paste Object Values")]
	public static void PasteValues(ScriptableObject scriptableObject)
	{
		if (scriptableObject.GetType().IsAssignableFrom(_copyType) && !string.IsNullOrEmpty(_copyBuffer))
		{
			JsonUtility.FromJsonOverwrite(_copyBuffer, scriptableObject);
		}
	}
}
