using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Card;

public class FontMaterialSettings : IPayload
{
	public readonly AltAssetReference<Material> MaterialReference = new AltAssetReference<Material>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return MaterialReference.RelativePath;
	}
}
