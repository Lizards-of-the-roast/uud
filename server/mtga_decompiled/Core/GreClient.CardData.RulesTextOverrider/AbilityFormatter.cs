using System;
using System.Collections.Generic;
using AssetLookupTree;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Duel;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace GreClient.CardData.RulesTextOverrider;

public static class AbilityFormatter
{
	public static bool IsDungeonRoomCard(CardPrintingData sourceCard)
	{
		return sourceCard?.Types.Contains(CardType.Dungeon) ?? false;
	}

	public static bool IsRollResults(AbilityPrintingData abilityData)
	{
		return abilityData?.ReferencedAbilityTypes.Contains(AbilityType.RollD20) ?? false;
	}

	public static string FormatDungeonRoom(string abilityText, CardTextColorSettings textColorSettings)
	{
		if (abilityText.Contains("—"))
		{
			abilityText = abilityText.Replace("<nobr>", string.Empty).Replace("</nobr>", string.Empty);
			string[] array = abilityText.Split("—".ToCharArray(), 2);
			if (array.Length == 2)
			{
				string text = array[0].Trim();
				abilityText = array[1].Trim();
				abilityText = "<b><font=\"" + Languages.DefaultTitleFontName + "\">" + text + "</font></b>" + Environment.NewLine + abilityText;
			}
		}
		return string.Format(textColorSettings.DefaultFormat, abilityText);
	}

	public static string FormatAbilityWithTargetSpecs(string abilityText, uint abilityId, IAbilityDataProvider abilityProvider, List<TargetSpec> targetSpecs, CardTextColorSettings textColorSettings, AssetLookupSystem assetLookupSystem, ICardDataAdapter sourceCardData)
	{
		abilityText = string.Format(TargetingColorer.GetFormatDueToTargeting(abilityId, abilityProvider, targetSpecs, assetLookupSystem, sourceCardData), abilityText);
		return FormatAbility(abilityText, textColorSettings);
	}

	public static string FormatAbility(string abilityText, CardTextColorSettings textColorSettings)
	{
		return string.Format(textColorSettings.DefaultFormat, abilityText);
	}
}
