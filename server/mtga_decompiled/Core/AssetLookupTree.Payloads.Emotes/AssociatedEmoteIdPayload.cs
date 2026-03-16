using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Emotes;

public class AssociatedEmoteIdPayload : IPayload
{
	public List<string> AssociatedIds = new List<string>();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
