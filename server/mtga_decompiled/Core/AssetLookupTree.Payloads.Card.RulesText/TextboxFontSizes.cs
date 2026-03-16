using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card.RulesText;

public class TextboxFontSizes : IPayload
{
	public readonly List<float> FontSizes = new List<float>();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
