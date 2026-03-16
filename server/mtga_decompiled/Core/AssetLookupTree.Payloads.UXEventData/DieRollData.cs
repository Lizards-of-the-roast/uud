using System.Collections.Generic;
using Wotc.Mtga.Duel;

namespace AssetLookupTree.Payloads.UXEventData;

public class DieRollData : IPayload
{
	public readonly AltAssetReference<DieRollUxEventData> DieRollUxEventDataRef = new AltAssetReference<DieRollUxEventData>();

	public IEnumerable<string> GetFilePaths()
	{
		if (DieRollUxEventDataRef != null)
		{
			yield return DieRollUxEventDataRef.RelativePath;
		}
	}
}
