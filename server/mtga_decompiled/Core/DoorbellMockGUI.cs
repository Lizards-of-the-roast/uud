using UnityEngine;

internal class DoorbellMockGUI
{
	private static GUIStyle _textInputStyleCache;

	private static GUIStyle _textInputStyle
	{
		get
		{
			if (_textInputStyleCache == null)
			{
				_textInputStyleCache = new GUIStyle(GUI.skin.GetStyle("TextField"));
			}
			return _textInputStyleCache;
		}
	}

	public void DoOnGUI()
	{
		GUILayout.BeginHorizontal();
		MDNPlayerPrefs.DoorbellOverrideToggle = GUILayout.Toggle(MDNPlayerPrefs.DoorbellOverrideToggle, "Doorbell Response Override");
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Doorbell Response Override Content", GUILayout.Width(250f));
		MDNPlayerPrefs.DoorbellOverrideContent = GUILayout.TextField(MDNPlayerPrefs.DoorbellOverrideContent, _textInputStyle);
		GUILayout.EndHorizontal();
	}
}
