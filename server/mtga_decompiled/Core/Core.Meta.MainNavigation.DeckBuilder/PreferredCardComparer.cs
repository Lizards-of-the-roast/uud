using System.Collections.Generic;
using Wizards.Mtga;
using Wizards.Mtga.PreferredPrinting;

namespace Core.Meta.MainNavigation.DeckBuilder;

public class PreferredCardComparer : IComparer<CardOrStyleEntry>
{
	public static readonly PreferredCardComparer Instance = new PreferredCardComparer();

	public static bool IsFavorite(in CardOrStyleEntry card)
	{
		PreferredPrintingWithStyle preferredPrintingForTitleId = Pantry.Get<IPreferredPrintingDataProvider>().GetPreferredPrintingForTitleId((int)card.PrintingQuantity.Printing.TitleId);
		if (preferredPrintingForTitleId == null)
		{
			return false;
		}
		string text = card.StyleInformation?.StyleCode;
		if (preferredPrintingForTitleId.printingGrpId == card.PrintingQuantity.Printing.GrpId)
		{
			return preferredPrintingForTitleId.styleCode == text;
		}
		return false;
	}

	private static bool IsStyle(in CardOrStyleEntry card)
	{
		return card.StyleInformation.HasValue;
	}

	private bool IsOwned(in CardOrStyleEntry card)
	{
		if (card.StyleInformation.HasValue)
		{
			return card.StyleInformation.Value.IsOwnedStyle;
		}
		return card.PrintingQuantity.Quantity != 0;
	}

	private static uint NewestPrinting(in CardOrStyleEntry card)
	{
		return card.PrintingQuantity.Printing.GrpId;
	}

	public int Compare(CardOrStyleEntry x, CardOrStyleEntry y)
	{
		int num = IsFavorite(in x).CompareTo(IsFavorite(in y));
		if (num == 0)
		{
			num = IsStyle(in y).CompareTo(IsStyle(in x));
		}
		if (num == 0)
		{
			num = IsOwned(in x).CompareTo(IsOwned(in y));
		}
		if (num == 0)
		{
			num = NewestPrinting(in x).CompareTo(NewestPrinting(in y));
		}
		return num;
	}
}
