using GreClient.Network;
using UnityEngine;

public class DebugRoomClipboard : MonoBehaviour
{
	private MatchManager _matchManager;

	public void Init(MatchManager matchManager)
	{
		_matchManager = matchManager;
		base.gameObject.SetActive(value: true);
		base.transform.SetParent(null);
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void OnGUI()
	{
		string text = _matchManager?.GreConnection?.McFabricUri;
		if (text != null)
		{
			GUI.TextField(new Rect(Screen.width / 2 - 400, 0f, 800f, 20f), text);
			if (GUI.Button(new Rect(Screen.width / 2 - 80, 20f, 160f, 80f), "Copy To Clipboard"))
			{
				GUIUtility.systemCopyBuffer = text;
			}
			if (_matchManager.GreConnection.MatchDoorState == MatchDoorConnectionState.Playing)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}
}
