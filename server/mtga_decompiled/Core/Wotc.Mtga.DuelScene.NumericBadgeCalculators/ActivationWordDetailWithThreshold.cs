using GreClient.CardData;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.NumericBadgeCalculators;

public class ActivationWordDetailWithThreshold : ActivationWordAdditionalDetailCount, IThresholdBadgeCalculator
{
	public ThresholdBadgeMode BadgeMode { get; set; } = ThresholdBadgeMode.Count;

	public bool HasThreshold(NumericBadgeCalculatorInput input)
	{
		foreach (AbilityWordData activeAbilityWord in input.CardData.ActiveAbilityWords)
		{
			if (activeAbilityWord.AbilityWord == ActivationWord && activeAbilityWord.Threshold.HasValue)
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool GetThreshold(NumericBadgeCalculatorInput input, out int threshold)
	{
		threshold = 0;
		if (input.CardData.ActiveAbilityWords.TryGetUnmetThreshold(ActivationWord, out var thresholdAbilityWord))
		{
			threshold = thresholdAbilityWord.Threshold.Value;
			return true;
		}
		return false;
	}
}
