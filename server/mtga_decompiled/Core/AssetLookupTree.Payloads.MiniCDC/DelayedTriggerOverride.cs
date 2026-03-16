using System.Collections.Generic;

namespace AssetLookupTree.Payloads.MiniCDC;

public class DelayedTriggerOverride : IPayload
{
	public bool SuppressMiniCDC;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
