using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Ability;

public class Ability_IsResolving : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.Ability != null && bb.ActiveResolution?.AbilityPrinting != null && bb.Ability.Id == bb.ActiveResolution.AbilityPrinting.Id);
	}
}
