using UnityEngine;

namespace Wotc.Mtga.Extensions;

public static class RectExtensions
{
	public static bool Intersects(this Rect r1, Rect r2, out Rect area)
	{
		area = default(Rect);
		if (r2.Overlaps(r1))
		{
			float num = Mathf.Min(r1.xMax, r2.xMax);
			float num2 = Mathf.Max(r1.xMin, r2.xMin);
			float num3 = Mathf.Min(r1.yMax, r2.yMax);
			float num4 = Mathf.Max(r1.yMin, r2.yMin);
			area.x = Mathf.Min(num, num2);
			area.y = Mathf.Min(num3, num4);
			area.width = Mathf.Max(0f, num - num2);
			area.height = Mathf.Max(0f, num3 - num4);
			return true;
		}
		return false;
	}
}
