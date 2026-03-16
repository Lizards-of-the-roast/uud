using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree;

public static class VfxPrefabDataExtensions
{
	public static IEnumerable<string> AllPrefabPaths(this VfxPrefabData vfxPrefabData)
	{
		if (vfxPrefabData == null)
		{
			yield break;
		}
		foreach (AltAssetReference<GameObject> allPrefab in vfxPrefabData.AllPrefabs)
		{
			yield return allPrefab.RelativePath;
		}
	}
}
