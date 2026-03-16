using System.Collections.Generic;

namespace AssetLookupTree.Payloads.UXEventData;

public class CoinFlipData : IPayload
{
	public readonly AltAssetReference<ChooseRandomUXEvent_CoinFlip_Data> CoinFlipUXEventDataRef = new AltAssetReference<ChooseRandomUXEvent_CoinFlip_Data>();

	public IEnumerable<string> GetFilePaths()
	{
		if (CoinFlipUXEventDataRef != null)
		{
			yield return CoinFlipUXEventDataRef.RelativePath;
		}
	}
}
