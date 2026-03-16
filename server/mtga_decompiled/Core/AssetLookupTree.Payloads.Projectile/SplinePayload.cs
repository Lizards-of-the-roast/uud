using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Projectile;

public class SplinePayload : IPayload
{
	public AltAssetReference<SplineMovementData> SplineDataRef = new AltAssetReference<SplineMovementData>();

	public IEnumerable<string> GetFilePaths()
	{
		if (SplineDataRef != null)
		{
			yield return SplineDataRef.RelativePath;
		}
	}
}
