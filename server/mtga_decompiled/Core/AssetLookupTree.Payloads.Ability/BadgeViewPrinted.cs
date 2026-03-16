using System.Collections.Generic;
using Wotc.Mtga.DuelScene.CardView.BattlefieldIcons;

namespace AssetLookupTree.Payloads.Ability;

public class BadgeViewPrinted : IPayload
{
	public readonly AltAssetReference<BadgeEntryView> BadgeEntryView = new AltAssetReference<BadgeEntryView>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return BadgeEntryView.RelativePath;
	}
}
