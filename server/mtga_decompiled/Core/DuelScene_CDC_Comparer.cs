using System.Collections.Generic;
using System.Linq;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

public class DuelScene_CDC_Comparer : IComparer<DuelScene_CDC>
{
	private readonly IReadOnlyList<DuelScene_CDC> _selectable;

	private CardModelComparer _cardModelComparer;

	public DuelScene_CDC_Comparer(IReadOnlyList<DuelScene_CDC> selectable, IGreLocProvider locManager)
	{
		_selectable = selectable;
		_cardModelComparer = new CardModelComparer(locManager);
	}

	public int Compare(DuelScene_CDC x, DuelScene_CDC y)
	{
		if (x == null && y == null)
		{
			return 0;
		}
		if (x == null)
		{
			return -1;
		}
		if (y == null)
		{
			return 1;
		}
		bool flag = _selectable.Contains(x);
		bool flag2 = _selectable.Contains(y);
		if (flag && !flag2)
		{
			return -1;
		}
		if (flag2 && !flag)
		{
			return 1;
		}
		if (x.Model.Visibility != Visibility.None && y.Model.Visibility == Visibility.None)
		{
			return -1;
		}
		if (x.Model.Visibility == Visibility.None && y.Model.Visibility != Visibility.None)
		{
			return 1;
		}
		return _cardModelComparer.Compare(x.Model, y.Model);
	}
}
