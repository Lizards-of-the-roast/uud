using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public class CounterSortingDataComparer : IComparer<CDCPart_Counters.CounterSortingData>
{
	public int Compare(CDCPart_Counters.CounterSortingData x, CDCPart_Counters.CounterSortingData y)
	{
		return x.CounterCategory.CompareTo(y.CounterCategory);
	}
}
