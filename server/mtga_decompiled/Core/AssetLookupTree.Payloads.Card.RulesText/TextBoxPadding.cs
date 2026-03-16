using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card.RulesText;

public class TextBoxPadding : IPayload
{
	public float BottomPadding;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
