using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.NumericBadgeCalculators;

public class BinaryTriggerConditionMetCalculator : INumericBadgeCalculator, IThresholdBadgeCalculator
{
	public string ActivationWord = string.Empty;

	public ThresholdBadgeMode BadgeMode { get; set; } = ThresholdBadgeMode.Count;

	public bool HasNumber(NumericBadgeCalculatorInput input)
	{
		return true;
	}

	public bool GetNumber(NumericBadgeCalculatorInput input, out int number, out string modifier)
	{
		if (string.IsNullOrEmpty(ActivationWord))
		{
			Debug.LogError("Attempting to get number for null/empty activation word");
		}
		number = 0;
		modifier = null;
		foreach (AbilityWordData activeAbilityWord in input.CardData.ActiveAbilityWords)
		{
			if (activeAbilityWord.AbilityWord == ActivationWord && !string.IsNullOrEmpty(activeAbilityWord.AdditionalDetail) && int.TryParse(activeAbilityWord.AdditionalDetail, out var result))
			{
				number = result;
				modifier = activeAbilityWord.ModifierString;
				return true;
			}
		}
		return false;
	}

	public bool HasThreshold(NumericBadgeCalculatorInput input)
	{
		return false;
	}

	public bool GetThreshold(NumericBadgeCalculatorInput input, out int threshold)
	{
		threshold = 0;
		return true;
	}
}
