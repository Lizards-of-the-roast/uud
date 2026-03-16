using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace AssetLookupTree.Payloads.UI.DuelScene.ManaWheel;

public class ManaConfig : IPayload
{
	public Color Tint = Color.black;

	public readonly AltAssetReference<Sprite> Sprite = new AltAssetReference<Sprite>();

	public readonly AltAssetReference<GameObject> PreviewVFX = new AltAssetReference<GameObject>();

	[JsonIgnore]
	public string SpritePath => Sprite?.RelativePath;

	[JsonIgnore]
	public string VFXPath => PreviewVFX?.RelativePath;

	public IEnumerable<string> GetFilePaths()
	{
		if (PreviewVFX != null)
		{
			yield return PreviewVFX.RelativePath;
		}
		if (Sprite != null)
		{
			yield return Sprite.RelativePath;
		}
	}
}
