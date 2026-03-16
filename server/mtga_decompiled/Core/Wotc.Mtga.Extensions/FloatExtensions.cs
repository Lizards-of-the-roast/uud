using System.Collections.Generic;

namespace Wotc.Mtga.Extensions;

public static class FloatExtensions
{
	public sealed class NonDuplicationComparer : IComparer<float>
	{
		public static readonly IComparer<float> Default = new NonDuplicationComparer();

		public int Compare(float x, float y)
		{
			int num = x.CompareTo(y);
			if (num != 0)
			{
				return num;
			}
			return 1;
		}
	}
}
