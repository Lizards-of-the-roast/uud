using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Projectile;

public class SustainSFX : IPayload
{
	public SfxData SfxData = new SfxData();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
