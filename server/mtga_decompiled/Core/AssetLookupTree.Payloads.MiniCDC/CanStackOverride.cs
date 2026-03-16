using System.Collections.Generic;

namespace AssetLookupTree.Payloads.MiniCDC;

public class CanStackOverride : IPayload
{
	public bool StackMiniCDC;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
