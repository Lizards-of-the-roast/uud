using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Resource;

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
