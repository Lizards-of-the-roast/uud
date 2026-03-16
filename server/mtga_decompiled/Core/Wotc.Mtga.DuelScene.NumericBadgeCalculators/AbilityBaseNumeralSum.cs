using GreClient.CardData;

namespace Wotc.Mtga.DuelScene.NumericBadgeCalculators;

public class AbilityBaseNumeralSum : INumericBadgeCalculator
{
	public int BaseId;

	public bool HasNumber(NumericBadgeCalculatorInput input)
	{
		foreach (AbilityPrintingData ability in input.CardData.Abilities)
		{
			if (ability.BaseId == BaseId)
			{
				return true;
			}
		}
		return false;
	}

	public bool GetNumber(NumericBadgeCalculatorInput input, out int number, out string modifier)
	{
		int num = 0;
		foreach (AbilityPrintingData ability in input.CardData.Abilities)
		{
			if (ability.BaseId == BaseId)
			{
				num += (int)ability.BaseIdNumeral.Value;
			}
		}
		number = num;
		modifier = null;
		return true;
	}
}
