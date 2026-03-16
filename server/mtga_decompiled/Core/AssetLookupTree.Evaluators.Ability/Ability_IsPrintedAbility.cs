using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Evaluators.Ability;

public class Ability_IsPrintedAbility : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Ability == null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, inValue: false);
		}
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.CardData?.Printing?.Abilities.ContainsId(bb.Ability.Id) == true);
	}
}
