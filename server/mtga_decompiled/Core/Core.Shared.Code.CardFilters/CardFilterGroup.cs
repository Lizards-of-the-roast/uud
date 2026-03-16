using System;
using System.Collections.Generic;
using GreClient.CardData;

namespace Core.Shared.Code.CardFilters;

public class CardFilterGroup
{
	public class FilteredCard
	{
		public readonly int Index;

		public readonly CardPrintingData Card;

		private readonly bool _defaultPassed;

		private readonly FilteredReason _defaultFailReason;

		public bool PassedFilter { get; private set; }

		public FilteredReason FailReason { get; private set; }

		public FilteredCard(CardPrintingData card, int id = 0, bool defaultPassed = true, FilteredReason failReason = FilteredReason.None)
		{
			Card = card;
			Index = id;
			_defaultPassed = defaultPassed;
			PassedFilter = defaultPassed;
			_defaultFailReason = failReason;
			FailReason = failReason;
		}

		public FilteredCard(FilteredCard card, bool copyPassed = true)
		{
			Card = card.Card;
			Index = card.Index;
			if (copyPassed)
			{
				_defaultPassed = card._defaultPassed;
				_defaultFailReason = card._defaultFailReason;
				PassedFilter = card.PassedFilter;
				FailReason = card.FailReason;
			}
			else
			{
				_defaultPassed = card._defaultPassed;
				_defaultFailReason = card._defaultFailReason;
				PassedFilter = _defaultPassed;
				FailReason = _defaultFailReason;
			}
		}

		public void FailFilter(FilteredReason failReason = FilteredReason.None)
		{
			PassedFilter = false;
			FailReason |= failReason;
		}

		public void ResetCardState()
		{
			FailReason = _defaultFailReason;
			PassedFilter = _defaultPassed;
		}
	}

	[Flags]
	public enum FilteredReason
	{
		None = 0,
		NotLand = 1,
		IsLandUnlimited = 2,
		IsLand = 4,
		Format = 8,
		Commander_Color = 0x10,
		Commander_NotPartner = 0x20,
		Commander_Hidden = 0x40,
		Companion_Check = 0x80,
		Quantity = 0x100,
		StringFilter_Type = 0x200,
		StringFilter_ExpansionCode = 0x400,
		StringFilter_Title = 0x800,
		StringFilter_Power = 0x1000,
		StringFilter_Toughness = 0x2000,
		StringFilter_ManaCost = 0x4000,
		StringFilter_Artist = 0x8000,
		StringFilter_Flavor = 0x10000,
		StringFilter_AnyText = 0x20000,
		StringFilter_Rebalanced = 0x40000,
		StringFilter_Rules = 0x80000,
		StringFilter_Nickname = 0x100000,
		TraitFilter = 0x200000,
		NumericFilter = 0x400000,
		NormalizedCommon = 0x800000,
		ColorFilter = 0x1000000,
		CardTypeFilter = 0x2000000,
		CardSuperTypeFilter = 0x4000000
	}

	public readonly IReadOnlyList<FilteredCard> Cards;

	public CardFilterGroup(IReadOnlyList<FilteredCard> cards)
	{
		Cards = cards;
	}

	public bool AnyPassed()
	{
		for (int i = 0; i < Cards.Count; i++)
		{
			if (Cards[i].PassedFilter)
			{
				return true;
			}
		}
		return false;
	}

	public List<FilteredCard> GetFilteredCards_Passed()
	{
		List<FilteredCard> list = new List<FilteredCard>();
		for (int i = 0; i < Cards.Count; i++)
		{
			if (Cards[i].PassedFilter)
			{
				list.Add(Cards[i]);
			}
		}
		return list;
	}

	public void ResetAllCardStates()
	{
		foreach (FilteredCard card in Cards)
		{
			card.ResetCardState();
		}
	}
}
