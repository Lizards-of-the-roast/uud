using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.Diagnostics;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_Crash : AutoPlayAction
{
	private enum CustomCrashCategory
	{
		None,
		EndlessLoop,
		Exception,
		SegFault,
		OOM
	}

	private CustomCrashCategory _customCrash;

	private ForcedCrashCategory _crashCategory;

	private List<int> _oomList;

	protected override void OnInitialize(in string[] parameters, int index)
	{
		string text = AutoPlayAction.FromParameter(in parameters, index + 1);
		_customCrash = text.IntoEnum<CustomCrashCategory>();
		_crashCategory = text.IntoEnum<ForcedCrashCategory>();
	}

	protected override void OnExecute()
	{
		switch (_customCrash)
		{
		case CustomCrashCategory.EndlessLoop:
			while (true)
			{
			}
		case CustomCrashCategory.Exception:
			throw new UnauthorizedAccessException();
		case CustomCrashCategory.SegFault:
			Marshal.ReadInt32(IntPtr.Zero);
			break;
		case CustomCrashCategory.OOM:
			_oomList = new List<int> { 0 };
			break;
		case CustomCrashCategory.None:
			Utils.ForceCrash(_crashCategory);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	protected override void OnUpdate()
	{
		if (_customCrash == CustomCrashCategory.OOM)
		{
			_oomList.AddRange(_oomList);
		}
	}
}
