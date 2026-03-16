using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Card.Badge;

public abstract class Card_Badge_Base : IPayload
{
	public readonly AltAssetReference<GameObject> PrefabRef = new AltAssetReference<GameObject>();

	public readonly AltAssetReference<Sprite> IconRef = new AltAssetReference<Sprite>();

	public string LocTitleKey = string.Empty;

	public string LocTermKey = string.Empty;

	public string LocAddendumKey = string.Empty;

	public IEnumerable<string> GetFilePaths()
	{
		yield return PrefabRef.RelativePath;
		yield return IconRef.RelativePath;
	}
}
