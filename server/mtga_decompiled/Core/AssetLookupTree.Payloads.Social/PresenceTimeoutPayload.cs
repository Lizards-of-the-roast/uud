using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Social;

public class PresenceTimeoutPayload : IPayload
{
	public int PresenceTimeout = 360;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
