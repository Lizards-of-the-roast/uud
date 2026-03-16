using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.NumericBadgeCalculators;

public class AffectorOfLinkedInfoIntValue : INumericBadgeCalculator
{
	public string DetailKey = string.Empty;

	public bool UseParent;

	public bool GetNumber(NumericBadgeCalculatorInput input, out int number, out string modifier)
	{
		MtgCardInstance input2 = (UseParent ? input.CardData.Parent : input.CardData.Instance);
		return GetNumber_Internal(input2, out number, out modifier);
	}

	private bool GetNumber_Internal(MtgCardInstance input, out int number, out string modifier)
	{
		modifier = null;
		foreach (LinkInfoData affectorOfLinkInfo in input.AffectorOfLinkInfos)
		{
			if (affectorOfLinkInfo.Details.TryGetValue(DetailKey, out var value))
			{
				number = (int)value;
				return true;
			}
		}
		number = 0;
		return false;
	}

	public bool HasNumber(NumericBadgeCalculatorInput input)
	{
		if (input.CardData == null)
		{
			return false;
		}
		MtgCardInstance instance = (UseParent ? input.CardData.Parent : input.CardData.Instance);
		return HasNumber_Internal(instance);
	}

	private bool HasNumber_Internal(MtgCardInstance instance)
	{
		if (instance == null)
		{
			return false;
		}
		IEnumerable<LinkInfoData> affectorOfLinkInfos = instance.AffectorOfLinkInfos;
		if (affectorOfLinkInfos == null)
		{
			return false;
		}
		foreach (LinkInfoData item in affectorOfLinkInfos)
		{
			if (item.Details.TryGetValue(DetailKey, out var value) && value is int)
			{
				return true;
			}
		}
		return false;
	}
}
