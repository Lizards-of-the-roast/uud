using System.Collections.Generic;

namespace AssetLookupTree.Payloads.General;

public abstract class LocKey : IPayload
{
	public string Key;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
