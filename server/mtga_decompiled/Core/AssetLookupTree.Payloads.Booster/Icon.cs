using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Booster;

public class Icon : IPayload
{
	public AltAssetReference<Sprite> SpriteRef = new AltAssetReference<Sprite>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return SpriteRef.RelativePath;
	}
}
