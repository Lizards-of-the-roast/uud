using System.IO;
using Wotc.Mtga.TimedReplays;

namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_TimedReplay : AutoPlayAction
{
	private string _replayPath;

	protected override void OnInitialize(in string[] parameters, int index)
	{
		string text = AutoPlayAction.FromParameter(in parameters, index + 1);
		_replayPath = Path.Combine(AutoPlayManager.GetConfigRoot, text ?? "");
	}

	protected override void OnExecute()
	{
		TimedReplayPlayer.NextReplay = _replayPath;
		Complete("Loaded replay " + _replayPath);
	}
}
