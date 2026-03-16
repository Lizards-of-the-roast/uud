using UnityEngine;
using Wizards.Mtga.Platforms;

public class AudioWatcher : MonoBehaviour
{
	private bool _enabled;

	public bool Toggle;

	private IMGUIDrawer _IMGUIDrawer;

	private void Awake()
	{
		_IMGUIDrawer = base.gameObject.AddComponent<IMGUIDrawer>();
		_IMGUIDrawer.Init(1, "DuelScene Debug UI", DrawUI, 300, 600, PlatformContext.GetIMGUIScale());
		_IMGUIDrawer.enabled = false;
	}

	private void Update()
	{
		if (Toggle)
		{
			_enabled = !_enabled;
			_IMGUIDrawer.enabled = _enabled;
			Toggle = false;
		}
	}

	public void DrawUI(int window)
	{
		AudioManager instance = AudioManager.Instance;
		if (!(instance == null))
		{
			AudioHistory eventHistory = instance.EventHistory;
			if (eventHistory != null)
			{
				GUI.DragWindow(new Rect(0f, 0f, 10000f, 20f));
				eventHistory.DoOnGui();
			}
		}
	}
}
