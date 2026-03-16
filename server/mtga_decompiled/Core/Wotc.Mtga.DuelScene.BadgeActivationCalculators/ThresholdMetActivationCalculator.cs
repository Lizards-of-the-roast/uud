using System.Linq;
using GreClient.CardData;

namespace Wotc.Mtga.DuelScene.BadgeActivationCalculators;

public class ThresholdMetActivationCalculator : IBadgeActivationCalculator
{
	public string ThresholdWord = string.Empty;

	public bool GetActive(BadgeActivationCalculatorInput input)
	{
		if (input.CardData.ActiveAbilityWords.TryGetUnmetThreshold(ThresholdWord, out var thresholdAbilityWord))
		{
			return thresholdAbilityWord.Values.FirstOrDefault() >= thresholdAbilityWord.Threshold;
		}
		return false;
	}
}
