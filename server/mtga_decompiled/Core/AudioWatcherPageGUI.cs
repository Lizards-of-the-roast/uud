public class AudioWatcherPageGUI : IDebugGUIPage
{
	private DebugInfoIMGUIOnGui _GUI;

	private AudioWatcher _audioWindow;

	public DebugInfoIMGUIOnGui.DebugTab TabType => DebugInfoIMGUIOnGui.DebugTab.AudioWatcher;

	public string TabName => "Audio Watcher";

	public bool HiddenInTab => false;

	public AudioWatcherPageGUI(AudioWatcher audioWindow)
	{
		_audioWindow = audioWindow;
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
		if (_GUI.ShowDebugButton("Open Window", 500f))
		{
			_audioWindow.Toggle = true;
		}
	}
}
