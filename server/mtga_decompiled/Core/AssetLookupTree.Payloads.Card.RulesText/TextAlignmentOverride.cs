using System.Collections.Generic;
using TMPro;

namespace AssetLookupTree.Payloads.Card.RulesText;

public class TextAlignmentOverride : IPayload
{
	public TextAlignmentOptions AlignmentOptions = TextAlignmentOptions.TopLeft;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
