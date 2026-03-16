using System.Collections.Generic;

namespace AssetLookupTree.Payloads.ReplacementEffect;

public class EndSFX : IPayload
{
	public SfxData SfxData;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
