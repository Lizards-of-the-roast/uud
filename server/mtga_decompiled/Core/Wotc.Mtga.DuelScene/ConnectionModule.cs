using GreClient.Network;
using UnityEngine;
using Wizards.Arena.TcpConnection;

namespace Wotc.Mtga.DuelScene;

public class ConnectionModule : DebugModule
{
	public interface IDebugConnection
	{
		MatchDoorConnectionState ConnectionState { get; }

		void Connect();

		void Disconnect(TcpConnectionCloseType closeType);
	}

	public class NullConnection : IDebugConnection
	{
		public static readonly IDebugConnection Default = new NullConnection();

		public MatchDoorConnectionState ConnectionState => MatchDoorConnectionState.None;

		public void Connect()
		{
		}

		public void Disconnect(TcpConnectionCloseType closeType)
		{
		}
	}

	public class DebugConnection : IDebugConnection
	{
		private readonly IGREConnection _greConnection;

		private readonly ConnectionConfig _connectionConfig;

		public MatchDoorConnectionState ConnectionState => _greConnection.MatchDoorState;

		public DebugConnection(IGREConnection matchConnection, ConnectionConfig connectionConfig)
		{
			_greConnection = matchConnection;
			_connectionConfig = connectionConfig;
		}

		public void Connect()
		{
			_greConnection.Connect(_connectionConfig);
		}

		public void Disconnect(TcpConnectionCloseType closeType)
		{
			_greConnection.Close(closeType, "DEBUG FORCE CLOSE");
		}
	}

	private readonly IDebugConnection _debugConnection;

	public override string Name => "Connection";

	public override string Description => "Displays your current connection status to a match and enables reconnection if necessary";

	public ConnectionModule(IDebugConnection debugConnection)
	{
		_debugConnection = debugConnection ?? NullConnection.Default;
	}

	public override void Render()
	{
		GUILayout.BeginVertical(GUI.skin.box);
		MatchDoorConnectionState connectionState = _debugConnection.ConnectionState;
		GUILayout.Label($"ConnectionState: {connectionState.ToString()}");
		switch (connectionState)
		{
		case MatchDoorConnectionState.ConnectedToMatchDoor:
		case MatchDoorConnectionState.Playing:
			if (GUILayout.Button("Disconnect from Match [NORMAL CLOSURE]"))
			{
				_debugConnection.Disconnect(TcpConnectionCloseType.NormalClosure);
			}
			if (GUILayout.Button("Disconnect from Match [TIMEOUT]"))
			{
				_debugConnection.Disconnect(TcpConnectionCloseType.InactivityTimeout);
			}
			break;
		case MatchDoorConnectionState.None:
		case MatchDoorConnectionState.Disconnected:
			if (GUILayout.Button("Connect to Match"))
			{
				_debugConnection.Connect();
			}
			break;
		}
		GUILayout.Space(3f);
		GUILayout.EndVertical();
	}
}
