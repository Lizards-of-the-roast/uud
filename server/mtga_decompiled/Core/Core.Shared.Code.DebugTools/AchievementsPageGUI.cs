using Core.Code.ClientFeatureToggle;
using UnityEngine;
using Wizards.Mtga;

namespace Core.Shared.Code.DebugTools;

public class AchievementsPageGUI : IDebugGUIPage
{
	private DebugInfoIMGUIOnGui _GUI;

	private static GUIStyle _textInputStyleCache;

	private static GUIStyle _guiStyleWordWrapWhite;

	public DebugInfoIMGUIOnGui.DebugTab TabType => DebugInfoIMGUIOnGui.DebugTab.Achievements;

	public string TabName => "Achievements";

	public bool HiddenInTab
	{
		get
		{
			ClientFeatureToggleDataProvider clientFeatureToggleDataProvider = Pantry.Get<ClientFeatureToggleDataProvider>();
			if (clientFeatureToggleDataProvider != null)
			{
				return !clientFeatureToggleDataProvider.GetToggleValueById("AchievementSceneStatus");
			}
			return true;
		}
	}

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
		if (_GUI.ShowDebugButton("Reload mock data", 200f))
		{
			SceneLoader.GetSceneLoader().GetAchievementsNavController();
		}
	}
}
