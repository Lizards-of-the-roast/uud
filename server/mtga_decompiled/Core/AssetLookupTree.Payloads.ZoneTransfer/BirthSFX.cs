using System.Collections.Generic;

namespace AssetLookupTree.Payloads.ZoneTransfer;

public class BirthSFX : IPayload
{
	public SfxData SfxData = new SfxData();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
