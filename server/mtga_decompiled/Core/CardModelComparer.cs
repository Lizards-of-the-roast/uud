using System;
using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;

public class CardModelComparer : IComparer<ICardDataAdapter>
{
	private int returnValue;

	private IGreLocProvider _locManager;

	private CardColorCombinationComparer _cardColorCombinationComparer;

	private LandVsNonlandComparer _landVsNonlandComparer;

	private XSpellVsNonXSpellComparer _xSpellVsNonXSpellComparer;

	private PerpetualModificationComparer _perpetualModificationComparer;

	public CardModelComparer(IGreLocProvider locManager)
	{
		_locManager = locManager;
		_cardColorCombinationComparer = new CardColorCombinationComparer();
		_landVsNonlandComparer = new LandVsNonlandComparer();
		_xSpellVsNonXSpellComparer = new XSpellVsNonXSpellComparer();
		_perpetualModificationComparer = new PerpetualModificationComparer();
	}

	public int Compare(ICardDataAdapter x, ICardDataAdapter y)
	{
		returnValue = _landVsNonlandComparer.Compare(x.CardTypes, y.CardTypes);
		if (returnValue != 0)
		{
			return returnValue;
		}
		returnValue = _xSpellVsNonXSpellComparer.Compare(x.PrintedCastingCost, y.PrintedCastingCost);
		if (returnValue != 0)
		{
			return returnValue;
		}
		if (x.ConvertedManaCost > y.ConvertedManaCost)
		{
			return -1;
		}
		if (y.ConvertedManaCost > x.ConvertedManaCost)
		{
			return 1;
		}
		returnValue = _cardColorCombinationComparer.Compare(x.Colors, y.Colors);
		if (returnValue != 0)
		{
			return returnValue;
		}
		int num = string.Compare(_locManager.GetLocalizedText(y.Printing.TitleId), _locManager.GetLocalizedText(x.Printing.TitleId), StringComparison.CurrentCulture);
		if (num < 0)
		{
			return 1;
		}
		if (num > 0)
		{
			return -1;
		}
		returnValue = _perpetualModificationComparer.Compare(x, y);
		if (returnValue != 0)
		{
			return returnValue;
		}
		return 0;
	}
}
