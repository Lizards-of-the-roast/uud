using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.NumericBadgeCalculators;

public class BinaryThresholdCalculator : INumericBadgeCalculator, IThresholdBadgeCalculator
{
	public string ActivationWord = string.Empty;

	public ThresholdBadgeMode BadgeMode { get; set; } = ThresholdBadgeMode.Icon;

	public bool HasNumber(NumericBadgeCalculatorInput input)
	{
		return true;
	}

	public bool GetNumber(NumericBadgeCalculatorInput input, out int number, out string modifier)
	{
		number = 0;
		foreach (AbilityWordData activeAbilityWord in input.CardData.ActiveAbilityWords)
		{
			if (activeAbilityWord.AbilityWord == ActivationWord)
			{
				number = 1;
				break;
			}
		}
		modifier = null;
		return true;
	}

	public bool HasThreshold(NumericBadgeCalculatorInput input)
	{
		return true;
	}

	public bool GetThreshold(NumericBadgeCalculatorInput input, out int threshold)
	{
		threshold = 1;
		return true;
	}
}
