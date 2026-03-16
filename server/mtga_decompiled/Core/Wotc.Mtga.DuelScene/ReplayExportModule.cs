using GreClient.History;
using UnityEngine;
using Wotc.Mtga.Replays;

namespace Wotc.Mtga.DuelScene;

public class ReplayExportModule : DebugModule
{
	private readonly MessageHistory _messageHistory;

	private string _replayName = string.Empty;

	public override string Name => "Replay Export";

	public override string Description => "Controls to save a replay of the current match";

	public ReplayExportModule(MessageHistory messageHistory)
	{
		_messageHistory = messageHistory ?? NullHistory.Default;
	}

	public override void Render()
	{
		ExportReplayControls();
	}

	private void ExportReplayControls()
	{
		bool flag = false;
		GUILayout.Label("Save Replay:");
		GUILayout.BeginHorizontal();
		_replayName = GUILayout.TextField(_replayName, GUILayout.ExpandWidth(expand: true));
		if (GUILayout.Button("Export", GUILayout.Width(100f)))
		{
			flag = true;
		}
		Event current = Event.current;
		if (current != null && current.keyCode == KeyCode.Return)
		{
			flag = true;
		}
		GUILayout.EndHorizontal();
		if (flag && !string.IsNullOrEmpty(_replayName))
		{
			ReplayUtilities.SaveReplay(_replayName, ReplayUtilities.GetReplayFolder(), _messageHistory.Messages, openInExplorer: true);
		}
		if (GUILayout.Button("View Replay Confluence"))
		{
			Application.OpenURL("https://confluence.wizards.com/display/MDN/Client+Side+Replays");
		}
	}
}
