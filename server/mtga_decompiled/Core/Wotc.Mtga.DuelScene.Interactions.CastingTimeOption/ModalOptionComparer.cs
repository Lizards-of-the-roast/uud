using System;
using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.DuelScene.Interactions.CastingTimeOption;

public class ModalOptionComparer : IComparer<ModalOption>
{
	private readonly IReadOnlyList<AbilityPrintingData> _hiddenAbilities;

	public ModalOptionComparer(IReadOnlyList<AbilityPrintingData> hiddenAbilities)
	{
		_hiddenAbilities = hiddenAbilities ?? Array.Empty<AbilityPrintingData>();
	}

	public int Compare(ModalOption x, ModalOption y)
	{
		int num = -1;
		int num2 = -1;
		for (int i = 0; i < _hiddenAbilities.Count; i++)
		{
			AbilityPrintingData abilityPrintingData = _hiddenAbilities[i];
			if (num == -1 && abilityPrintingData.Id == x.GrpId)
			{
				num = i;
			}
			if (num2 == -1 && abilityPrintingData.Id == y.GrpId)
			{
				num2 = i;
			}
			if (num != -1 && num2 != -1)
			{
				break;
			}
		}
		if (num == -1 || num2 == -1)
		{
			return 0;
		}
		return num.CompareTo(num2);
	}
}
