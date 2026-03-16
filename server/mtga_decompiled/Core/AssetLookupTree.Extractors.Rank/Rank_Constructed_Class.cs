using AssetLookupTree.Blackboard;
using Wizards.Mtga.FrontDoorModels;

namespace AssetLookupTree.Extractors.Rank;

public class Rank_Constructed_Class : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.ConstructedRank.rank;
		return bb.ConstructedRank.rank != RankingClassType.None;
	}
}
