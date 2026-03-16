using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Ability;

public class Ability_StaticSpellId : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.ActiveResolution == null)
		{
			return false;
		}
		if (bb.ActiveResolution.AbilityPrinting == null && bb.ActiveResolution.Model != null)
		{
			return CheckStaticSpellAbilities(bb);
		}
		if (bb.Ability != null)
		{
			return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, (int)bb.Ability.Id);
		}
		return false;
	}

	private bool CheckStaticSpellAbilities(IBlackboard bb)
	{
		foreach (AbilityPrintingData item in bb.ActiveResolution.Model.Abilities.Where((AbilityPrintingData x) => x.Category == AbilityCategory.Spell || x.Category == AbilityCategory.Static))
		{
			if (EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, (int)item.Id))
			{
				return true;
			}
		}
		return false;
	}
}
