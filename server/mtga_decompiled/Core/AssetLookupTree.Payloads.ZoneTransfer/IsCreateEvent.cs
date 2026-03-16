using System.Collections.Generic;

namespace AssetLookupTree.Payloads.ZoneTransfer;

public class IsCreateEvent : IPayload
{
	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
