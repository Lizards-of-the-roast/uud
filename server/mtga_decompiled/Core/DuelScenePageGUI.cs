using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.DuelScene;

public class DuelScenePageGUI : IDebugGUIPage
{
	private DebugInfoIMGUIOnGui _GUI;

	public DebugInfoIMGUIOnGui.DebugTab TabType => DebugInfoIMGUIOnGui.DebugTab.DuelScene;

	public string TabName => "DuelScene";

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

	public void OnGUI()
	{
		if (MatchSceneManager.Instance != null)
		{
			DebugUI debugUI = Object.FindObjectOfType<DebugUI>();
			if (debugUI != null)
			{
				debugUI.ToggleVisibility();
			}
		}
		bool fixedRulesTextSize = MDNPlayerPrefs.FixedRulesTextSize;
		GUILayout.BeginHorizontal();
		fixedRulesTextSize = _GUI.ShowToggleWithStyle(fixedRulesTextSize, "Show larger text in card textboxes", GUILayout.ExpandHeight(expand: false), GUILayout.ExpandWidth(expand: false));
		_GUI.ShowLabel("Show enlarged text in card textboxes");
		GUILayout.EndHorizontal();
		if (MDNPlayerPrefs.FixedRulesTextSize != fixedRulesTextSize)
		{
			MDNPlayerPrefs.FixedRulesTextSize = fixedRulesTextSize;
		}
		OverridesConfiguration local = OverridesConfiguration.Local;
		GUILayout.BeginHorizontal();
		bool value = local.HasFeatureToggleValue("MP_A") && local.GetFeatureToggleValue("MP_A");
		value = _GUI.ShowToggleWithStyle(value, "MP_A", GUILayout.ExpandHeight(expand: false), GUILayout.ExpandWidth(expand: false));
		_GUI.ShowLabel("MP_A (relaunch required)");
		TryWriteFeatureToggle(local, "MP_A", value);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		bool value2 = local.HasFeatureToggleValue("MP_B") && local.GetFeatureToggleValue("MP_B");
		value2 = _GUI.ShowToggleWithStyle(value2, "MP_B", GUILayout.ExpandHeight(expand: false), GUILayout.ExpandWidth(expand: false));
		_GUI.ShowLabel("MP_B (relaunch required)");
		TryWriteFeatureToggle(local, "MP_B", value2);
		GUILayout.EndHorizontal();
	}

	private static void TryWriteFeatureToggle(OverridesConfiguration config, string key, bool val)
	{
		if (config.HasFeatureToggleValue(key))
		{
			if (config.GetFeatureToggleValue(key) != val)
			{
				config.SetFeatureToggleValue(key, val);
				OverridesConfiguration.Local = config;
			}
		}
		else if (val)
		{
			config.SetFeatureToggleValue(key, val);
			OverridesConfiguration.Local = config;
		}
	}
}
