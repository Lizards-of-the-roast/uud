using System.Collections.Generic;

namespace AssetLookupTree.Payloads;

public abstract class LocPayload : IPayload
{
	public string LocKey;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
