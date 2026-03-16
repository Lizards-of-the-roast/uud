using System.Collections.Generic;
using Wotc.Mtga.DuelScene.CardView.BattlefieldIcons;

namespace AssetLookupTree.Payloads.Card;

public class AttackerIcon : IPayload
{
	public readonly AltAssetReference<CombatIcon_AttackerFrame> FrameRef = new AltAssetReference<CombatIcon_AttackerFrame>();

	public readonly OffsetData OffsetData = new OffsetData();

	public IEnumerable<string> GetFilePaths()
	{
		yield return FrameRef.RelativePath;
	}
}
