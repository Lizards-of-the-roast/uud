using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Card;

public class WontUntapVFX : IPayload
{
	public readonly AltAssetReference<GameObject> SpawnVfxReference = new AltAssetReference<GameObject>();

	public readonly AltAssetReference<GameObject> SustainVfxReference = new AltAssetReference<GameObject>();

	public readonly AltAssetReference<GameObject> DespawnVfxReference = new AltAssetReference<GameObject>();

	public IEnumerable<string> GetFilePaths()
	{
		if (!string.IsNullOrWhiteSpace(SpawnVfxReference.RelativePath))
		{
			yield return SpawnVfxReference.RelativePath;
		}
		if (!string.IsNullOrWhiteSpace(SustainVfxReference.RelativePath))
		{
			yield return SustainVfxReference.RelativePath;
		}
		if (!string.IsNullOrWhiteSpace(DespawnVfxReference.RelativePath))
		{
			yield return DespawnVfxReference.RelativePath;
		}
	}
}
