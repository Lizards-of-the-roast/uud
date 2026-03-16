using System;

namespace Wotc.Mtga.Login;

public static class AgeGateUtils
{
	public static TimeSpan GateDuration { get; private set; } = new TimeSpan(0, 2, 0, 0);

	public static bool IsGatedDueToAge()
	{
		return MDNPlayerPrefs.AgeGateTime + GateDuration > DateTime.UtcNow;
	}

	public static void GateUserFromLoginDueToAge()
	{
		MDNPlayerPrefs.AgeGateTime = DateTime.UtcNow;
	}
}
