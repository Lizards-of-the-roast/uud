using AssetLookupTree.Blackboard;
using Wizards.Mtga.FrontDoorModels;

namespace AssetLookupTree.Evaluators.Rank;

public class Rank_Constructed_Class : EvaluatorBase_List<RankingClassType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.ConstructedRank.rank != RankingClassType.None)
		{
			return EvaluatorBase_List<RankingClassType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.ConstructedRank.rank);
		}
		return false;
	}
}
