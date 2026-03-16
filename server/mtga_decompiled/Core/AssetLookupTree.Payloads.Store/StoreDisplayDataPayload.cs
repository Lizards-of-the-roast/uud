using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Store;

public class StoreDisplayDataPayload : IPayload
{
	public readonly AltAssetReference<StoreDisplayData> DisplayDataRef = new AltAssetReference<StoreDisplayData>();

	public IEnumerable<string> GetFilePaths()
	{
		if (DisplayDataRef != null)
		{
			yield return DisplayDataRef.RelativePath;
		}
	}
}
