using System;
using System.Collections.Generic;
using System.Linq;
using GreClient.Rules;

namespace GreClient.CardData;

public static class AbilityWordExtensions
{
	public static bool TryGetUnmetThreshold<T>(this IEnumerable<AbilityWordData> abilityWordDatas, T data, Func<T, string, bool> predicate, out AbilityWordData thresholdAbilityWord)
	{
		AbilityWordData? abilityWordData = null;
		foreach (AbilityWordData abilityWordData2 in abilityWordDatas)
		{
			if (predicate(data, abilityWordData2.AbilityWord) && abilityWordData2.Threshold.HasValue)
			{
				if (abilityWordData2.Values.FirstOrDefault() < abilityWordData2.Threshold)
				{
					abilityWordData = abilityWordData2;
					break;
				}
				if (!abilityWordData.HasValue || abilityWordData2.Threshold > abilityWordData.Value.Threshold)
				{
					abilityWordData = abilityWordData2;
				}
			}
		}
		thresholdAbilityWord = (abilityWordData.HasValue ? abilityWordData.Value : default(AbilityWordData));
		return abilityWordData.HasValue;
	}

	public static bool TryGetUnmetThreshold(this IEnumerable<AbilityWordData> abilityWordDatas, string abilityWord, out AbilityWordData thresholdAbilityWord)
	{
		return abilityWordDatas.TryGetUnmetThreshold(abilityWord, (string checkWord, string text) => checkWord == text, out thresholdAbilityWord);
	}
}
