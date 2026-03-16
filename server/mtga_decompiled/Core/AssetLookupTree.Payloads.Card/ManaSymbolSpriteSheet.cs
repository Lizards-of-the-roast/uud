using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AssetLookupTree.Payloads.Card;

public class ManaSymbolSpriteSheet : IPayload
{
	public readonly AltAssetReference<TMP_SpriteAsset> SpriteSheet = new AltAssetReference<TMP_SpriteAsset>();

	public Color TintColor = Color.white;

	public IEnumerable<string> GetFilePaths()
	{
		yield return SpriteSheet.RelativePath;
	}
}
