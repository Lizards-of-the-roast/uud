using System;
using Newtonsoft.Json;

namespace MTGA.Social;

public class ChallengeMessageConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(ChallengeMessage);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ChallengeMessage challengeMessage = new ChallengeMessage();
		reader.Read();
		if ((string)reader.Value != "Version")
		{
			throw new Exception("ChallengeMessage: First token in parsed JSON was not 'Version': " + (string)reader.Value);
		}
		reader.Read();
		challengeMessage.Version = (string)reader.Value;
		if (challengeMessage.Version != ChallengeMessage.OutgoingVersion)
		{
			throw new Exception("ChallengeMessage: Incoming message version mismatch. Expected: " + ChallengeMessage.OutgoingVersion + ", Actual: " + challengeMessage.Version);
		}
		reader.Read();
		challengeMessage.Method = (ChallengeMessageType)Enum.Parse(typeof(ChallengeMessageType), reader.Value.ToString());
		reader.Read();
		switch (challengeMessage.Method)
		{
		case ChallengeMessageType.IncomingInvite:
			challengeMessage.Params = serializer.Deserialize<ChallengeInviteMessage>(reader);
			break;
		case ChallengeMessageType.RespondToChallenge:
			challengeMessage.Params = serializer.Deserialize<ChallengeInviteResponseMessage>(reader);
			break;
		case ChallengeMessageType.GeneralUpdate:
			challengeMessage.Params = serializer.Deserialize<ChallengeConfigData>(reader);
			break;
		case ChallengeMessageType.PlayerUpdate:
			challengeMessage.Params = serializer.Deserialize<ChallengePlayerUpdateMessage>(reader);
			break;
		default:
			throw new Exception($"ChallengeMessage: Method {challengeMessage.Method} did not match any known ChallengeMessage method");
		}
		reader.Read();
		return challengeMessage;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		ChallengeMessage challengeMessage = value as ChallengeMessage;
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteStartObject();
		writer.WritePropertyName("Version");
		writer.WriteValue(ChallengeMessage.OutgoingVersion);
		writer.WritePropertyName(challengeMessage.Method.ToString());
		serializer.Serialize(writer, challengeMessage.Params);
		writer.WriteEndObject();
	}
}
