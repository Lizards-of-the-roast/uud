using AssetLookupTree.Blackboard;
using Wizards.Mtga.FrontDoorModels;

namespace AssetLookupTree.Extractors.Rank;

public class Rank_Limited_Tier : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = bb.LimitedRank.tier;
		return bb.LimitedRank.rank != RankingClassType.None;
	}
}
