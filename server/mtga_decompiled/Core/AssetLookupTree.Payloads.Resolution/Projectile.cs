using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Resolution;

public class Projectile : IPayload
{
	public float Duration = 0.25f;

	public AltAssetReference<SplineMovementData> SplineRef = new AltAssetReference<SplineMovementData>();

	public SfxData BirthSfx = new SfxData();

	public VfxData BirthVfx = new VfxData();

	public SfxData SustainSfx = new SfxData();

	public VfxData SustainVfx = new VfxData();

	public SfxData HitSfx = new SfxData();

	public VfxData HitVfx = new VfxData();

	public IEnumerable<string> GetFilePaths()
	{
		yield return SplineRef.RelativePath;
		foreach (AltAssetReference<GameObject> allPrefab in BirthVfx.PrefabData.AllPrefabs)
		{
			yield return allPrefab.RelativePath;
		}
		foreach (AltAssetReference<GameObject> allPrefab2 in SustainVfx.PrefabData.AllPrefabs)
		{
			yield return allPrefab2.RelativePath;
		}
		foreach (AltAssetReference<GameObject> allPrefab3 in HitVfx.PrefabData.AllPrefabs)
		{
			yield return allPrefab3.RelativePath;
		}
	}
}
