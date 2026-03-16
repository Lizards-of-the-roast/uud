using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Card;

public class TextureOverride : ILayeredPayload, IPayload
{
	public class TextureOverrideEntry
	{
		public readonly AltAssetReference<Texture2D> TextureRef = new AltAssetReference<Texture2D>();

		public string Property = "_MainTex";

		public string Keyword = "_USEMAINTEX_ON";

		public string Trigger = "_UseMainTex";
	}

	public readonly HashSet<TextureOverrideEntry> TextureOverrideEntries = new HashSet<TextureOverrideEntry>();

	public HashSet<string> Layers { get; } = new HashSet<string>();

	public IEnumerable<string> GetFilePaths()
	{
		if (TextureOverrideEntries == null)
		{
			yield break;
		}
		foreach (TextureOverrideEntry textureOverrideEntry in TextureOverrideEntries)
		{
			if (textureOverrideEntry.TextureRef != null)
			{
				yield return textureOverrideEntry.TextureRef.RelativePath;
			}
		}
	}
}
