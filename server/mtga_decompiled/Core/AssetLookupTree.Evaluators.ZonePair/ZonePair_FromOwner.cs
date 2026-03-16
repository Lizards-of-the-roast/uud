using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.ZonePair;

public class ZonePair_FromOwner : EvaluatorBase_List<GREPlayerNum>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<GREPlayerNum>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.ZonePair.FromOwner);
	}
}
