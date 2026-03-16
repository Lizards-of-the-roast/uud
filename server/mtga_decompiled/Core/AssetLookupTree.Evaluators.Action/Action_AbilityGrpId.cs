using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Action;

public class Action_AbilityGrpId : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.GreAction != null)
		{
			return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, (int)bb.GreAction.AbilityGrpId);
		}
		return false;
	}
}
