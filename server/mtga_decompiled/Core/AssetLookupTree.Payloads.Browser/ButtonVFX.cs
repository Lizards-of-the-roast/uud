using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Browser;

public class ButtonVFX : IPayload
{
	public readonly VfxData VfxData = new VfxData();

	public string ButtonName;

	public IEnumerable<string> GetFilePaths()
	{
		foreach (AltAssetReference<GameObject> allPrefab in VfxData.PrefabData.AllPrefabs)
		{
			yield return allPrefab.RelativePath;
		}
	}
}
