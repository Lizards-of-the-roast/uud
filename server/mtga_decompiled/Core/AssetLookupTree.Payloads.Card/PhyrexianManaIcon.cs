using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Card;

public class PhyrexianManaIcon : IPayload
{
	public readonly AltAssetReference<Sprite> SpriteRef = new AltAssetReference<Sprite>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return SpriteRef.RelativePath;
	}
}
