using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AssetLookupTree.Payloads.Avatar;

public class HighlightVFX : IPayload
{
	public VfxData VfxData = new VfxData();

	public IEnumerable<string> GetFilePaths()
	{
		return VfxData.PrefabData.AllPrefabs.Select((AltAssetReference<GameObject> p) => p.RelativePath);
	}
}
