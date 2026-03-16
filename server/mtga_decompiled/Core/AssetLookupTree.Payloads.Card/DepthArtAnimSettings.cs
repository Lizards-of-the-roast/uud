using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card;

public class DepthArtAnimSettings : IPayload
{
	public readonly AltAssetReference<DepthArtSettings> SettingsRef = new AltAssetReference<DepthArtSettings>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return SettingsRef.RelativePath;
	}
}
