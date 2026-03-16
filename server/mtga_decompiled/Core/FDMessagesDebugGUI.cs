using UnityEngine;

internal class FDMessagesDebugGUI
{
	private static GUIStyle _textInputStyleCache;

	private string _payloadJson;

	private static GUIStyle TextInputStyle => _textInputStyleCache ?? (_textInputStyleCache = new GUIStyle(GUI.skin.GetStyle("TextField")));

	public void DoOnGUI()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("FrontDoor push notification message payload JSON", GUILayout.Width(250f));
		_payloadJson = GUILayout.TextField(_payloadJson, TextInputStyle);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Send FrontDoor push notification spoof", GUILayout.Width(250f)))
		{
			FrontDoorDebugSingleton.SpoofIncomingFrontDoorMessage(_payloadJson);
		}
		GUILayout.EndHorizontal();
	}
}
