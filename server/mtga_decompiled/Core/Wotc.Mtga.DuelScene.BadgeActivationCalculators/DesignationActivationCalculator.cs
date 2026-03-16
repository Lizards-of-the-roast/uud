using GreClient.Rules;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.BadgeActivationCalculators;

public class DesignationActivationCalculator : IBadgeActivationCalculator
{
	public Designation ActivationDesignation;

	public bool GetActive(BadgeActivationCalculatorInput input)
	{
		if (ActivationDesignation == Designation.None)
		{
			Debug.LogError("Attempting to get activation status for NONE designation");
		}
		foreach (DesignationData designation in input.CardData.Instance.Designations)
		{
			if (designation.Type == ActivationDesignation)
			{
				return true;
			}
		}
		return false;
	}
}
