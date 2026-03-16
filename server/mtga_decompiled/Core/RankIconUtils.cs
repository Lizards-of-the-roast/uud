using System;
using AssetLookupTree;
using AssetLookupTree.Payloads.Player.PlayerRankSprites;
using Wizards.Mtga.FrontDoorModels;

public static class RankIconUtils
{
	public static RankInfo GetNextRankInfo(RankInfo rank)
	{
		rank.level--;
		if (rank.level <= 0)
		{
			rank.level = 4;
			rank.rankClass = (RankingClassType)Enum.ToObject(typeof(RankingClassType), (int)(rank.rankClass + 1));
		}
		return rank;
	}

	public static PlayerRankSprites GetRankSprite(AssetLookupSystem assetLookupSystem, RankingClassType classType, int tierNum, bool isConstructed)
	{
		assetLookupSystem.Blackboard.Clear();
		if (isConstructed)
		{
			assetLookupSystem.Blackboard.ConstructedRank = ValueTuple.Create(classType, tierNum);
		}
		else
		{
			assetLookupSystem.Blackboard.LimitedRank = ValueTuple.Create(classType, tierNum);
		}
		PlayerRankSprites payload = assetLookupSystem.TreeLoader.LoadTree<PlayerRankSprites>().GetPayload(assetLookupSystem.Blackboard);
		assetLookupSystem.Blackboard.Clear();
		return payload;
	}

	public static string GetRankImagePath(AssetLookupSystem assetLookupSystem, CombinedRankInfo combinedRankInfo, bool isLimited)
	{
		RankInfo rankInfo = (isLimited ? combinedRankInfo.limited : combinedRankInfo.constructed);
		PlayerRankSprites rankSprite = GetRankSprite(assetLookupSystem, rankInfo.rankClass, rankInfo.level, !isLimited);
		if (rankSprite == null)
		{
			return string.Empty;
		}
		return rankSprite.SpriteRef.RelativePath;
	}
}
