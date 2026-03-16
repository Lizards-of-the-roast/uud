using System.Collections.Generic;
using TMPro;

namespace AssetLookupTree.Payloads.Card;

public class FieldFont : IPayload
{
	public bool CanSwapMaterial = true;

	public readonly AltAssetReference<TMP_FontAsset> FontAssetReference = new AltAssetReference<TMP_FontAsset>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return FontAssetReference.RelativePath;
	}
}
