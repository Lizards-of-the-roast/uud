using System.Collections.Generic;
using Wotc.Mtga.DuelScene.CardView.BattlefieldIcons;

namespace AssetLookupTree.Payloads.Card;

public class BlockerIcon : IPayload
{
	public readonly AltAssetReference<CombatIcon_BlockerFrame> FrameRef = new AltAssetReference<CombatIcon_BlockerFrame>();

	public readonly OffsetData OffsetData = new OffsetData();

	public IEnumerable<string> GetFilePaths()
	{
		yield return FrameRef.RelativePath;
	}
}
