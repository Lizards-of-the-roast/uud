using System.Collections.Generic;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

public class LandVsNonlandComparer : IComparer<IReadOnlyCollection<CardType>>
{
	public int Compare(IReadOnlyCollection<CardType> x, IReadOnlyCollection<CardType> y)
	{
		bool flag = x.Contains(CardType.Land);
		bool flag2 = y.Contains(CardType.Land);
		if (!flag && flag2)
		{
			return -1;
		}
		if (!flag2 && flag)
		{
			return 1;
		}
		return 0;
	}
}
