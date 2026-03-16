using System.Collections.Generic;
using GreClient.CardData;

public class PerpetualModificationComparer : IComparer<ICardDataAdapter>
{
	public int Compare(ICardDataAdapter x, ICardDataAdapter y)
	{
		bool flag = x.HasPerpetualChanges();
		bool flag2 = y.HasPerpetualChanges();
		if (flag2 && !flag)
		{
			return -1;
		}
		if (flag && !flag2)
		{
			return 1;
		}
		return 0;
	}
}
