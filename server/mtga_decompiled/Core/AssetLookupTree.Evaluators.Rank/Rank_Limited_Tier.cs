using AssetLookupTree.Blackboard;
using Wizards.Mtga.FrontDoorModels;

namespace AssetLookupTree.Evaluators.Rank;

public class Rank_Limited_Tier : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.LimitedRank.rank != RankingClassType.None)
		{
			return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, bb.LimitedRank.tier);
		}
		return false;
	}
}
