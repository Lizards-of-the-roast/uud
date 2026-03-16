using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace AssetLookupTree.Payloads.Prefab;

public abstract class PrefabPayload<TPrefab> : IPayload where TPrefab : Object
{
	public AltAssetReference<TPrefab> Prefab = new AltAssetReference<TPrefab>();

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
