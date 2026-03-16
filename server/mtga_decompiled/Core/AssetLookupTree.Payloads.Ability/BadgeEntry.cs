using System.Collections.Generic;
using AssetLookupTree.Payloads.Ability.Metadata;
using Wotc.Mtga.DuelScene.BadgeActivationCalculators;

namespace AssetLookupTree.Payloads.Ability;

public class BadgeEntry : ILayeredPayload, IPayload
{
	public readonly BadgeEntryData Data = new BadgeEntryData();

	public bool HasSprite = true;

	public HashSet<string> Layers { get; } = new HashSet<string>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return Data.SpriteRef.RelativePath;
		if (!(Data.ActivationCalculator is NullActivationCalculator) && Data.ActivatedSpriteRef != null)
		{
			yield return Data.ActivatedSpriteRef.RelativePath;
		}
	}
}
