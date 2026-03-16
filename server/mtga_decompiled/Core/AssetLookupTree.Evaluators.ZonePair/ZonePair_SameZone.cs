using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.ZonePair;

public class ZonePair_SameZone : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.ZonePair.FromZone == bb.ZonePair.ToZone);
	}
}
