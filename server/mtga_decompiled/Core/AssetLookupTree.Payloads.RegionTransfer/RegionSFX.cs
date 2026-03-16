using System.Collections.Generic;

namespace AssetLookupTree.Payloads.RegionTransfer;

public class RegionSFX : IPayload
{
	public SfxData SfxData = new SfxData();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
