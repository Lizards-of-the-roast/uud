using System.Collections.Generic;

namespace AssetLookupTree.Payloads.ZoneTransfer;

public class IsDestroyEvent : IPayload
{
	public bool IsDestroy = true;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
