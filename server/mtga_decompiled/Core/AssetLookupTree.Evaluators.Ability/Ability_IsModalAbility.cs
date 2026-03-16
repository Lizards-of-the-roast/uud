using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Ability;

public class Ability_IsModalAbility : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.Ability?.IsModalAbility() ?? false);
	}
}
