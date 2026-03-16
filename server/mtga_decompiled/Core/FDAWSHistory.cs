using System.Collections.Concurrent;
using System.Collections.Generic;
using Core.Shared.Code.DebugTools;
using GreClient.Network;
using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;

public class FDAWSHistory : IFDHistory
{
	private readonly MatchManager _matchManager;

	private bool _newestFirst = true;

	private IConnectionHistory _targetConnection;

	private IConnectionHistory _previousMatchHistory;

	private int _lastHistorySize;

	private List<HistoryEntry> _historyEntries = new List<HistoryEntry>();

	public bool NewestFirst
	{
		get
		{
			return _newestFirst;
		}
		set
		{
			_newestFirst = value;
		}
	}

	public List<HistoryEntry> HistoryEntries
	{
		get
		{
			_historyEntries.Clear();
			if (_targetConnection != null)
			{
				_historyEntries.AddRange(_targetConnection.History);
			}
			return _historyEntries;
		}
	}

	public FDAWSHistory(MatchManager matchManager)
	{
		_matchManager = matchManager;
	}

	public void Clear()
	{
		_lastHistorySize = 0;
	}

	public void ClearHistory()
	{
		_targetConnection.History = new ConcurrentQueue<HistoryEntry>();
	}

	public bool HasConnection()
	{
		return _targetConnection != null;
	}

	public bool HasPreviousMatchHistoryConnection()
	{
		return _previousMatchHistory != null;
	}

	private void MoveMatchHistoryToPreviousMatchHistory(MatchDoorConnectionState state, IConnectionHistory history)
	{
		if (state != MatchDoorConnectionState.MatchCompleted && state != MatchDoorConnectionState.Disconnected && _previousMatchHistory != history)
		{
			_previousMatchHistory = history;
		}
	}

	public bool DoUpdate(string connectionCategory)
	{
		IConnectionHistory connectionHistory = null;
		switch (connectionCategory)
		{
		case "FrontDoor":
			connectionHistory = Pantry.Get<IFrontDoorConnectionServiceWrapper>()?.FDCAWS?.History;
			break;
		case "MatchDoor":
		{
			if (_matchManager == null || _matchManager.GreConnection == null)
			{
				break;
			}
			IConnectionHistory history = _matchManager.GreConnection.History;
			if (history != _previousMatchHistory)
			{
				_matchManager.GreConnection.MatchConnectionStateChanged += delegate(MatchDoorConnectionState oldState, MatchDoorConnectionState newState)
				{
					MoveMatchHistoryToPreviousMatchHistory(newState, history);
				};
				connectionHistory = history;
			}
			break;
		}
		case "MatchDoor(Prev)":
			connectionHistory = _previousMatchHistory;
			break;
		}
		if (_targetConnection != connectionHistory && connectionHistory != null)
		{
			_targetConnection = connectionHistory;
			_lastHistorySize = -1;
		}
		if (_targetConnection == null)
		{
			return false;
		}
		if (_targetConnection.History.Count != _lastHistorySize)
		{
			_lastHistorySize = _targetConnection.History.Count;
			return true;
		}
		return false;
	}
}
