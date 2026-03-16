using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace AssetLookupTree.Payloads.Counter;

public class CounterVisuals : IPayload
{
	public AltAssetReference<ViewCounter> Prefab = new AltAssetReference<ViewCounter>();

	public AltAssetReference<Sprite> Sprite = new AltAssetReference<Sprite>();

	public CDCPart_Counters.CounterCategory counterCategory;

	[JsonIgnore]
	public string PrefabPath => Prefab?.RelativePath;

	[JsonIgnore]
	public string SpritePath => Sprite?.RelativePath;

	public IEnumerable<string> GetFilePaths()
	{
		if (Prefab != null)
		{
			yield return Prefab.RelativePath;
		}
		if (Sprite != null)
		{
			yield return Sprite.RelativePath;
		}
	}
}
