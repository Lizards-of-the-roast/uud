using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Emotes;

public class EmoteOverridePayload : IPayload
{
	public string OverrideEmoteId;

	public bool IsTemporary = true;

	public float OverrideDuration = 10f;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
