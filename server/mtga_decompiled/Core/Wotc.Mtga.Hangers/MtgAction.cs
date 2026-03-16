using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public readonly struct MtgAction
{
	public readonly ActionType ActionType;

	public readonly IReadOnlyList<ManaQuantity> ActionCost;

	public readonly AbilityPrintingData AbilityData;

	public MtgAction(Action action, AbilityPrintingData abilityData)
	{
		ActionType = action.ActionType;
		ActionCost = action.ConvertedActionManaCost(abilityData);
		AbilityData = abilityData;
	}

	public static MtgAction Convert(ActionInfo actionInfo, IAbilityDataProvider cardDatabase)
	{
		Action action = actionInfo.Action;
		AbilityPrintingData abilityPrintingById = cardDatabase.GetAbilityPrintingById(action.AbilityGrpId);
		return new MtgAction(action, abilityPrintingById);
	}
}
