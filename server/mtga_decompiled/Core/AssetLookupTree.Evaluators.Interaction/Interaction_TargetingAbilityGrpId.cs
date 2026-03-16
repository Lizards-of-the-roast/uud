using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Interaction;

public class Interaction_TargetingAbilityGrpId : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, (int)bb.TargetSelectionParams.TargetingAbilityGrpId);
	}
}
