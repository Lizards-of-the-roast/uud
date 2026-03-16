using System.Collections.Generic;

namespace AssetLookupTree.Payloads.UXEventData;

public class ScryResultData : IPayload
{
	public readonly AltAssetReference<ScryResultUXEvent_Data> SryResultUXEventDataRef = new AltAssetReference<ScryResultUXEvent_Data>();

	public IEnumerable<string> GetFilePaths()
	{
		if (SryResultUXEventDataRef != null)
		{
			yield return SryResultUXEventDataRef.RelativePath;
		}
	}
}
