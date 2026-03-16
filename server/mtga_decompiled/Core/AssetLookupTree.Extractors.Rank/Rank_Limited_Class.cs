using AssetLookupTree.Blackboard;
using Wizards.Mtga.FrontDoorModels;

namespace AssetLookupTree.Extractors.Rank;

public class Rank_Limited_Class : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.LimitedRank.rank;
		return bb.LimitedRank.rank != RankingClassType.None;
	}
}
