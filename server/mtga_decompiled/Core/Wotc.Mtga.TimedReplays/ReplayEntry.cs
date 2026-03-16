using Google.Protobuf;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.TimedReplays;

public class ReplayEntry
{
	public readonly long Timestamp;

	public readonly ClientToGREMessage ClientToGRE;

	public readonly GREToClientMessage GREToClient;

	public ReplayEntry(ClientToGREMessage clientToGRE, long timestamp)
	{
		GREToClient = null;
		ClientToGRE = clientToGRE;
		Timestamp = timestamp;
	}

	public ReplayEntry(GREToClientMessage greToClient, long timestamp)
	{
		GREToClient = greToClient;
		ClientToGRE = null;
		Timestamp = timestamp;
	}

	public string Serialize()
	{
		if (ClientToGRE != null)
		{
			return $"OUT-{Timestamp}:{ClientToGRE}";
		}
		return $"IN-{Timestamp}:{GREToClient}";
	}

	public static ReplayEntry Deserialize(string line)
	{
		int num = line.IndexOf('-');
		bool num2 = line.Substring(0, num) == "OUT";
		int num3 = line.IndexOf(':');
		long timestamp = long.Parse(line.Substring(num + 1, num3 - (num + 1)));
		string json = line.Substring(num3 + 1);
		JsonParser jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(ignoreUnknownFields: true));
		if (num2)
		{
			return new ReplayEntry(jsonParser.Parse<ClientToGREMessage>(json), timestamp);
		}
		return new ReplayEntry(jsonParser.Parse<GREToClientMessage>(json), timestamp);
	}
}
