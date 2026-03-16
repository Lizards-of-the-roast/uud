using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Resolution;

public abstract class VFX_Base : ILayeredPayload, IPayload
{
	public float Duration = 0.25f;

	public List<VfxData> VfxDatas = new List<VfxData>();

	public HashSet<string> Layers { get; } = new HashSet<string>();

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
