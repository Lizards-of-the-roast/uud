using Google.Protobuf;

namespace Test.Scenes.NetworkMessagingHarness;

public class ProtobufMessageSize : IByteSizer<IMessage>
{
	public int SizeInBytes(IMessage value)
	{
		return value?.CalculateSize() ?? 0;
	}
}
