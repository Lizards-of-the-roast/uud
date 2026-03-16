using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.RegionPair;

public class RegionPair_ToRegion : EvaluatorBase_List<BattlefieldRegionType>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<BattlefieldRegionType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.RegionPair.ToRegion);
	}
}
