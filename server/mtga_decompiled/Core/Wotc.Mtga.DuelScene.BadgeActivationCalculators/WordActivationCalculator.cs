using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.BadgeActivationCalculators;

public class WordActivationCalculator : IBadgeActivationCalculator
{
	public string ActivationWord = string.Empty;

	public bool GetActive(BadgeActivationCalculatorInput input)
	{
		if (string.IsNullOrEmpty(ActivationWord))
		{
			Debug.LogError("Attempting to get activation status for null/empty activation word");
		}
		foreach (AbilityWordData allActiveAbilityWord in GetAllActiveAbilityWords(input.CardData))
		{
			if (allActiveAbilityWord.AbilityWord == ActivationWord)
			{
				return true;
			}
		}
		return false;
	}

	private static IEnumerable<AbilityWordData> GetAllActiveAbilityWords(ICardDataAdapter cardData)
	{
		foreach (AbilityWordData activeAbilityWord in cardData.ActiveAbilityWords)
		{
			yield return activeAbilityWord;
		}
		if (cardData.Parent == null || cardData.ObjectType != GameObjectType.Ability)
		{
			yield break;
		}
		foreach (AbilityWordData activeAbilityWord2 in cardData.Parent.ActiveAbilityWords)
		{
			yield return activeAbilityWord2;
		}
	}
}
