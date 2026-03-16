using Wizards.Arena.Enums.System;
using Wizards.Arena.Protocol;

namespace Test.Scenes.NetworkMessagingHarness;

public class ResponsePayloadSize : IByteSizer<Response>
{
	public int SizeInBytes(Response resp)
	{
		if (resp == null)
		{
			return 0;
		}
		if (resp.Format == Wizards.Arena.Enums.System.SerializationFormat.Protobuf)
		{
			return resp.ProtobufPayload.CalculateSize();
		}
		return resp.JsonPayload.Length;
	}
}
