using System;
using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.Cards.Text;

public class AbilityTextComparer : IComparer<AbilityTextData>
{
	private const string NEGATIVE_X_LOYALTY_TEXT = "-X";

	private const string POSITIVE_X_LOYALTY_TEXT = "+X";

	private const float NEGATIVE_X_SORT_VALUE = -3.5f;

	private const float POSITIVE_X_SORT_VALUE = 3.5f;

	private AbilityTextData _anchorKeyword;

	private IReadOnlyList<KeyValuePair<AbilityPrintingData, AbilityState>> _allAbilities;

	private IReadOnlyList<AbilityPrintingData> _printingAbilities;

	private IReadOnlyList<MtgCardInstance> _mutationChildren;

	private IReadOnlyList<AbilityTextData> _abilityTextDatas;

	public void SetParams(AbilityTextData anchorKeyword, IReadOnlyList<KeyValuePair<AbilityPrintingData, AbilityState>> allAbilities, IReadOnlyList<AbilityPrintingData> printingAbilities, IReadOnlyList<MtgCardInstance> mutationChildren, IReadOnlyList<AbilityTextData> abilityTextDatas)
	{
		_anchorKeyword = anchorKeyword;
		_allAbilities = allAbilities ?? Array.Empty<KeyValuePair<AbilityPrintingData, AbilityState>>();
		_printingAbilities = printingAbilities ?? Array.Empty<AbilityPrintingData>();
		_mutationChildren = mutationChildren ?? Array.Empty<MtgCardInstance>();
		_abilityTextDatas = abilityTextDatas ?? Array.Empty<AbilityTextData>();
	}

	public void ClearParams()
	{
		_anchorKeyword = null;
		_allAbilities = Array.Empty<KeyValuePair<AbilityPrintingData, AbilityState>>();
		_printingAbilities = Array.Empty<AbilityPrintingData>();
		_mutationChildren = Array.Empty<MtgCardInstance>();
		_abilityTextDatas = Array.Empty<AbilityTextData>();
	}

	public int Compare(AbilityTextData lhs, AbilityTextData rhs)
	{
		int num = 0;
		bool flag = lhs.State.HasFlag(AbilityState.Added);
		bool flag2 = rhs.State.HasFlag(AbilityState.Added);
		if (flag && flag2)
		{
			num = lhs.IsLoyaltyAbility.CompareTo(rhs.IsLoyaltyAbility);
			if (num != 0)
			{
				return num;
			}
			float value = (lhs.IsLoyaltyAbility ? ConvertLoyaltyCostToSortValue(lhs.Printing.LoyaltyCost) : 0f);
			num = (rhs.IsLoyaltyAbility ? ConvertLoyaltyCostToSortValue(rhs.Printing.LoyaltyCost) : 0f).CompareTo(value);
			if (num != 0)
			{
				return num;
			}
		}
		bool value2 = flag && !lhs.IsKeyword;
		num = (flag2 && !rhs.IsKeyword).CompareTo(value2);
		if (num != 0)
		{
			return num;
		}
		bool value3 = flag && !lhs.IsGroupable && lhs.IsKeyword;
		num = (flag2 && !rhs.IsGroupable && rhs.IsKeyword).CompareTo(value3);
		if (num != 0)
		{
			return num;
		}
		if (flag && lhs.IsGroupable && lhs.IsKeyword && _anchorKeyword == rhs && _anchorKeyword.State != AbilityState.Added)
		{
			return -1;
		}
		if (flag2 && rhs.IsKeyword && rhs.IsGroupable && _anchorKeyword == lhs)
		{
			AbilityTextData anchorKeyword = _anchorKeyword;
			if (anchorKeyword == null || anchorKeyword.State != AbilityState.Added)
			{
				return 1;
			}
		}
		if (_anchorKeyword != null)
		{
			if (lhs.IsGroupable && !rhs.IsGroupable && lhs != _anchorKeyword)
			{
				int num2 = _abilityTextDatas.IndexOf(_anchorKeyword);
				int value4 = _abilityTextDatas.IndexOf(rhs);
				num = num2.CompareTo(value4);
				if (num != 0)
				{
					return num;
				}
			}
			else if (rhs.IsGroupable && !lhs.IsGroupable && rhs != _anchorKeyword)
			{
				int value5 = _abilityTextDatas.IndexOf(_anchorKeyword);
				num = _abilityTextDatas.IndexOf(lhs).CompareTo(value5);
				if (num != 0)
				{
					return num;
				}
			}
		}
		bool value6 = flag && lhs.IsGroupable && lhs.IsKeyword;
		num = (flag2 && rhs.IsGroupable && rhs.IsKeyword).CompareTo(value6);
		if (num != 0)
		{
			return num;
		}
		if (_mutationChildren.Count > 0)
		{
			bool value7 = _printingAbilities.Exists(lhs.Printing.Id, (AbilityPrintingData x, uint printingId) => x.Id == printingId);
			num = _printingAbilities.Exists(rhs.Printing.Id, (AbilityPrintingData x, uint printingId) => x.Id == printingId).CompareTo(value7);
			if (num != 0)
			{
				return num;
			}
			int num3 = _mutationChildren.FindIndex(lhs.Printing.Id, (MtgCardInstance x, uint printingId) => x.Abilities.Exists(printingId, (AbilityPrintingData z, uint zId) => z.Id == zId));
			int value8 = _mutationChildren.FindIndex(rhs.Printing.Id, (MtgCardInstance x, uint printingId) => x.Abilities.Exists(printingId, (AbilityPrintingData z, uint zId) => z.Id == zId));
			num = num3.CompareTo(value8);
			if (num != 0)
			{
				return num;
			}
		}
		bool flag3 = flag && lhs.IsPerpetual;
		bool value9 = flag2 && rhs.IsPerpetual;
		num = flag3.CompareTo(value9);
		if (num != 0)
		{
			return num;
		}
		int num4 = _printingAbilities.FindIndex(lhs.Printing.Id, (AbilityPrintingData x, uint printingId) => x.Id == printingId);
		int value10 = _printingAbilities.FindIndex(rhs.Printing.Id, (AbilityPrintingData x, uint printingId) => x.Id == printingId);
		num = num4.CompareTo(value10);
		if (num != 0)
		{
			return num;
		}
		int num5 = _allAbilities.FindIndex(lhs.Printing.Id, (KeyValuePair<AbilityPrintingData, AbilityState> x, uint printingId) => x.Key.Id == printingId);
		int value11 = _allAbilities.FindIndex(rhs.Printing.Id, (KeyValuePair<AbilityPrintingData, AbilityState> x, uint printingId) => x.Key.Id == printingId);
		num = num5.CompareTo(value11);
		if (num != 0)
		{
			return num;
		}
		return 0;
	}

	private static float ConvertLoyaltyCostToSortValue(StringBackedInt loyaltyCost)
	{
		string rawText = loyaltyCost.RawText;
		if (!(rawText == "-X"))
		{
			if (rawText == "+X")
			{
				return 3.5f;
			}
			return loyaltyCost.Value;
		}
		return -3.5f;
	}
}
