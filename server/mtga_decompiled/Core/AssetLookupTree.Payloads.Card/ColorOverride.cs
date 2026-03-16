using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card;

public class ColorOverride : IPayload
{
	public readonly AltAssetReference<CardColorTable> ColorTableRef = new AltAssetReference<CardColorTable>();

	public string PrimaryProperty = string.Empty;

	public string SecondaryProperty = string.Empty;

	public bool UsePrintedFrameColors;

	public IEnumerable<string> GetFilePaths()
	{
		if (ColorTableRef != null)
		{
			yield return ColorTableRef.RelativePath;
		}
	}
}
