using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card.MultistepEffects_FX;

public class MultistepEffect_Start_SFX : IPayload
{
	public SfxData SfxData = new SfxData();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
