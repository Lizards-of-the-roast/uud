using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Browser;

public class CardVFX : IPayload
{
	public readonly AltAssetReference<GameObject> PrefabRef = new AltAssetReference<GameObject>();

	public readonly OffsetData Offset = new OffsetData();

	public IEnumerable<string> GetFilePaths()
	{
		yield return PrefabRef.RelativePath;
	}
}
