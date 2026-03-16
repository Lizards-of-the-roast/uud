using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Avatar;

public class AvatarFrameColorOverride : ILayeredPayload, IPayload
{
	[Flags]
	public enum OverrideType
	{
		None = 0,
		Tint = 1,
		SpriteSwap = 2,
		MaterialSwap = 4
	}

	public OverrideType Type = OverrideType.Tint;

	public Color Tint = Color.white;

	public readonly AltAssetReference<Sprite> SpriteSwap = new AltAssetReference<Sprite>();

	public readonly AltAssetReference<Material> MaterialSwap = new AltAssetReference<Material>();

	public HashSet<string> Layers { get; } = new HashSet<string>();

	public IEnumerable<string> GetFilePaths()
	{
		if (Type.HasFlag(OverrideType.SpriteSwap) && SpriteSwap?.RelativePath != null)
		{
			yield return SpriteSwap.RelativePath;
		}
		if (Type.HasFlag(OverrideType.MaterialSwap) && MaterialSwap?.RelativePath != null)
		{
			yield return MaterialSwap.RelativePath;
		}
	}
}
