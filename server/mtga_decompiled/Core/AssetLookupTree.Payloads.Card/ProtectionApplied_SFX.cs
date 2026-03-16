using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card;

public class ProtectionApplied_SFX : IPayload
{
	public SfxData SfxData = new SfxData();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
