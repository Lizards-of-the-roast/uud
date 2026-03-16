using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Card;

public class EffectDisqualified_VFX : IPayload
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
