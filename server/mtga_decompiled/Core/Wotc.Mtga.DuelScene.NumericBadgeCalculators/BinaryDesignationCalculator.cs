using System.Collections.Generic;
using System.Linq;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.NumericBadgeCalculators;

public class BinaryDesignationCalculator : INumericBadgeCalculator, IThresholdBadgeCalculator
{
	public Designation Designation;

	public ThresholdBadgeMode BadgeMode { get; set; } = ThresholdBadgeMode.Icon;

	public bool HasNumber(NumericBadgeCalculatorInput input)
	{
		return true;
	}

	public bool GetNumber(NumericBadgeCalculatorInput input, out int number, out string modifier)
	{
		if (Designation == Designation.None)
		{
			Debug.LogError("Attempting to get number forr NONE Designation");
		}
		number = 0;
		modifier = null;
		IEnumerable<DesignationData> enumerable = input.CardData.Instance?.Designations;
		foreach (DesignationData item in enumerable ?? Enumerable.Empty<DesignationData>())
		{
			if (item.Type == Designation)
			{
				number = 1;
				break;
			}
		}
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
