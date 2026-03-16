using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.NumericBadgeCalculators;

public class ActivationWordAdditionalDetailCount : INumericBadgeCalculator
{
	public string ActivationWord = string.Empty;

	public bool HasNumber(NumericBadgeCalculatorInput input)
	{
		foreach (AbilityWordData activeAbilityWord in input.CardData.ActiveAbilityWords)
		{
			if (activeAbilityWord.AbilityWord == ActivationWord && !string.IsNullOrEmpty(activeAbilityWord.AdditionalDetail) && int.TryParse(activeAbilityWord.AdditionalDetail, out var _))
			{
				return true;
			}
		}
		return false;
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

	public int[] GetValues(NumericBadgeCalculatorInput input)
	{
		if (string.IsNullOrEmpty(ActivationWord))
		{
			Debug.LogError("Attempting to get number for null/empty activation word");
		}
		foreach (AbilityWordData activeAbilityWord in input.CardData.ActiveAbilityWords)
		{
			if (activeAbilityWord.AbilityWord == ActivationWord)
			{
				return activeAbilityWord.Values;
			}
		}
		return null;
	}
}
