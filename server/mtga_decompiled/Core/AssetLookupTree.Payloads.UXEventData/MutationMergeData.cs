using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.UXEventData;

public class MutationMergeData : IPayload
{
	public AltAssetReference<SplineMovementData> OverSplineRef = new AltAssetReference<SplineMovementData>();

	public Vector3 OverPositionOffset = Vector3.up * 0.1f;

	public VfxData OverSplineBirthVfxData = new VfxData();

	public SfxData OverSplineBirthSfxData = new SfxData();

	public VfxData OverSplineTrailVfxData = new VfxData();

	public VfxData OverSplineHitVfxData = new VfxData();

	public SfxData OverSplineHitSfxData = new SfxData();

	public AltAssetReference<SplineMovementData> UnderSplineRef = new AltAssetReference<SplineMovementData>();

	public Vector3 UnderPositionOffset = Vector3.down * 0.2f;

	public Vector3 UnderParentPositionOffset = new Vector3(0f, 2f, -1f);

	public Vector3 UnderParentRotationOffset = new Vector3(30f, 0f, 0f);

	public Vector3 UnderSiblingPositionOffset = Vector3.down * 0.1f;

	public Vector3 UnderSiblingRotationOffset = Vector3.zero;

	public VfxData UnderSplineBirthVfxData = new VfxData();

	public SfxData UnderSplineBirthSfxData = new SfxData();

	public VfxData UnderSplineTrailVfxData = new VfxData();

	public VfxData UnderSplineHitVfxData = new VfxData();

	public SfxData UnderSplineHitSfxData = new SfxData();

	public IEnumerable<string> GetFilePaths()
	{
		yield return OverSplineRef.RelativePath;
		foreach (AltAssetReference<GameObject> allPrefab in OverSplineBirthVfxData.PrefabData.AllPrefabs)
		{
			yield return allPrefab.RelativePath;
		}
		foreach (AltAssetReference<GameObject> allPrefab2 in OverSplineTrailVfxData.PrefabData.AllPrefabs)
		{
			yield return allPrefab2.RelativePath;
		}
		foreach (AltAssetReference<GameObject> allPrefab3 in OverSplineHitVfxData.PrefabData.AllPrefabs)
		{
			yield return allPrefab3.RelativePath;
		}
		yield return UnderSplineRef.RelativePath;
		foreach (AltAssetReference<GameObject> allPrefab4 in UnderSplineBirthVfxData.PrefabData.AllPrefabs)
		{
			yield return allPrefab4.RelativePath;
		}
		foreach (AltAssetReference<GameObject> allPrefab5 in UnderSplineTrailVfxData.PrefabData.AllPrefabs)
		{
			yield return allPrefab5.RelativePath;
		}
		foreach (AltAssetReference<GameObject> allPrefab6 in UnderSplineHitVfxData.PrefabData.AllPrefabs)
		{
			yield return allPrefab6.RelativePath;
		}
	}
}
