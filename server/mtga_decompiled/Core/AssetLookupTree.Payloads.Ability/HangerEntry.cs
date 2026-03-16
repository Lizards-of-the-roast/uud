using System.Collections.Generic;
using AssetLookupTree.Payloads.Ability.Metadata;

namespace AssetLookupTree.Payloads.Ability;

public class HangerEntry : ILayeredPayload, IPayload
{
	public readonly HangerEntryData Data = new HangerEntryData();

	public HashSet<string> Layers { get; } = new HashSet<string>();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
