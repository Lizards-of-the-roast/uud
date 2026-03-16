using System.Collections.Generic;
using System.Linq;
using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.NumericBadgeCalculators;

public class GamewideHistoryCountCalculator : INumericBadgeCalculator, IThresholdBadgeCalculator
{
	public uint AbilityGrpId;

	public int Threshold;

	public ThresholdBadgeMode BadgeMode { get; set; } = ThresholdBadgeMode.Count;

	public bool HasNumber(NumericBadgeCalculatorInput input)
	{
		using (IEnumerator<GamewideCountData> enumerator = (input.CardData?.Controller?.GamewideCounts.Where((GamewideCountData x) => x.AbilityId == AbilityGrpId)).GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current.Count != 0;
			}
		}
		return false;
	}

	public bool GetNumber(NumericBadgeCalculatorInput input, out int number, out string modifier)
	{
		using (IEnumerator<GamewideCountData> enumerator = (input.CardData?.Controller?.GamewideCounts.Where((GamewideCountData x) => x.AbilityId == AbilityGrpId)).GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				GamewideCountData current = enumerator.Current;
				if (HasThreshold(input))
				{
					GetThreshold(input, out var threshold);
					number = Mathf.Clamp((int)current.Count, 0, threshold);
					modifier = null;
					return true;
				}
				number = (int)current.Count;
				modifier = null;
				return true;
			}
		}
		number = 0;
		modifier = null;
		return false;
	}

	public bool HasThreshold(NumericBadgeCalculatorInput input)
	{
		return Threshold > 0;
	}

	public bool GetThreshold(NumericBadgeCalculatorInput input, out int threshold)
	{
		threshold = Threshold;
		return true;
	}
}
