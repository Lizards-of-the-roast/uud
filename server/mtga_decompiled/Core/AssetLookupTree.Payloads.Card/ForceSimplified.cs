using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card;

public class ForceSimplified : IPayload
{
	public bool UseSimplifiedOverride { get; set; } = true;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
