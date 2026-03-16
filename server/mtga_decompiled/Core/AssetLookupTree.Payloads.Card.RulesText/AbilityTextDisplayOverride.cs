using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card.RulesText;

public class AbilityTextDisplayOverride : IPayload
{
	public bool HideAbility = true;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
