using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.ZonePair;

public class ZonePair_SameOwner : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.ZonePair.FromOwner == bb.ZonePair.ToOwner);
	}
}
