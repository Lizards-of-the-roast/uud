using AssetLookupTree.Blackboard;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;

namespace AssetLookupTree.Evaluators.Ability;

public class Ability_IsPrintedAbilityOnParent : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.CardDatabase.GetPrintingFromInstance(bb.CardData?.Parent)?.Abilities.ContainsId(bb.Ability.Id) ?? false);
	}
}
