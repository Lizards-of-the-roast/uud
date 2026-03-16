using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtga.Extensions;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_HasActiveNumericAbilityWord : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Ability != null && bb.CardData != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.CardData.ActiveAbilityWords.Exists((AbilityWordData x) => x.AbilityWord == EnumExtensions.EnumCleanName(bb.Ability.NumericAid)));
		}
		return false;
	}
}
