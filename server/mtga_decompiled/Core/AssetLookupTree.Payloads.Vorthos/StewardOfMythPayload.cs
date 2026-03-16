using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Vorthos;

public class StewardOfMythPayload : IPayload
{
	public AltAssetReference<Sprite> Reference = new AltAssetReference<Sprite>();

	public IEnumerable<string> GetFilePaths()
	{
		if (Reference != null)
		{
			yield return Reference.RelativePath;
		}
	}
}
