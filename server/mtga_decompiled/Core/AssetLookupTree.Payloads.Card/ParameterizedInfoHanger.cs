using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Card;

public class ParameterizedInfoHanger : LocParameterProviderPayload, ILayeredPayload, IPayload
{
	public readonly AltAssetReference<Sprite> SpriteRef = new AltAssetReference<Sprite>();

	public string HeaderLocKey = string.Empty;

	public string BodyLocKey = string.Empty;

	public HashSet<string> Layers { get; } = new HashSet<string>();

	public override IEnumerable<string> GetFilePaths()
	{
		yield return SpriteRef.RelativePath;
	}
}
