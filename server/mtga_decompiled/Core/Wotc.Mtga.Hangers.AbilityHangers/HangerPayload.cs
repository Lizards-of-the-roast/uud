using System.Collections.Generic;
using AssetLookupTree.Payloads.Ability.Metadata;

namespace Wotc.Mtga.Hangers.AbilityHangers;

public readonly struct HangerPayload
{
	public readonly HangerEntryData Data;

	public readonly IReadOnlyCollection<string> Layers;

	public HangerPayload(HangerEntryData data, IReadOnlyCollection<string> layers)
	{
		Data = data;
		Layers = layers;
	}
}
