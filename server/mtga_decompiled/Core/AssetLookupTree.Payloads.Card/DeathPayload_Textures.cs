using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Card;

public class DeathPayload_Textures : IPayload
{
	public readonly AltAssetReference<Texture2D> RampTexRef = new AltAssetReference<Texture2D>();

	public readonly AltAssetReference<Texture2D> NoiseTexRef = new AltAssetReference<Texture2D>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return RampTexRef.RelativePath;
		yield return NoiseTexRef.RelativePath;
	}
}
