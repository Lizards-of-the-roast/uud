using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.Ability;

public class Ability_IsExhausted : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, IsExhausted(bb.Ability, bb.CardData));
	}

	private bool IsExhausted(AbilityPrintingData ability, ICardDataAdapter cardData)
	{
		if (ability == null || cardData == null)
		{
			return false;
		}
		MtgCardInstance instance = cardData.Instance;
		if (instance == null)
		{
			return false;
		}
		foreach (MtgAbilityInstance abilityInstance in instance.AbilityInstances)
		{
			if (abilityInstance.IsExhausted && abilityInstance.Printing.Id == ability.Id)
			{
				return true;
			}
		}
		return false;
	}
}
