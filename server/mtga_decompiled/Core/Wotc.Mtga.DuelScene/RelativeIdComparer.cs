using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public class RelativeIdComparer : IComparer<uint>
{
	private readonly uint _id;

	public RelativeIdComparer(uint id)
	{
		_id = id;
	}

	public int Compare(uint x, uint y)
	{
		bool flag = x > _id;
		bool flag2 = y > _id;
		if (flag == flag2)
		{
			return x.CompareTo(y);
		}
		if (!flag)
		{
			return 1;
		}
		return -1;
	}
}
