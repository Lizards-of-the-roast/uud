using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Avatar;

public class VoiceSFX : IPayload
{
	public SfxData SfxData = new SfxData();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
