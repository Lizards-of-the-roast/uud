using System;
using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.Cards.Text;

public class LandAbilityReplacer
{
	public void Replace(List<AbilityTextData> abilities, IAbilityDataProvider abilityDataProvider, Func<uint, string> getAbilityText, CardTextColorSettings colorSettings)
	{
		if (!ShouldReplace(abilities))
		{
			return;
		}
		int index = 0;
		for (int i = 0; i < abilities.Count; i++)
		{
			uint id = abilities[i].Printing.Id;
			if (id == 1001 || id == 1002 || id == 1003 || id == 1004 || id == 1005)
			{
				index = i + 1;
			}
		}
		AbilityPrintingData abilityPrintingById = abilityDataProvider.GetAbilityPrintingById(1055u);
		string arg = getAbilityText(1055u);
		string formattedLocalizedText = string.Format(colorSettings.DefaultFormat, arg);
		abilities.Insert(index, new AbilityTextData
		{
			Printing = abilityPrintingById,
			FormattedLocalizedText = formattedLocalizedText,
			State = AbilityState.Normal,
			IsKeyword = false,
			IsGroupable = false,
			IsPerpetual = false
		});
		abilities.RemoveAll((AbilityTextData x) => x.Printing.Id == 1001 && x.State == AbilityState.Normal);
		abilities.RemoveAll((AbilityTextData x) => x.Printing.Id == 1002 && x.State == AbilityState.Normal);
		abilities.RemoveAll((AbilityTextData x) => x.Printing.Id == 1003 && x.State == AbilityState.Normal);
		abilities.RemoveAll((AbilityTextData x) => x.Printing.Id == 1004 && x.State == AbilityState.Normal);
		abilities.RemoveAll((AbilityTextData x) => x.Printing.Id == 1005 && x.State == AbilityState.Normal);
	}

	private bool ShouldReplace(List<AbilityTextData> abilities)
	{
		if (abilities == null)
		{
			return false;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		foreach (AbilityTextData ability in abilities)
		{
			uint id = ability.Printing.Id;
			AbilityState state = ability.State;
			flag = flag || (id == 1001 && state == AbilityState.Normal);
			flag2 = flag2 || (id == 1002 && state == AbilityState.Normal);
			flag3 = flag3 || (id == 1003 && state == AbilityState.Normal);
			flag4 = flag4 || (id == 1004 && state == AbilityState.Normal);
			flag5 = flag5 || (id == 1005 && state == AbilityState.Normal);
		}
		return flag && flag2 && flag3 && flag4 && flag5;
	}
}
