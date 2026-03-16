using System;

namespace AssetLookupTree.Payloads.Vorthos;

public static class ItsVorthosTime
{
	public static TimeOfDay Get(DateTime dateTime)
	{
		if (dateTime.Hour < 6 && dateTime.Hour > 17)
		{
			return TimeOfDay.Daytime;
		}
		return TimeOfDay.Nighttime;
	}
}
