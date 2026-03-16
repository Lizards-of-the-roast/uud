using AssetLookupTree.Blackboard;
using Wizards.Mtga.FrontDoorModels;

namespace AssetLookupTree.Evaluators.Rank;

public class Rank_Constructed_Tier : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.ConstructedRank.rank != RankingClassType.None)
		{
			return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, bb.ConstructedRank.tier);
		}
		return false;
	}
}
