using UnityEngine;

public class BackgroundDownloadingGUI
{
	public void DoOnGUI()
	{
		GUILayout.BeginHorizontal();
		MDNPlayerPrefs.IsBackgroundDownloadEnabled = GUILayout.Toggle(MDNPlayerPrefs.IsBackgroundDownloadEnabled, "Is Background Download Enabled");
		GUILayout.EndHorizontal();
	}
}
