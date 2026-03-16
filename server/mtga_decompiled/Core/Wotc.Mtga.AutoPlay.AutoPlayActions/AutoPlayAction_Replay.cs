using System.IO;
using Wotc.Mtga.Replays;

namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_Replay : AutoPlayAction
{
	private string _replayName;

	private string _replayPath;

	private PAPA _papa;

	protected override void OnInitialize(in string[] parameters, int index)
	{
		_replayName = AutoPlayAction.FromParameter(in parameters, index + 1);
		_replayPath = Path.Combine(AutoPlayManager.GetConfigRoot, "Replays", _replayName + ".replay");
	}

	protected override void OnExecute()
	{
		PAPA pAPA = ComponentGetters.FindComponent<PAPA>();
		ReplayUtilities.StartReplay(new ReplayInfo(_replayPath, ReplayFormat.Text), pAPA, pAPA.MatchManager, base._cardDatabase, base._cardViewBuilder, base._cardMaterialBuilder, OnLoad, "DuelSceneDebugLauncher");
		void OnLoad()
		{
			Complete("Playing timed replay " + _replayPath);
		}
	}
}
