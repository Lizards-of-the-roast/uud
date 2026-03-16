using AssetLookupTree.Blackboard;
using Wizards.Mtga.FrontDoorModels;

namespace AssetLookupTree.Evaluators.Rank;

public class Rank_Limited_Class : EvaluatorBase_List<RankingClassType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.LimitedRank.rank != RankingClassType.None)
		{
			return EvaluatorBase_List<RankingClassType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.LimitedRank.rank);
		}
		return false;
	}
}
