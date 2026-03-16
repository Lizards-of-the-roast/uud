using System.Collections.Generic;

namespace AssetLookupTree.Payloads.ZoneTransfer;

public class DelayPayload : IPayload
{
	public float EndDelay;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
