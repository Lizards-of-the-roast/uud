using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.BadgeActivationCalculators;

public class ActionActivationCalculator : IBadgeActivationCalculator
{
	public bool GetActive(BadgeActivationCalculatorInput input)
	{
		if (input.CardData.Instance == null)
		{
			return false;
		}
		if (IsActionOnCard(input.Ability.Id, input.CardData.Instance))
		{
			return true;
		}
		if (IsActionOnCard(input.Ability.BaseId, input.CardData.Instance))
		{
			return true;
		}
		foreach (MtgCardInstance linkedFaceInstance in input.CardData.Instance.LinkedFaceInstances)
		{
			if (IsActionOnCard(input.Ability.Id, linkedFaceInstance))
			{
				return true;
			}
			if (IsActionOnCard(input.Ability.BaseId, linkedFaceInstance))
			{
				return true;
			}
		}
		return false;
		static bool IsActionOnCard(uint abilityId, MtgCardInstance card)
		{
			foreach (ActionInfo action in card.Actions)
			{
				if (action.Action != null && action.Action.AbilityGrpId != 0 && action.Action.AbilityGrpId == abilityId)
				{
					return true;
				}
			}
			return false;
		}
	}
}
