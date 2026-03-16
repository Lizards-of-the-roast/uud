using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Combat;

public class Spline : IPayload
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
