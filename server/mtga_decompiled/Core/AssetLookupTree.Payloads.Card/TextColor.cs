using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card;

public class TextColor : IPayload
{
	public readonly AltAssetReference<CardTextColorTable> ColorTableRef = new AltAssetReference<CardTextColorTable>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return ColorTableRef.RelativePath;
	}
}
