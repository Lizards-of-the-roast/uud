namespace Test.Scenes.NetworkMessagingHarness;

public class ByteArraySize : IByteSizer<byte[]>
{
	public int SizeInBytes(byte[] value)
	{
		if (value == null)
		{
			return 0;
		}
		return value.Length;
	}
}
