public interface IDebugGUIPage
{
	DebugInfoIMGUIOnGui.DebugTab TabType { get; }

	string TabName { get; }

	bool HiddenInTab { get; }

	void Init(DebugInfoIMGUIOnGui gui);

	void Destroy();

	void OnQuit();

	bool OnUpdate();

	void OnGUI();
}
