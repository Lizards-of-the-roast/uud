using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Card;

public class LandSymbol : IPayload
{
	public readonly AltAssetReference<Sprite> SpriteRef = new AltAssetReference<Sprite>();

	public Color SpriteColor = Color.white;

	public IEnumerable<string> GetFilePaths()
	{
		yield return SpriteRef.RelativePath;
	}
}
