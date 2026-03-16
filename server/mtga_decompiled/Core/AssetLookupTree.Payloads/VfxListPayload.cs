using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads;

public abstract class VfxListPayload : IPayload
{
	public List<VfxData> VfxDatas = new List<VfxData>();

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
