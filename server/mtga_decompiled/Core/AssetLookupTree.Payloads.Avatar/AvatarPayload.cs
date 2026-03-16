using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Avatar;

public abstract class AvatarPayload : IPayload
{
	public readonly AltAssetReference<GameObject> Prefab = new AltAssetReference<GameObject>();

	public IEnumerable<string> GetFilePaths()
	{
		if (Prefab != null)
		{
			yield return Prefab.RelativePath;
		}
	}
}
