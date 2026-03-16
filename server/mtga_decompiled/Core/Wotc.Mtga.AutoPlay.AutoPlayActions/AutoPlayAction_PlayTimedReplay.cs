using System.IO;
using Wotc.Mtga.Replays;

namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_PlayTimedReplay : AutoPlayAction
{
	private string _replayPath;

	protected override void OnInitialize(in string[] parameters, int index)
	{
		string text = AutoPlayAction.FromParameter(in parameters, index + 1);
		_replayPath = Path.Combine(AutoPlayManager.GetConfigRoot, text ?? "");
	}

	protected override void OnExecute()
	{
		PAPA pAPA = ComponentGetters.FindComponent<PAPA>();
		ReplayUtilities.StartReplay(new ReplayInfo(_replayPath, ReplayFormat.TimedReplay), pAPA, pAPA.MatchManager, base._cardDatabase, base._cardViewBuilder, base._cardMaterialBuilder, OnLoad);
		void OnLoad()
		{
			Complete("Playing timed replay " + _replayPath);
		}
	}
}
