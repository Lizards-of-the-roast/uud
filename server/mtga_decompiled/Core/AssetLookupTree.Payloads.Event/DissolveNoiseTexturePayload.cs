using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Event;

public class DissolveNoiseTexturePayload : IPayload
{
	public AltAssetReference<Texture> TextureRef = new AltAssetReference<Texture>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return TextureRef.RelativePath;
	}
}
