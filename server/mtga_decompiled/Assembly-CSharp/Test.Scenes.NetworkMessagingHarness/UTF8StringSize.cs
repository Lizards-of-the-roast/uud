using System.Text;

namespace Test.Scenes.NetworkMessagingHarness;

public class UTF8StringSize : IByteSizer<string>
{
	public int SizeInBytes(string value)
	{
		if (value != null)
		{
			return Encoding.UTF8.GetByteCount(value);
		}
		return 0;
	}
}
