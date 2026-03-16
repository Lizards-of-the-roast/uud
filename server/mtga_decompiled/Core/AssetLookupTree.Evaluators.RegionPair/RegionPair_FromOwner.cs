using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.RegionPair;

public class RegionPair_FromOwner : EvaluatorBase_List<GREPlayerNum>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<GREPlayerNum>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.RegionPair.FromOwner);
	}
}
