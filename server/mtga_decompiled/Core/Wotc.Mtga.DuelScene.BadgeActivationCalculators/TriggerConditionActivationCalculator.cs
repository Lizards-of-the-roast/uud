using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.BadgeActivationCalculators;

public class TriggerConditionActivationCalculator : IBadgeActivationCalculator
{
	public bool GetActive(BadgeActivationCalculatorInput input)
	{
		foreach (AbilityWordData activeAbilityWord in input.CardData.ActiveAbilityWords)
		{
			if (activeAbilityWord.TriggerConditionMet == 1)
			{
				return true;
			}
		}
		return false;
	}
}
