using System.Collections.Generic;

namespace AssetLookupTree.Payloads.ReplacementEffect;

public class StartSFX : IPayload
{
	public SfxData SfxData;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
