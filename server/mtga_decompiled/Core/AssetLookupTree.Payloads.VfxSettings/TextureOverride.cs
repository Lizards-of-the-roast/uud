using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.VfxSettings;

public class TextureOverride : ILayeredPayload, IPayload
{
	public readonly AltAssetReference<Texture2D> TextureRef = new AltAssetReference<Texture2D>();

	public string Property = "_LUT";

	public string Keyword = "_USELUT_ON";

	public string Trigger = "_UseLUT";

	public HashSet<string> Layers { get; } = new HashSet<string>();

	public IEnumerable<string> GetFilePaths()
	{
		if (TextureRef != null)
		{
			yield return TextureRef.RelativePath;
		}
	}
}
