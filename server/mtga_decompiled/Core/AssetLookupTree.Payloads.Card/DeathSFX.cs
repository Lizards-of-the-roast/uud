using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card;

public class DeathSFX : IPayload
{
	public SfxData SfxData = new SfxData();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
