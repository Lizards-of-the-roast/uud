using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Card;

public class MaterialOverride : IPayload
{
	public readonly AltAssetReference<Material> MaterialRef = new AltAssetReference<Material>();

	public IEnumerable<string> GetFilePaths()
	{
		if (MaterialRef != null)
		{
			yield return MaterialRef.RelativePath;
		}
	}
}
