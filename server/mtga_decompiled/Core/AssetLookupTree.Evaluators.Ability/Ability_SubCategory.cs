using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Ability;

public class Ability_SubCategory : EvaluatorBase_List<AbilitySubCategory>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Ability != null)
		{
			return EvaluatorBase_List<AbilitySubCategory>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.Ability.SubCategory);
		}
		return false;
	}
}
