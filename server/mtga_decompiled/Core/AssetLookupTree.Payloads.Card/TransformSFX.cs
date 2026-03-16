using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card;

public class TransformSFX : IPayload
{
	public SfxData SfxData = new SfxData();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
