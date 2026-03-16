using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Card;

public class PrePropertyUpdateVFX : ILayeredPayload, IPayload, IAnchoredVfxPayload
{
	public List<VfxData> VfxDatas = new List<VfxData>();

	public HashSet<string> Layers { get; } = new HashSet<string>();

	public AnchorPointType AnchorPointType { get; set; }

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
