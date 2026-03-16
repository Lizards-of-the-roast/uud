using System.Collections.Generic;

namespace AssetLookupTree.Payloads.GeneralEffect;

public class UserActionTakenSfx : IPayload
{
	public SfxData SfxData = new SfxData();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
