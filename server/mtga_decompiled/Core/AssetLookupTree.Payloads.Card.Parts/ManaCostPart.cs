using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card.Parts;

public class ManaCostPart : IPayload
{
	public readonly AltAssetReference<CDCPart> PartRef = new AltAssetReference<CDCPart>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return PartRef.RelativePath;
	}
}
