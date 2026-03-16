using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Booster;

public class Background : IPayload
{
	public AltAssetReference<Texture> TextureRef = new AltAssetReference<Texture>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return TextureRef.RelativePath;
	}
}
