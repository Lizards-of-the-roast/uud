using System.Collections.Generic;

namespace AssetLookupTree.Payloads.MiniCDC;

public class PendingEffectTextOverride : IPayload
{
	public bool IgnorePendingEffect;

	public string LocKey = "";

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
