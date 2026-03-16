using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using Wotc.Mtgo.Gre.External.Messaging;

public class XSpellVsNonXSpellComparer : IComparer<IReadOnlyList<ManaQuantity>>
{
	public int Compare(IReadOnlyList<ManaQuantity> x, IReadOnlyList<ManaQuantity> y)
	{
		bool flag = x.Contains(new ManaQuantity(1u, ManaColor.X));
		bool flag2 = y.Contains(new ManaQuantity(1u, ManaColor.X));
		if (flag2 && !flag)
		{
			return 1;
		}
		if (flag && !flag2)
		{
			return -1;
		}
		return 0;
	}
}
