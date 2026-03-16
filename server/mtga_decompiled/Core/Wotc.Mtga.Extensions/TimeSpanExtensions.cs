using System;

namespace Wotc.Mtga.Extensions;

public static class TimeSpanExtensions
{
	public static string To_HH_MM(this TimeSpan ts)
	{
		if (ts < TimeSpan.Zero || ts.TotalMinutes < 1.0)
		{
			return "<0:01";
		}
		return $"{(int)ts.TotalHours}:{ts.Minutes:00}";
	}

	public static string To_HH_MM_SS(this TimeSpan ts)
	{
		if (ts < TimeSpan.Zero)
		{
			return "0:00:00";
		}
		return $"{(int)ts.TotalHours}:{ts.Minutes:00}:{ts.Seconds:00}";
	}
}
