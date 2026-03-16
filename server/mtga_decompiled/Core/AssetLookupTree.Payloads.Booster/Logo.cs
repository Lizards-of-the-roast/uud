using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Booster;

public class Logo : IPayload
{
	public AltAssetReference<Texture> TextureRef = new AltAssetReference<Texture>();

	public AltAssetReference<Texture> HeaderLogo = new AltAssetReference<Texture>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return TextureRef.RelativePath;
		yield return HeaderLogo.RelativePath;
	}

	public string GetHeaderFilePath()
	{
		if (!string.IsNullOrEmpty(HeaderLogo.RelativePath))
		{
			return HeaderLogo.RelativePath;
		}
		return TextureRef.RelativePath;
	}
}
