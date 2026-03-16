using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Emotes;

public class EmoteLocKey : IPayload
{
	public string PreviewLocKey;

	public string FullLocKey;

	public bool UseFullLocInStore = true;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
