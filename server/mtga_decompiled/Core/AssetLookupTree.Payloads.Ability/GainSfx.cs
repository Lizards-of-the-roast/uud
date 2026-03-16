using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Ability;

public class GainSfx : IPayload
{
	public SfxData SfxData = new SfxData();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
