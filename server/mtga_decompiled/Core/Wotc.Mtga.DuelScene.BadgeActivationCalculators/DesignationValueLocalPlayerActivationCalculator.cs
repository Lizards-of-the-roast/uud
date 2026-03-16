using GreClient.Rules;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.BadgeActivationCalculators;

public class DesignationValueLocalPlayerActivationCalculator : IBadgeActivationCalculator
{
	public Designation ActivationDesignation;

	public int DesignationValue;

	public bool GetActive(BadgeActivationCalculatorInput input)
	{
		if (ActivationDesignation == Designation.None)
		{
			Debug.LogError("Attempting to get activation status for NONE designation");
		}
		if (input.GameState == null)
		{
			return false;
		}
		foreach (DesignationData designation in input.GameState.LocalPlayer.Designations)
		{
			if (designation.Type == ActivationDesignation && designation.Value == DesignationValue)
			{
				return true;
			}
		}
		return false;
	}
}
