using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.General;

public class BattlefieldId : EvaluatorBase_List<string>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<string>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.BattlefieldId);
	}
}
