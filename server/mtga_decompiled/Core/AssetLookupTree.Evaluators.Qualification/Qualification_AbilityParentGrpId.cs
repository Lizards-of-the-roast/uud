using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Qualification;

public class Qualification_AbilityParentGrpId : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Qualification.HasValue)
		{
			return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, (int)bb.Qualification.Value.AbilityParentGrpId);
		}
		return false;
	}
}
