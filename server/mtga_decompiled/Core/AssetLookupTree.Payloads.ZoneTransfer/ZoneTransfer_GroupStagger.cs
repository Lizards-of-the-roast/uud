using System.Collections.Generic;

namespace AssetLookupTree.Payloads.ZoneTransfer;

public class ZoneTransfer_GroupStagger : IPayload
{
	public float Stagger = 0.5f;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
