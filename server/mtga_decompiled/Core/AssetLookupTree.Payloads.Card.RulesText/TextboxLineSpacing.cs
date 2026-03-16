using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card.RulesText;

public class TextboxLineSpacing : IPayload
{
	public float LineSpacing;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
