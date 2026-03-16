using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace AssetLookupTree.Payloads.UI.DuelScene.ManaWheel;

public class WedgeConfig : IPayload
{
	public float Orientation;

	public float Increment;

	public readonly AltAssetReference<GameObject> Prefab = new AltAssetReference<GameObject>();

	[JsonIgnore]
	public string PrefabPath => Prefab?.RelativePath;

	public IEnumerable<string> GetFilePaths()
	{
		if (Prefab != null)
		{
			yield return Prefab.RelativePath;
		}
	}
}
