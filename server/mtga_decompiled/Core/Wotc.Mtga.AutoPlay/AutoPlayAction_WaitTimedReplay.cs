using Wotc.Mtga.TimedReplays;

namespace Wotc.Mtga.AutoPlay;

public class AutoPlayAction_WaitTimedReplay : AutoPlayAction
{
	private TimedReplayPlayer _player;

	protected override void OnInitialize(in string[] parameters, int index)
	{
		Timeout = 100000f;
	}

	protected override void OnExecute()
	{
		_player = ComponentGetters.FindComponent<TimedReplayPlayer>();
		if (_player == null)
		{
			Fail("Could not find replay script player.");
			return;
		}
		_player.OnError += PlayerOnError;
		_player.OnDone += PlayerOnDone;
	}

	private void PlayerOnError(string message)
	{
		_player.OnError -= PlayerOnError;
		_player.OnDone -= PlayerOnDone;
		LogAction("Replay failed: " + message);
		Fail("Replay has failed, see log for more details. You may need to re-record the match, see the Confluence page \"Recording Timed Replays\" for more details.");
	}

	private void PlayerOnDone()
	{
		_player.OnError -= PlayerOnError;
		_player.OnDone -= PlayerOnDone;
		Complete("Successfully finished script replay");
	}
}
