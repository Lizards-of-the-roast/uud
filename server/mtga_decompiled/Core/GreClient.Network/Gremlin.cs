using System.Collections.Generic;
using Google.Protobuf;
using Wotc.Mtga.Replays;
using Wotc.Mtgo.Gre.External.Messaging;

namespace GreClient.Network;

internal class Gremlin : GremlinBase
{
	private readonly List<IMessage> _messages;

	public Gremlin(string path, ReplayFormat replayFormat)
	{
		_messages = ReplayUtilities.LoadRecordedMessages(path, replayFormat);
	}

	public override void ProcessMessages()
	{
		if (TryUpdate() && _messages.Count > 0)
		{
			if (_messages[0] is GREToClientMessage msg)
			{
				InvokeMessageReceived(msg);
			}
			_messages.RemoveAt(0);
		}
	}

	public override void SendMessage(ClientToGREMessage msg)
	{
		while (_messages.Count > 0 && _messages[0] is ClientToGREMessage)
		{
			_messages.RemoveAt(0);
		}
	}
}
