using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Event;

public class VersusBackgroundPayload : IPayload
{
	public List<AltAssetReference<Sprite>> References = new List<AltAssetReference<Sprite>>();

	public IEnumerable<string> GetFilePaths()
	{
		foreach (AltAssetReference<Sprite> reference in References)
		{
			yield return reference.RelativePath;
		}
	}
}
