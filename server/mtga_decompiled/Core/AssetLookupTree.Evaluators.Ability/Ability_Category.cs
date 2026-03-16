using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Ability;

public class Ability_Category : EvaluatorBase_List<AbilityCategory>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Ability != null)
		{
			return EvaluatorBase_List<AbilityCategory>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.Ability.Category);
		}
		return false;
	}
}
