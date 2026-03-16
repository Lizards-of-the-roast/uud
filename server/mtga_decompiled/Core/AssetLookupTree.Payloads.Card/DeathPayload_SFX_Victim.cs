using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card;

public class DeathPayload_SFX_Victim : IPayload
{
	public SfxData SfxData = new SfxData();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
