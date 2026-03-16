using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public static class CostModifiedHanger
{
	public static void ActionTypeFilter(ref List<MtgAction> actions)
	{
		actions.RemoveAll((MtgAction x) => x.ActionType != ActionType.Activate);
	}

	public static void NullAbilityFilter(ref List<MtgAction> actions)
	{
		actions.RemoveAll((MtgAction x) => x.AbilityData == null);
	}

	public static void ManaCostFilter(ref List<MtgAction> actions)
	{
		actions.RemoveAll((MtgAction x) => ActionCostEqualsPrintingCost(x));
	}

	public static bool ActionCostEqualsPrintingCost(MtgAction action)
	{
		return ManaUtilities.CompareManaCosts(action.ActionCost, action.AbilityData.ManaCost) == 0;
	}

	public static HangerConfig CreateConfig(IReadOnlyList<ManaQuantity> manaQuantities)
	{
		string localizedText = Languages.ActiveLocProvider.GetLocalizedText("AbilityHanger/CostChange/ModifiedAbilityCost_Title");
		string item = ManaUtilities.ConvertToOldSchoolManaText(manaQuantities);
		string localizedText2 = Languages.ActiveLocProvider.GetLocalizedText("AbilityHanger/CostChange/ModifiedAbilityCost_Body", ("newCost", item));
		return new HangerConfig(localizedText, localizedText2);
	}
}
