using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.LifeChange;

public abstract class LifeChangeProjectile : IPayload
{
	public float Duration = 0.25f;

	public AltAssetReference<SplineMovementData> SplineRef = new AltAssetReference<SplineMovementData>();

	public Vector3 StartOffset = Vector3.zero;

	public Vector3 EndOffset = Vector3.zero;

	public VfxPrefabData BirthVFX = new VfxPrefabData();

	public VfxPrefabData SustainVFX = new VfxPrefabData();

	public VfxPrefabData HitVFX = new VfxPrefabData();

	public IEnumerable<string> GetFilePaths()
	{
		yield return SplineRef.RelativePath;
		foreach (AltAssetReference<GameObject> allPrefab in BirthVFX.AllPrefabs)
		{
			yield return allPrefab.RelativePath;
		}
		foreach (AltAssetReference<GameObject> allPrefab2 in SustainVFX.AllPrefabs)
		{
			yield return allPrefab2.RelativePath;
		}
		foreach (AltAssetReference<GameObject> allPrefab3 in HitVFX.AllPrefabs)
		{
			yield return allPrefab3.RelativePath;
		}
	}
}
