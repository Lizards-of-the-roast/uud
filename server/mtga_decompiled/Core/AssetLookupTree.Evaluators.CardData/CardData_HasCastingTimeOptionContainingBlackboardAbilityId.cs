using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_HasCastingTimeOptionContainingBlackboardAbilityId : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb?.Ability != null)
		{
			ICardDataAdapter cardData = bb.CardData;
			if (cardData != null && cardData.CastingTimeOptions?.Count > 0)
			{
				return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.CardData.CastingTimeOptions.Where((CastingTimeOption cto) => cto.AbilityId == bb.Ability.Id).Count() > 0);
			}
		}
		return false;
	}
}
