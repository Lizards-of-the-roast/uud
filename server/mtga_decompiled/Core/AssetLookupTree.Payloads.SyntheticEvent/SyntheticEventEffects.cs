using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.SyntheticEvent;

public class SyntheticEventEffects : IPayload
{
	public readonly List<VfxData> VfxDatas = new List<VfxData>();

	public readonly SfxData SfxData = new SfxData();

	public IEnumerable<string> GetFilePaths()
	{
		foreach (VfxData vfxData in VfxDatas)
		{
			foreach (AltAssetReference<GameObject> allPrefab in vfxData.PrefabData.AllPrefabs)
			{
				yield return allPrefab.RelativePath;
			}
		}
	}
}
