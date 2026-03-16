using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card;

public class FieldTextColor : IPayload
{
	public readonly AltAssetReference<FieldTextColorSettings> ColorSettingsRef = new AltAssetReference<FieldTextColorSettings>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return ColorSettingsRef.RelativePath;
	}
}
