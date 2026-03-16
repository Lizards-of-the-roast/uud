using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Evaluators.Ability;

public class Ability_HasDuplicateBaseId : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null && bb.CardData.Abilities != null && bb.Ability != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.CardData.Abilities.Where((AbilityPrintingData x) => x.BaseId == bb.Ability.BaseId).Count() > 1);
		}
		return false;
	}
}
