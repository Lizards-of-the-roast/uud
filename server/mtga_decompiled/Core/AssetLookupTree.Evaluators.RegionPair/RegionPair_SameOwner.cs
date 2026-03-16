using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.RegionPair;

public class RegionPair_SameOwner : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.RegionPair.FromOwner == bb.RegionPair.ToOwner);
	}
}
