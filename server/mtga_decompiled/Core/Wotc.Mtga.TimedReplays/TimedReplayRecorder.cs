using Wotc.Mtga.Replays;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.TimedReplays;

public class TimedReplayRecorder
{
	private readonly Matchmaking _matchmaker;

	private ReplayWriter _activeReplay;

	private MatchManager _activeMatchManager;

	public TimedReplayRecorder(Matchmaking matchmaking)
	{
		_matchmaker = matchmaking;
		_matchmaker.MatchManagerInitialized += StartMatch;
	}

	public void StartMatch(MatchManager match)
	{
		if (MDNPlayerPrefs.SaveDSReplays)
		{
			if (_activeMatchManager != null || _activeReplay != null)
			{
				CompleteMatch();
			}
			_activeMatchManager = match;
			_activeMatchManager.ConnectRespReceived += ActiveMatchManagerOnConnectRespReceived;
		}
	}

	private void ActiveMatchManagerOnConnectRespReceived(GREToClientMessage connectMessage)
	{
		_activeReplay = ReplayWriter.CreateFromPath(ReplayUtilities.GetNewTimedReplayPath(), _activeMatchManager.LocalPlayerInfo, _activeMatchManager.OpponentInfo, BattlefieldUtil.BattlefieldId);
		_activeMatchManager.MessageSent += _activeReplay.WriteMessage;
		_activeMatchManager.MessageReceived += _activeReplay.WriteMessage;
		_activeMatchManager.MatchCompleted += ActiveMatchManagerOnMatchCompleted;
	}

	private void ActiveMatchManagerOnMatchCompleted()
	{
		CompleteMatch();
	}

	private void CompleteMatch()
	{
		if (_activeMatchManager != null)
		{
			_activeMatchManager.ConnectRespReceived -= ActiveMatchManagerOnConnectRespReceived;
			_activeMatchManager.MatchCompleted -= ActiveMatchManagerOnMatchCompleted;
			if (_activeReplay != null)
			{
				_activeMatchManager.MessageSent -= _activeReplay.WriteMessage;
				_activeMatchManager.MessageReceived -= _activeReplay.WriteMessage;
			}
			_activeMatchManager = null;
		}
		_activeReplay?.Dispose();
		_activeReplay = null;
	}

	public void CleanUp()
	{
		CompleteMatch();
		if (_matchmaker != null)
		{
			_matchmaker.MatchManagerInitialized -= StartMatch;
		}
	}
}
