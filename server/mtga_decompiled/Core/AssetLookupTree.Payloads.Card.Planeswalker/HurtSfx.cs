using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card.Planeswalker;

public class HurtSfx : IPayload
{
	public SfxData SfxData = new SfxData();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
