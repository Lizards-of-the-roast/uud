using System.Collections.Generic;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace GreClient.CardData;

public static class AbilityPrintingDataExtensions
{
	public static bool IsKeyword(this AbilityPrintingData ability)
	{
		AbilitySubCategory subCategory = ability.SubCategory;
		if (subCategory == AbilitySubCategory.Adapt || (uint)(subCategory - 27) <= 1u || subCategory == AbilitySubCategory.Monstrosity)
		{
			return true;
		}
		if (ability.Id == 188773)
		{
			return false;
		}
		if (!ability.Tags.Contains(MetaDataTag.Ability_Keyword))
		{
			return ability.BaseAbility?.Tags.Contains(MetaDataTag.Ability_Keyword) ?? false;
		}
		return true;
	}

	public static bool IsEvergreen(this AbilityPrintingData ability)
	{
		AbilitySubCategory subCategory = ability.SubCategory;
		if (subCategory == AbilitySubCategory.Surveil || subCategory == AbilitySubCategory.Scry)
		{
			return true;
		}
		if (!ability.Tags.Contains(MetaDataTag.Ability_Evergreen))
		{
			return ability.BaseAbility?.Tags.Contains(MetaDataTag.Ability_Evergreen) ?? false;
		}
		return true;
	}

	public static bool IsEvergreen(this AbilityType abilityType)
	{
		switch (abilityType)
		{
		case AbilityType.ProtectionFrom:
		case AbilityType.Surveil:
		case AbilityType.Scry1:
		case AbilityType.Scry2:
		case AbilityType.Scry3:
		case AbilityType.Scry4:
		case AbilityType.ScryX:
		case AbilityType.Surveil1:
		case AbilityType.Surveil2:
		case AbilityType.Surveil3:
		case AbilityType.Surveil4:
		case AbilityType.Surveil5:
			return true;
		default:
			return false;
		}
	}

	public static bool GetOmitDuplicates(this AbilityPrintingData ability)
	{
		if (ability.PaymentType == AbilityPaymentType.Loyalty)
		{
			return true;
		}
		if (!ability.Tags.Contains(MetaDataTag.Ability_OmitDuplicates))
		{
			return ability.BaseAbility?.Tags.Contains(MetaDataTag.Ability_OmitDuplicates) ?? false;
		}
		return true;
	}

	public static bool IsGroupable(this AbilityPrintingData ability)
	{
		if (!ability.Tags.Contains(MetaDataTag.Ability_Groupable))
		{
			return ability.BaseAbility?.Tags.Contains(MetaDataTag.Ability_Groupable) ?? false;
		}
		return true;
	}

	public static IEnumerable<AbilityPrintingData> GetClassGrantedAbilities(this AbilityPrintingData ability, CardPrintingData card)
	{
		if (card == null || ability == null || ability.SubCategory != AbilitySubCategory.ClassLevel)
		{
			yield break;
		}
		int num = card.Abilities.FindIndex(ability.Id, (AbilityPrintingData cardAbility, uint abilityId) => cardAbility.Id == abilityId);
		if (num == -1 || num + 1 >= card.Abilities.Count)
		{
			yield break;
		}
		AbilityPrintingData abilityPrintingData = card.Abilities[num + 1];
		if (abilityPrintingData.SubCategory != AbilitySubCategory.ClassAbilityGranting)
		{
			yield break;
		}
		foreach (AbilityPrintingData hiddenAbility in abilityPrintingData.HiddenAbilities)
		{
			yield return hiddenAbility;
		}
	}
}
