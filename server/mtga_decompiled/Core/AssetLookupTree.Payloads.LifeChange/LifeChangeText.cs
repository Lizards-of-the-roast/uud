using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.LifeChange;

public abstract class LifeChangeText : IPayload
{
	public AltAssetReference<GameObject> PrefabRef = new AltAssetReference<GameObject>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return PrefabRef.RelativePath;
	}
}
