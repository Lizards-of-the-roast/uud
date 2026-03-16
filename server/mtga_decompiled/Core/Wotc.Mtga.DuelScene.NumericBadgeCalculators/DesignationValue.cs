using GreClient.Rules;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.NumericBadgeCalculators;

public class DesignationValue : INumericBadgeCalculator
{
	public Designation Designation;

	public bool HasNumber(NumericBadgeCalculatorInput input)
	{
		foreach (DesignationData designation in input.CardData.Instance.Designations)
		{
			if (DesignationTranslator.TryTranslateIntensityDesignation(designation, out var _))
			{
				return true;
			}
		}
		return false;
	}

	public bool GetNumber(NumericBadgeCalculatorInput input, out int number, out string modifier)
	{
		if (Designation == Designation.None)
		{
			Debug.LogError("Attempting to get number for NONE Designation");
		}
		number = 0;
		modifier = null;
		foreach (DesignationData designation in input.CardData.Instance.Designations)
		{
			if (DesignationTranslator.TryTranslateIntensityDesignation(designation, out var intensityDesignation))
			{
				number = (int)intensityDesignation.IntensityLevel;
				return true;
			}
		}
		return false;
	}
}
