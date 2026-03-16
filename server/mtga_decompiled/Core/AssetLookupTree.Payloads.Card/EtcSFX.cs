using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card;

public class EtcSFX : IPayload
{
	public SfxData SfxData = new SfxData();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
