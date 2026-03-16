using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card.UnlocalizedText;

public abstract class UnlocalizedText : IPayload
{
	public string Text;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
