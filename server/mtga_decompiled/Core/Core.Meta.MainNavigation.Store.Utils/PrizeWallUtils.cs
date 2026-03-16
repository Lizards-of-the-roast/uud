using AssetLookupTree;
using AssetLookupTree.Payloads.PrizeWall;

namespace Core.Meta.MainNavigation.Store.Utils;

public static class PrizeWallUtils
{
	public static string GetBackgroundImagePath(AssetLookupSystem assetLookupSystem, string prizeWallId)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.LookupString = prizeWallId;
		PrizeWallBGPayload payload = assetLookupSystem.TreeLoader.LoadTree<PrizeWallBGPayload>().GetPayload(assetLookupSystem.Blackboard);
		assetLookupSystem.Blackboard.Clear();
		return payload?.Reference.RelativePath;
	}

	public static string GetTokenImagePath(AssetLookupSystem assetLookupSystem, string tokenId)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.LookupString = tokenId;
		PrizeWallTokenPayload payload = assetLookupSystem.TreeLoader.LoadTree<PrizeWallTokenPayload>().GetPayload(assetLookupSystem.Blackboard);
		assetLookupSystem.Blackboard.Clear();
		return payload?.Reference.RelativePath;
	}
}
