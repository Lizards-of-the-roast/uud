using System;
using Core.Shared.Code.Connection;
using Wizards.Mtga;

public class ConnectionGUI : IDebugGUIPage
{
	private DebugInfoIMGUIOnGui _GUI;

	public DebugInfoIMGUIOnGui.DebugTab TabType => DebugInfoIMGUIOnGui.DebugTab.Connection;

	public string TabName => "Connection";

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
		_GUI.ShowLabel("------- Security -------");
		bool shouldValidateServerCert = MDNPlayerPrefs.ShouldValidateServerCert;
		bool flag = _GUI.ShowToggle(shouldValidateServerCert, "GRE/FD Validate Server Cert");
		if (flag != shouldValidateServerCert)
		{
			MDNPlayerPrefs.ShouldValidateServerCert = flag;
		}
		_GUI.ShowLabel("------- Input Idle Timeout -------");
		FrontDoorConnectionManager frontDoorConnectionManager = Pantry.Get<FrontDoorConnectionManager>();
		int lastInputTime = frontDoorConnectionManager.LastInputTime;
		int num = Environment.TickCount - lastInputTime;
		_GUI.ShowLabel($"input Timestamp: ${lastInputTime} ms since application start");
		_GUI.ShowLabel($"Time since last input: ${num} ms");
		frontDoorConnectionManager.IdleTimerActive = _GUI.ShowToggle(frontDoorConnectionManager.IdleTimerActive, "Check For Idle Timeouts");
		frontDoorConnectionManager.IdleTimeoutSec = _GUI.ShowInputField("Idle Seconds:", frontDoorConnectionManager.IdleTimeoutSec);
		_GUI.ShowLabel("------- TCP Inactivity -------");
		int inactivityTimeoutMs = MDNPlayerPrefs.InactivityTimeoutMs;
		int num2 = _GUI.ShowInputField("InactivityTimeoutMs (requires restart to take effect)", inactivityTimeoutMs);
		if (num2 != inactivityTimeoutMs)
		{
			MDNPlayerPrefs.InactivityTimeoutMs = num2;
		}
	}
}
