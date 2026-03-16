using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Card;

public class FaceSymbol : IPayload
{
	public readonly AltAssetReference<Sprite> FrontSymbolRef = new AltAssetReference<Sprite>();

	public readonly AltAssetReference<Sprite> BackSymbolRef = new AltAssetReference<Sprite>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return FrontSymbolRef.RelativePath;
		yield return BackSymbolRef.RelativePath;
	}
}
