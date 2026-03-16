using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.GameState.Designation;

public class AddedVfx : IPayload
{
	public VfxData VfxData = new VfxData();

	public IEnumerable<string> GetFilePaths()
	{
		foreach (AltAssetReference<GameObject> allPrefab in VfxData.PrefabData.AllPrefabs)
		{
			yield return allPrefab.RelativePath;
		}
	}
}
