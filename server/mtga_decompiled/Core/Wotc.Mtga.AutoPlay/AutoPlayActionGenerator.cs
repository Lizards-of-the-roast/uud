using System;
using System.Collections.Generic;
using Wotc.Mtga.AutoPlay.AutoPlayActions;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.AutoPlay;

public static class AutoPlayActionGenerator
{
	private static readonly Dictionary<string, Func<AutoPlayAction>> Generator = new Dictionary<string, Func<AutoPlayAction>>
	{
		{
			"Click",
			() => new AutoPlayAction_Click()
		},
		{
			"ClickDown",
			() => new AutoPlayAction_Click()
		},
		{
			"Crash",
			() => new AutoPlayAction_Crash()
		},
		{
			"BotBattleTest",
			() => new AutoPlayAction_BotBattleTest()
		},
		{
			"FPSReport",
			() => new AutoPlayAction_FPSReport()
		},
		{
			"HangingRefs",
			() => new AutoPlayAction_HangingRefs()
		},
		{
			"Hover",
			() => new AutoPlayAction_Hover()
		},
		{
			"Include",
			() => new AutoPlayAction_Include()
		},
		{
			"InputText",
			() => new AutoPlayAction_InputText()
		},
		{
			"Language",
			() => new AutoPlayAction_Language()
		},
		{
			"LoadScene",
			() => new AutoPlayAction_LoadScene()
		},
		{
			"Log",
			() => new AutoPlayAction_Log()
		},
		{
			"Login",
			() => new AutoPlayAction_Login()
		},
		{
			"Logout",
			() => new AutoPlayAction_Logout()
		},
		{
			"MemoryReport",
			() => new AutoPlayAction_MemoryReport()
		},
		{
			"ObjectMethod",
			() => new AutoPlayAction_ObjectMethod()
		},
		{
			"Screenshot",
			() => new AutoPlayAction_Screenshot()
		},
		{
			"PlayerPref",
			() => new AutoPlayAction_SetPlayerPref()
		},
		{
			"UserPref",
			() => new AutoPlayAction_SetUserPref()
		},
		{
			"Replay",
			() => new AutoPlayAction_Replay()
		},
		{
			"TimeScale",
			() => new AutoPlayAction_TimeScale()
		},
		{
			"WaitGame",
			() => new AutoPlayAction_WaitGame()
		},
		{
			"WaitScene",
			() => new AutoPlayAction_WaitScene()
		},
		{
			"WaitTime",
			() => new AutoPlayAction_WaitTime()
		},
		{
			"BreakIfScene",
			() => new AutoPlayAction_BreakIfScene()
		},
		{
			"ReplayScript",
			() => new AutoPlayAction_TimedReplay()
		},
		{
			"LoadTimedReplay",
			() => new AutoPlayAction_TimedReplay()
		},
		{
			"PlayTimedReplay",
			() => new AutoPlayAction_PlayTimedReplay()
		},
		{
			"AnimatorTrigger",
			() => new AutoPlayAction_Animator()
		},
		{
			"Dropdown",
			() => new AutoPlayAction_DropDown()
		},
		{
			"ClearPlayerPref",
			() => new AutoPlayAction_SetPlayerPref()
		},
		{
			"ClearUserPref",
			() => new AutoPlayAction_SetUserPref()
		},
		{
			"AutoRegister",
			() => new AutoPlayAction_AutoRegister()
		},
		{
			"WaitScriptReplay",
			() => new AutoPlayAction_WaitTimedReplay()
		},
		{
			"WaitTimedReplay",
			() => new AutoPlayAction_WaitTimedReplay()
		},
		{
			"DeleteBundles",
			() => new AutoPlayAction_DeleteBundles()
		},
		{
			"EventTrigger",
			() => new AutoPlayAction_EventTrigger()
		},
		{
			"Fail",
			() => new AutoPlayAction_Fail()
		}
	};

	public static AutoPlayAction AutoPlayActionFromString(string filename, int lineNumber, string[] lineParts, Action<string> logAction, AutoPlayComponentGetters componentGetters, AutoPlayManager autoPlayManager)
	{
		int num = 0;
		float delay = 0f;
		bool isOptional = false;
		if (lineParts.Length != 0 && lineParts[0].Length > 0 && lineParts[0][0] == '@')
		{
			delay = lineParts[0].Substring(1).IntoFloat();
			num = 1;
		}
		if (lineParts.Length > num && lineParts[num].Length > 0 && lineParts[num] == "Try")
		{
			isOptional = true;
			num++;
		}
		if (num >= lineParts.Length)
		{
			return null;
		}
		AutoPlayAction obj = Generator.GetValueOrDefault(in lineParts[num])?.Invoke();
		if (obj != null)
		{
			obj.Initialize(filename, lineNumber, logAction, componentGetters, delay, isOptional, in lineParts, num, autoPlayManager);
			return obj;
		}
		return obj;
	}
}
