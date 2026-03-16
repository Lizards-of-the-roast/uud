using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card;

public class Scaffold : IPayload
{
	public readonly AltAssetReference<ScaffoldingBase> ScaffoldRef = new AltAssetReference<ScaffoldingBase>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return ScaffoldRef.RelativePath;
	}
}
