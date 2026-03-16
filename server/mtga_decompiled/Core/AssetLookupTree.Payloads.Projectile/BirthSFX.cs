using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Projectile;

public class BirthSFX : IPayload
{
	public SfxData SfxData = new SfxData();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
