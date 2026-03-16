using System.Collections.Generic;
using AssetLookupTree.Payloads.Helpers;
using UnityEngine;

namespace AssetLookupTree.Payloads.Card;

public class Sleeve : IPayload
{
	public readonly AltAssetReference<Material> MaterialRef = new AltAssetReference<Material>();

	public readonly ClientOrGreLocKey SleeveLocKey = new ClientOrGreLocKey();

	public IEnumerable<string> GetFilePaths()
	{
		if (MaterialRef != null)
		{
			yield return MaterialRef.RelativePath;
		}
	}
}
