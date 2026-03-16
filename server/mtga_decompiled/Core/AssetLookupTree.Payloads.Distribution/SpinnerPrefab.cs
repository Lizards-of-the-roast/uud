using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Distribution;

public class SpinnerPrefab : IPayload
{
	public AltAssetReference<SpinnerAnimated> SpinnerRef = new AltAssetReference<SpinnerAnimated>();

	public IEnumerable<string> GetFilePaths()
	{
		if (SpinnerRef != null)
		{
			yield return SpinnerRef.RelativePath;
		}
	}
}
