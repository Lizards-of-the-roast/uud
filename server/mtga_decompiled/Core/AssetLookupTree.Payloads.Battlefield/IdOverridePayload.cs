using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Battlefield;

public class IdOverridePayload : IPayload
{
	public string Text;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
