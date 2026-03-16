using System;
using System.Collections.Generic;
using Core.Meta.MainNavigation.Cosmetics;

namespace ProfileUI;

public class TitleListViewItemSorter : IComparer<TitleListViewItem>
{
	public int Compare(TitleListViewItem x, TitleListViewItem y)
	{
		if ((object)x == y)
		{
			return 0;
		}
		if ((object)y == null)
		{
			return 1;
		}
		if ((object)x == null)
		{
			return -1;
		}
		int num = (y.TitleData.Id == "NoTitle").CompareTo(x.TitleData.Id == "NoTitle");
		if (num != 0)
		{
			return num;
		}
		int num2 = y.IsOwned.CompareTo(x.IsOwned);
		if (num2 != 0)
		{
			return num2;
		}
		return string.Compare(x.LocalizedText, y.LocalizedText, StringComparison.Ordinal);
	}
}
