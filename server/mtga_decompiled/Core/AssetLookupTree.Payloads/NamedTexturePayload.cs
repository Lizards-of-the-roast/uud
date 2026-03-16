using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads;

public class NamedTexturePayload : IPayload
{
	public AltAssetReference<Texture> Reference = new AltAssetReference<Texture>();

	public IEnumerable<string> GetFilePaths()
	{
		if (Reference != null)
		{
			yield return Reference.RelativePath;
		}
	}
}
