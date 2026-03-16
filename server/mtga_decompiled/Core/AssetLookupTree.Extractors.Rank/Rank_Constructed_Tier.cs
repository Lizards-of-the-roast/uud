using AssetLookupTree.Blackboard;
using Wizards.Mtga.FrontDoorModels;

namespace AssetLookupTree.Extractors.Rank;

public class Rank_Constructed_Tier : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = bb.ConstructedRank.tier;
		return bb.ConstructedRank.rank != RankingClassType.None;
	}
}
