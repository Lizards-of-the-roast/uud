using System.Collections.Generic;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtgo.Gre.External.Messaging;

public class CardColorCombinationComparer : IComparer<IReadOnlyCollection<CardColor>>
{
	public int Compare(IReadOnlyCollection<CardColor> x, IReadOnlyCollection<CardColor> y)
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
		int num = BrowserConstants.BROWSER_COLOR_COMBINATION_ORDER.Count;
		int num2 = BrowserConstants.BROWSER_COLOR_COMBINATION_ORDER.Count;
		for (int i = 0; i < BrowserConstants.BROWSER_COLOR_COMBINATION_ORDER.Count; i++)
		{
			if (BrowserConstants.BROWSER_COLOR_COMBINATION_ORDER[i].SetEquals(x))
			{
				num = i;
			}
			if (BrowserConstants.BROWSER_COLOR_COMBINATION_ORDER[i].SetEquals(y))
			{
				num2 = i;
			}
		}
		if (num < num2)
		{
			return -1;
		}
		if (num2 < num)
		{
			return 1;
		}
		return 0;
	}
}
