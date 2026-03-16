using Newtonsoft.Json;

namespace MTGA.Social;

[JsonConverter(typeof(ChallengeMessageConverter))]
public class ChallengeMessage
{
	public static string OutgoingVersion = "1.0.0";

	public string Version;

	public ChallengeMessageType Method;

	public object Params;
}
