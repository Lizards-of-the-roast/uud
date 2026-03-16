using UnityEngine;

namespace Wizards.Mtga.UI;

public static class ScreenKeepAlive
{
	public static void KeepScreenAwake()
	{
		Screen.sleepTimeout = -1;
	}

	public static void AllowScreenTimeout()
	{
		Screen.sleepTimeout = -2;
	}
}
