using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card.RulesText;

public class AvatarMiniCdcOverride : IPayload
{
	public List<uint> AbilityIds { get; } = new List<uint>();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
