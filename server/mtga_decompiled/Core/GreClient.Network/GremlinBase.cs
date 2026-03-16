using System;
using Core.Shared.Code.DebugTools;
using Wizards.Arena.TcpConnection;
using Wotc.Mtgo.Gre.External.Messaging;

namespace GreClient.Network;

internal abstract class GremlinBase : IGREConnection
{
	public const string REPLAY_MATCH_ID = "REPLAY_MATCH_ID";

	public const string REPLAY_URI = "REPLAY_URI";

	private MatchDoorConnectionState _lastState;

	private MatchDoorConnectionState _state;

	private object _lock = new object();

	protected MatchDoorConnectionState ThreadsafeState
	{
		get
		{
			lock (_lock)
			{
				return _state;
			}
		}
		set
		{
			lock (_lock)
			{
				_state = value;
			}
		}
	}

	public string MatchId => "REPLAY_MATCH_ID";

	public string McFabricUri => "REPLAY_URI";

	public MatchDoorConnectionState MatchDoorState => ThreadsafeState;

	public IConnectionHistory History => null;

	public event Action<MatchDoorConnectionState, MatchDoorConnectionState> MatchConnectionStateChanged;

	public event Action<GREToClientMessage> MessageReceived;

	public event Action<string, string, string> ServerExceptionReceived;

	public event System.Action MatchCompleted;

	public event Action<(string reason, string details)> MatchFailed;

	public event Action<string> ConnectionLost;

	public event Action<string> ConnectionFailed;

	protected void InvokeMatchConnectionStateChanged(MatchDoorConnectionState oldState, MatchDoorConnectionState newState)
	{
		this.MatchConnectionStateChanged?.Invoke(oldState, newState);
	}

	protected void InvokeMessageReceived(GREToClientMessage msg)
	{
		this.MessageReceived?.Invoke(msg);
	}

	protected void InvokeMatchCompleted()
	{
		this.MatchCompleted?.Invoke();
	}

	public void Connect(ConnectionConfig greConnectionConfig)
	{
		ThreadsafeState = MatchDoorConnectionState.ConnectedToMatchDoor;
	}

	public void ConnectToGRE(string mcFabricUri, string matchId)
	{
		ThreadsafeState = MatchDoorConnectionState.Playing;
	}

	public void Close(TcpConnectionCloseType status, string reason)
	{
		ThreadsafeState = MatchDoorConnectionState.Disconnected;
	}

	public void CreateMatch(ClientToMatchServiceMessage createMatchMsg)
	{
		ThreadsafeState = MatchDoorConnectionState.ConnectedToMatchDoor;
	}

	protected bool TryUpdate()
	{
		if (this.MessageReceived == null || ThreadsafeState == MatchDoorConnectionState.None)
		{
			return false;
		}
		MatchDoorConnectionState threadsafeState = ThreadsafeState;
		if (threadsafeState != _lastState)
		{
			InvokeMatchConnectionStateChanged(_lastState, threadsafeState);
			_lastState = threadsafeState;
		}
		return true;
	}

	public abstract void ProcessMessages();

	public abstract void SendMessage(ClientToGREMessage msg);
}
