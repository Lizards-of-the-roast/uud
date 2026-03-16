using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Emotes;

public class EmoteSFX : IPayload
{
	public SfxData SfxData = new SfxData();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
