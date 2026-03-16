using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card;

public class FieldTextMode : IPayload
{
	public bool UsePerpetual;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
